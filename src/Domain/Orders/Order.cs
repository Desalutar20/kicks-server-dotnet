using Domain.Abstractions;
using Domain.DeliveryOptions;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;
using Domain.Users;

namespace Domain.Orders;

public class Order : Entity<OrderId>
{
    public const int ExpirationMaxMinutes = 30;
    public const int MaxPaymentsCount = 10;

    public Email Email { get; private set; } = null!;
    public NonEmptyString PhoneNumber { get; private set; } = null!;
    public OrderAddress? BillingAddress { get; private set; }
    public OrderAddress DeliveryAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public Money DeliveryPrice { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }

    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyList<OrderItem> OrderItems => _orderItems;

    private readonly List<OrderPayment> _orderPayments = [];
    public IReadOnlyList<OrderPayment> OrderPayments => _orderPayments;

    public UserId UserId { get; private set; } = null!;

    public DeliveryOptionId DeliveryOptionId { get; private set; } = null!;
    public DeliveryOption DeliveryOption { get; private set; } = null!;

    public PromocodeId? PromocodeId { get; private set; }
    public Promocode? Promocode { get; private set; }

    public Money Total
    {
        get
        {
            var subtotalCents = _orderItems.Sum(x => x.Price.Cents * x.Quantity);
            var subtotal = Money.FromCents(subtotalCents).Value;

            if (Promocode is not null)
            {
                var discount = Promocode.CalculateDiscount(subtotal);
                subtotal -= discount;
            }

            if (DeliveryPrice.Cents > 0)
            {
                subtotal += DeliveryPrice;
            }

            return subtotal;
        }
    }

    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;

    private Order()
        : base(new OrderId(Guid.NewGuid())) { }

    private Order(
        Email email,
        NonEmptyString phoneNumber,
        OrderAddress? billingAddress,
        OrderAddress deliveryAddress,
        DateTimeOffset expiresAt,
        List<OrderItem> orderItems,
        UserId userId,
        DeliveryOption deliveryOption,
        Promocode? promocode
    )
        : base(new OrderId(Guid.NewGuid()))
    {
        Email = email;
        PhoneNumber = phoneNumber;
        BillingAddress = billingAddress;
        DeliveryAddress = deliveryAddress;
        ExpiresAt = expiresAt;
        _orderItems = orderItems;
        UserId = userId;
        DeliveryOption = deliveryOption;
        DeliveryPrice = deliveryOption.Price;
        DeliveryOptionId = deliveryOption.Id;
        PromocodeId = promocode?.Id;
        Promocode = promocode;
    }

    public Result AddPayment(OrderPayment payment)
    {
        if (IsExpired)
        {
            return Error.Failure("Order has expired.");
        }

        if (Status != OrderStatus.Pending)
        {
            return Error.Failure($"Order status is '{Status}'. Only Pending orders can be paid.");
        }

        if (_orderPayments.Any(p => p.Status == OrderPaymentStatus.Completed))
        {
            return Error.Failure("Order already has a completed payment.");
        }

        if (_orderPayments.Count >= MaxPaymentsCount)
        {
            return Error.Failure("Payment attempts limit reached.");
        }

        _orderPayments.Add(payment);

        return Result.Success();
    }

    public static Result<Order> Create(
        Email email,
        NonEmptyString phoneNumber,
        OrderAddress? billingAddress,
        OrderAddress deliveryAddress,
        DateTimeOffset expiresAt,
        List<OrderItem> orderItems,
        UserId userId,
        DeliveryOption deliveryOption,
        Promocode? promocode
    )
    {
        if (orderItems == null || orderItems.Count == 0)
        {
            return Error.Internal("Order must contain at least one item.");
        }

        var now = DateTimeOffset.UtcNow;
        var diff = expiresAt - now;

        if (expiresAt <= now)
            return Error.Internal("Order expiration must be in the future.");

        if (diff > TimeSpan.FromMinutes(ExpirationMaxMinutes))
            return Error.Internal(
                $"Order expiration cannot exceed {ExpirationMaxMinutes} minutes from now."
            );

        return new Order(
            email,
            phoneNumber,
            billingAddress,
            deliveryAddress,
            expiresAt,
            orderItems,
            userId,
            deliveryOption,
            promocode
        );
    }
}

using System.Text.Json.Serialization;

namespace Domain.Orders;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderPaymentStatus
{
    Pending,
    Completed,
    Failed,
    Expired,
    Refunded,
    Cancelled,
}

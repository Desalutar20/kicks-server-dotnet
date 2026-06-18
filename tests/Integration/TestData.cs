using Application.Admin.Products.ProductSkus.Constants;
using Domain.Brands;
using Domain.Categories;
using Domain.DeliveryOptions;
using Domain.Products;
using Domain.Products.ProductSkus;
using Domain.Promocodes;
using Domain.Shared.FileContent;
using Domain.Shared.ValueObjects;
using Microsoft.AspNetCore.Http;
using Presentation.Admin.Products.Endpoints;
using Presentation.Admin.Products.ProductSkus.Endpoints;
using Presentation.Admin.Promocodes.Dto;
using Presentation.Admin.Promocodes.Endpoints;

namespace Integration;

public static class TestData
{
    private static readonly Faker Faker = new();

    public static SignUpRequest SignUpRequest() =>
        new Faker<SignUpRequest>()
            .CustomInstantiator(f => new SignUpRequest(
                f.Internet.Email(),
                f.Internet.Password(),
                f.Name.FirstName(),
                f.Name.LastName(),
                "male"
            ))
            .Generate();

    public static CreateProductRequest CreateProductRequest(string categoryId, string brandId) =>
        new Faker<CreateProductRequest>()
            .CustomInstantiator(f => new CreateProductRequest(
                f.Commerce.ProductName(),
                f.Commerce.ProductDescription(),
                "men",
                [],
                categoryId,
                brandId
            ))
            .Generate();

    public static CreateProductSkuRequest CreateProductSkuRequest(int imagesCount = 1)
    {
        return new Faker<CreateProductSkuRequest>()
            .CustomInstantiator(f =>
            {
                var request = new CreateProductSkuRequest
                {
                    Price = f.Random.Int(10, 1000),
                    SalePrice = f.Random.Bool() ? f.Random.Int(1, 9) : null,
                    Quantity = f.Random.Int(1, 100),
                    Size = f.Random.Int(36, 46),
                    Color = f.Internet.Color(),
                    Sku = String(ProductSkuSku.MaxLength),
                    Images = CreateImages(imagesCount, ProductSkusConstants.MaxFileSizeBytes),
                };

                return request;
            })
            .Generate();
    }

    public static CreatePromocodeRequest CreatePromocodeRequest() =>
        new Faker<CreatePromocodeRequest>()
            .CustomInstantiator(f =>
            {
                var type = f.PickRandom<PromocodeType>();

                var discountValue =
                    type == PromocodeType.Percent ? f.Random.Int(1, 99) : f.Random.Int(100, 10_000);

                return new CreatePromocodeRequest(
                    DiscountValue: discountValue,
                    Type: type.ToString(),
                    ValidityPeriod: new PromocodeValidityPeriodDto(
                        ValidFrom: DateTimeOffset.UtcNow,
                        ValidTo: DateTimeOffset.UtcNow.AddDays(30)
                    ),
                    UsageLimit: f.Random.Int(1, 1000),
                    Code: f.Random.AlphaNumeric(10).ToUpper()
                );
            })
            .Generate();

    public static FormFileCollection CreateImages(int count = 1, long fileSizeBytes = 1024)
    {
        var files = new FormFileCollection();

        for (var i = 0; i < count; i++)
        {
            var bytes = CreateImage(fileSizeBytes);

            var file = new FormFile(
                new MemoryStream(bytes),
                0,
                bytes.Length,
                "images",
                $"image_{i}.png"
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = (i % 3) switch
                {
                    0 => "image/png",
                    1 => "image/jpeg",
                    _ => "image/webp",
                },
            };

            files.Add(file);
        }

        return files;
    }

    private static byte[] CreateImage(long sizeBytes)
    {
        var baseImage = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+XW1sAAAAASUVORK5CYII="
        );

        if (sizeBytes <= baseImage.Length)
        {
            return baseImage;
        }

        var result = new byte[sizeBytes];

        Buffer.BlockCopy(baseImage, 0, result, 0, baseImage.Length);

        return result;
    }

    public static string Email() => Faker.Internet.Email();

    public static string String(int length) => Faker.Commerce.Random.AlphaNumeric(length);

    public static string Password(int length = 10) => Faker.Internet.Password(length);

    public static List<User> SeedUsers() =>
        Enumerable
            .Range(1, 200)
            .Select(i =>
            {
                var email = Domain.Shared.ValueObjects.Email.Create($"user{i}@test.local").Value;
                var hashedPassword = HashedPassword
                    .Create(Password(HashedPassword.MinLength))
                    .Value;

                return new User(email, hashedPassword, null, null, null, null, null);
            })
            .ToList();

    public static List<Brand> SeedBrands() =>
        Enumerable
            .Range(0, 59)
            .Select(_ => new Brand(BrandName.Create(String(BrandName.MaxLength)).Value))
            .ToList();

    public static List<Category> SeedCategories() =>
        Enumerable
            .Range(0, 50)
            .Select(_ => new Category(CategoryName.Create(String(CategoryName.MaxLength)).Value))
            .ToList();

    public static List<Product> SeedProducts(List<Brand> brands, List<Category> categories)
    {
        var faker = new Faker();

        return
        [
            .. brands.SelectMany(brand =>
                categories.Select(category =>
                {
                    var title = ProductTitle.Create(faker.Commerce.ProductName()).Value;

                    var description = ProductDescription
                        .Create(faker.Commerce.ProductDescription())
                        .Value;

                    var gender = faker.PickRandom<ProductGender>();

                    var tags = ProductTags
                        .Create([
                            .. Enumerable
                                .Range(0, faker.Random.Int(1, ProductTags.MaxTags))
                                .Select(_ => faker.Commerce.ProductAdjective())
                                .Distinct(),
                        ])
                        .Value;

                    return new Product(title, description, gender, tags, brand.Id, category.Id);
                })
            ),
        ];
    }

    public static List<ProductSku> SeedProductSkus(List<Product> products)
    {
        var faker = new Faker();
        var sizes = Enumerable.Range(35, 5).ToList();

        return
        [
            .. products
                .Take(200)
                .SelectMany(product =>
                    sizes
                        .Select(s =>
                        {
                            var priceResult = ProductSkuPrice.Create(
                                Money.FromCents(250 + s * 10).Value,
                                s % 2 == 0 ? Money.FromCents(150 + s * 10).Value : null
                            );

                            if (priceResult.IsFailure)
                                return null;

                            var quantityResult = PositiveInt.Create(100);
                            if (quantityResult.IsFailure)
                                return null;

                            var sizeResult = PositiveInt.Create(s);
                            if (sizeResult.IsFailure)
                                return null;

                            var colorResult = ProductSkuColor.Create(faker.Internet.Color());
                            if (colorResult.IsFailure)
                                return null;

                            var skuResult = ProductSkuSku.Create(String(ProductSkuSku.MaxLength));
                            if (skuResult.IsFailure)
                                return null;

                            var images = Enumerable
                                .Range(0, 3)
                                .Select(_ =>
                                {
                                    var urlResult = FileUrl.Create(faker.Image.PlaceImgUrl());
                                    if (urlResult.IsFailure)
                                        return null;

                                    var nameResult = FileName.Create(faker.System.CommonFileName());
                                    if (nameResult.IsFailure)
                                        return null;

                                    return ProductSkuImage.Create(
                                        Guid.NewGuid(),
                                        urlResult.Value,
                                        nameResult.Value
                                    );
                                })
                                .Where(x => x is not null)
                                .ToList();

                            var productSkuResult = ProductSku.Create(
                                priceResult.Value,
                                quantityResult.Value,
                                colorResult.Value,
                                skuResult.Value,
                                sizeResult.Value,
                                images!,
                                product.Id
                            );

                            if (productSkuResult.IsFailure)
                                return null;

                            var productSku = productSkuResult.Value;

                            return productSku;
                        })
                        .Where(x => x is not null)
                        .Select(x => x!)
                ),
        ];
    }

    public static List<Promocode> SeedPromocodes()
    {
        var faker = new Faker<Promocode>().CustomInstantiator(f =>
        {
            var type = f.PickRandom<PromocodeType>();

            var discountValue =
                type == PromocodeType.Percent ? f.Random.Int(1, 99) : f.Random.Int(100, 10_000);

            var validFrom = DateTimeOffset.UtcNow;
            var validTo = validFrom.AddDays(f.Random.Int(1, 365));

            return Promocode
                .Create(
                    PositiveInt.Create(discountValue).Value,
                    type,
                    PromocodeValidityPeriod.Create(validFrom, validTo).Value,
                    PositiveInt.Create(f.Random.Int(1, 1000)).Value,
                    PromocodeCode.Create(f.Random.Replace("PROMO-????-####").ToUpper()).Value
                )
                .Value;
        });

        return faker.Generate(100);
    }

    public static List<DeliveryOption> SeedDeliveryOptions()
    {
        return
        [
            new DeliveryOption(
                DeliveryOptionTitle.Create("Standard Delivery").Value,
                DeliveryOptionDescription.Create("Delivery within 3-5 business days.").Value,
                Money.FromCents(1500).Value
            ),
            new DeliveryOption(
                DeliveryOptionTitle.Create("Express Delivery").Value,
                DeliveryOptionDescription.Create("Delivery within 1-2 business days.").Value,
                Money.FromCents(3000).Value
            ),
            new DeliveryOption(
                DeliveryOptionTitle.Create("Same Day Delivery").Value,
                DeliveryOptionDescription
                    .Create("Delivered on the same day for eligible locations.")
                    .Value,
                Money.FromCents(5000).Value
            ),
            new DeliveryOption(
                DeliveryOptionTitle.Create("Store Pickup").Value,
                DeliveryOptionDescription.Create("Pick up your order from our store.").Value,
                Money.FromCents(0).Value
            ),
        ];
    }
}

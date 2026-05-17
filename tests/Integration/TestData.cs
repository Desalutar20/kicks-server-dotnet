using Application.Admin.Products.ProductSkus.Constants;
using Domain.Brand;
using Domain.Category;
using Domain.Product;
using Domain.Product.ProductSku;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Presentation.Admin.Products.Endpoints;
using Presentation.Admin.Products.ProductSkus.Endpoints;

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
                var request = new CreateProductSkuRequest()
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
            return baseImage;

        var result = new byte[sizeBytes];

        Buffer.BlockCopy(baseImage, 0, result, 0, baseImage.Length);

        return result;
    }

    public static string Email() => Faker.Internet.Email();

    public static string String(int length) => Faker.Commerce.Random.AlphaNumeric(length);

    public static string Password(int length = 10) => Faker.Internet.Password(length);

    public static List<User> SeedUsers()
    {
        var faker = new Faker<User>().CustomInstantiator(f =>
        {
            var email = Domain.User.Email.Create(f.Internet.Email()).Value;
            var hashedPassword = HashedPassword.Create(Password(HashedPassword.MinLength)).Value;

            return User.Create(email, hashedPassword, null, null, null, null, null);
        });

        return faker.Generate(200);
    }

    public static List<Brand> SeedBrands() =>
        Enumerable
            .Range(0, 59)
            .Select(_ => Brand.Create(BrandName.Create(String(BrandName.MaxLength)).Value))
            .ToList();

    public static List<Category> SeedCategories() =>
        Enumerable
            .Range(0, 50)
            .Select(_ => Category.Create(CategoryName.Create(String(CategoryName.MaxLength)).Value))
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

                    return Product.Create(title, description, gender, tags, brand.Id, category.Id);
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
                    sizes.Select(s =>
                    {
                        var price = ProductSkuPrice
                            .Create(
                                PositiveInt.Create(250 + (s * 10)).Value,
                                s % 2 == 0 ? PositiveInt.Create(150 + (s * 10)).Value : null
                            )
                            .Value;

                        var quantity = PositiveInt.Create(100).Value;
                        var size = PositiveInt.Create(s).Value;
                        var color = ProductSkuColor.Create(faker.Internet.Color()).Value;
                        var sku = ProductSkuSku.Create(String(ProductSkuSku.MaxLength)).Value;

                        return ProductSku.Create(price, quantity, color, sku, size, product.Id);
                    })
                ),
        ];
    }
}

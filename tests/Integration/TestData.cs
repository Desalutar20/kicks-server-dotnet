using Domain.Product;
using Domain.Product.Brand;
using Domain.Product.Category;
using Presentation.Admin.Products.Endpoints;

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
}

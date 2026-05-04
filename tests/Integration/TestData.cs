using Domain.Product.Brand;
using Domain.Product.Category;

namespace Integration;

public static class TestData
{
    private static readonly Faker Faker = new();

    private static readonly Faker<SignUpRequest> SignUpFaker =
        new Faker<SignUpRequest>().CustomInstantiator(f => new SignUpRequest(
            f.Internet.Email(),
            f.Internet.Password(),
            f.Name.FirstName(),
            f.Name.LastName(),
            "male"
        ));

    public static SignUpRequest SignUpRequest() => SignUpFaker.Generate();

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
}

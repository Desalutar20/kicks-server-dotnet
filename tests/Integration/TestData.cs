namespace Integration;

public static class TestData
{
    private static readonly Faker Faker = new();

    private static readonly Faker<SignUpRequest> SignUpFaker = new Faker<SignUpRequest>()
        .CustomInstantiator(f => new SignUpRequest(
            f.Internet.Email(),
            f.Internet.Password(),
            f.Name.FirstName(),
            f.Name.LastName(),
            "male"
        ));

    public static SignUpRequest SignUpRequest() => SignUpFaker.Generate();

    public static string Email() => Faker.Internet.Email();
    public static string Password(int length = 10) => Faker.Internet.Password(length);
}
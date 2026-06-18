using Domain.DeliveryOptions;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.DeliveryOptions.Endpoints;

namespace Integration.Admin.DeliveryOptions;

public sealed class CreateDeliveryOptionTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var createDeliveryOptionRequest = new CreateDeliveryOptionRequest(
            TestData.String(DeliveryOptionTitle.MaxLength),
            TestData.String(DeliveryOptionDescription.MaxLength),
            100
        );
        var response = await CreateDeliveryOption(createDeliveryOptionRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var deliveryOption = await GetDeliveryOptionFromDbByTitle(
            DeliveryOptionTitle.Create(createDeliveryOptionRequest.Title).Value,
            ct
        );

        deliveryOption.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        CreateDeliveryOptionRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await CreateDeliveryOption(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_DeliveryOptionAlreadyExists()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var createDeliveryOptionRequest = new CreateDeliveryOptionRequest(
            TestData.String(DeliveryOptionTitle.MaxLength),
            TestData.String(DeliveryOptionDescription.MaxLength),
            100
        );
        var response = await CreateDeliveryOption(createDeliveryOptionRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await CreateDeliveryOption(
            createDeliveryOptionRequest,
            sessionCookie,
            ct
        );
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new CreateDeliveryOptionRequest(
            TestData.String(DeliveryOptionTitle.MaxLength),
            TestData.String(DeliveryOptionDescription.MaxLength),
            100
        );

        var response = await CreateDeliveryOption(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var createDeliveryOptionRequest = new CreateDeliveryOptionRequest(
            TestData.String(DeliveryOptionTitle.MaxLength),
            TestData.String(DeliveryOptionDescription.MaxLength),
            100
        );
        var response = await CreateDeliveryOption(createDeliveryOptionRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, CreateDeliveryOptionRequest> InvalidRequests()
    {
        var request = new CreateDeliveryOptionRequest(
            TestData.String(DeliveryOptionTitle.MaxLength),
            TestData.String(DeliveryOptionDescription.MaxLength),
            100
        );

        return
        [
            ("title", request with { Title = "" }),
            ("title", request with { Title = "   " }),
            ("title", request with { Title = TestData.String(DeliveryOptionTitle.MaxLength + 1) }),
            ("description", request with { Description = "" }),
            ("description", request with { Description = "   " }),
            (
                "description",
                request with
                {
                    Description = TestData.String(DeliveryOptionDescription.MaxLength + 1),
                }
            ),
            ("price", request with { Price = -1 }),
        ];
    }
}

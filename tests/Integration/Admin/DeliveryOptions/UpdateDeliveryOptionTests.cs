using Domain.DeliveryOptions;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.DeliveryOptions.Dto;
using Presentation.Admin.DeliveryOptions.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.DeliveryOptions;

public class UpdateDeliveryOptionTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetDeliveryOptions(sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<AdminDeliveryOptionDto>>
        >(ct);
        body.Should().NotBeNull();

        var newTitle = TestData.String(DeliveryOptionTitle.MaxLength);
        var updateDeliveryOptionRequest = new UpdateDeliveryOptionRequest(newTitle, null, null);
        var updateDeliveryOptionResponse = await UpdateDeliveryOption(
            body.Data[0].Id,
            updateDeliveryOptionRequest,
            sessionCookie,
            ct
        );
        updateDeliveryOptionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deliveryOption = await GetDeliveryOptionFromDbById(
            new DeliveryOptionId(body.Data[0].Id),
            ct
        );
        deliveryOption.Should().NotBeNull();
        deliveryOption.Title.Value.Should().Be(newTitle);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        UpdateDeliveryOptionRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await UpdateDeliveryOption(Guid.Empty, invalidRequest, sessionCookie, ct);
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

        var response = await GetDeliveryOptions(sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<AdminDeliveryOptionDto>>
        >(ct);
        body.Should().NotBeNull();

        var updateDeliveryOptionRequest = new UpdateDeliveryOptionRequest(
            body.Data[1].Title,
            null,
            null
        );
        var updateDeliveryOptionResponse = await UpdateDeliveryOption(
            body.Data[0].Id,
            updateDeliveryOptionRequest,
            sessionCookie,
            ct
        );

        updateDeliveryOptionResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_DeliveryOptionNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var updateDeliveryOptionRequest = new UpdateDeliveryOptionRequest(
            TestData.String(DeliveryOptionTitle.MaxLength),
            null,
            null
        );
        var updateDeliveryOptionResponse = await UpdateDeliveryOption(
            Guid.NewGuid(),
            updateDeliveryOptionRequest,
            sessionCookie,
            ct
        );

        updateDeliveryOptionResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new UpdateDeliveryOptionRequest(
            TestData.String(DeliveryOptionTitle.MaxLength),
            null,
            null
        );

        var response = await UpdateDeliveryOption(Guid.Empty, request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var updateDeliveryOptionRequest = new UpdateDeliveryOptionRequest(
            TestData.String(DeliveryOptionTitle.MaxLength),
            null,
            null
        );
        var response = await UpdateDeliveryOption(
            Guid.Empty,
            updateDeliveryOptionRequest,
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, UpdateDeliveryOptionRequest> InvalidRequests()
    {
        var request = new UpdateDeliveryOptionRequest(null, null, null);

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

using Application.Abstractions.OAuth;

namespace Infrastructure.Services.OAuth;

internal sealed class OAuthClientFactory(IServiceProvider sp) : IOAuthClientFactory
{
    public IOAuthClient Get(OAuthProvider provider) => sp.GetRequiredKeyedService<IOAuthClient>(provider);
}
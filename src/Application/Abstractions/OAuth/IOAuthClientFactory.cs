namespace Application.Abstractions.OAuth;

public interface IOAuthClientFactory
{
    IOAuthClient Get(OAuthProvider provider);
}
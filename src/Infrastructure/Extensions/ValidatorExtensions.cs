namespace Infrastructure.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> Url<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(UrlIsValidUri);

        static bool UrlIsValidUri(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var outUri)
                && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
        }
    }
}

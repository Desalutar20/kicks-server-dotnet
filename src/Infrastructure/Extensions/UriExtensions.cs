using System.Web;

namespace Infrastructure.Extensions;

public static class UriExtensions
{
    public static Uri AddParameter(this Uri url, string paramName, string paramValue)
    {
        var uriBuilder = new UriBuilder(url);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query.Add(paramName, paramValue);
        uriBuilder.Query = query.ToString();

        return uriBuilder.Uri;
    }
}

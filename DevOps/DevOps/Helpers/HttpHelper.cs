using System.Web;

namespace DevOps.Helpers
{
    public static class HttpHelper
    {
        public static void UpsertQueryParam(ref string url, string paramKey, string paramValue)
        {
            var uri = new Uri(url);
            var nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
            nameValueCollection.Remove(paramKey);
            if (!string.IsNullOrEmpty(paramValue))
            {
                nameValueCollection.Add(paramKey, paramValue);
            }

            var ub = new UriBuilder(uri);
            ub.Query = nameValueCollection.ToString();

            url = ub.Uri.ToString();
        }
    }
}

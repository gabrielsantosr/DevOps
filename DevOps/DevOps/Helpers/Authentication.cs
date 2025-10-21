using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Text;

namespace DevOps.Helpers
{
    internal static class Authentication
    {
        
        internal static async Task<AuthenticationHeaderValue> GetAuthorization()
        {
            if (basicAuth)
                return GetPersonalAccessTokenAuthorization();
            else
                return await GetManagedIdentityAuthorization();
        }
        private static AuthenticationHeaderValue GetPersonalAccessTokenAuthorization() => new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($":{PAT}")));


        private static async Task<AuthenticationHeaderValue> GetManagedIdentityAuthorization()
        {
            if (lastTokenExpirty < DateTimeOffset.Now.AddMinutes(-5) || lastTokenValue is null)
            {
                var credential = new ManagedIdentityCredential();
                var tokenRequest = new TokenRequestContext(new[] { "https://app.vssps.visualstudio.com/.default" });
                var token = await credential.GetTokenAsync(tokenRequest);
                lastTokenExpirty = token.ExpiresOn;
                lastTokenValue = token.Token;
            }
            return new AuthenticationHeaderValue("Bearer", lastTokenValue);
        }
        
        private static string PAT = Environment.GetEnvironmentVariable("PAT");
        private static bool basicAuth = Environment.GetEnvironmentVariable("Authentication") == "Basic" && PAT != null;
        
        private static DateTimeOffset lastTokenExpirty = DateTimeOffset.MinValue;
        private static string lastTokenValue;
    }
}

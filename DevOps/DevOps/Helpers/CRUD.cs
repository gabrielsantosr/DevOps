using DevOps.Classes;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace DevOps.Helpers
{
    public static class CRUD
    {
        private static readonly string acceptHeaderValue = $"application/json";

        public static Task<CrudResponse> Create(string url, object itemToCreate) => Request(Enums.CRUD.Create, url, itemToCreate);

        public static Task<CrudResponse> Retrieve(string url, bool allPages = false) => allPages ? RetrievePages(url) : Request(Enums.CRUD.Retrieve, url);

        public static Task<CrudResponse> Update(string url, object itemToUpdate) => Request(Enums.CRUD.Update, url, itemToUpdate);

        public static Task<CrudResponse> Delete(string url, object itemToUpdate) => Request(Enums.CRUD.Delete, url);

        private static async Task<CrudResponse> Request(Enums.CRUD crudOperation, string url, object request = null)
        {

            try
            {
                StringContent reqBody = request is null ? null : new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeaderValue));

                    client.DefaultRequestHeaders.Authorization = await Authentication.GetAuthorization();

                    Task<HttpResponseMessage> responseTask = null;
                    switch (crudOperation)
                    {
                        case Enums.CRUD.Create:
                            responseTask = client.PostAsync(url, reqBody); break;
                        case Enums.CRUD.Retrieve:
                            responseTask = client.GetAsync(url); break;
                        case Enums.CRUD.Update:
                            responseTask = client.PatchAsync(url, reqBody); break;
                        case Enums.CRUD.Delete:
                            responseTask = client.DeleteAsync(url); break;
                    }
                    using (HttpResponseMessage response = await responseTask)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return new CrudResponse()
                        {
                            StatusCode = response.StatusCode,
                            Content = responseBody,
                            NextPageToken = crudOperation == Enums.CRUD.Retrieve ? GetContinuationToken(response.Headers) : null
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new CrudResponse()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = ex.Message,
                };
            }
        }

        private static string GetContinuationToken(HttpResponseHeaders headers)
        {
            foreach (var k in headers)
            {
                if (k.Key.ToLower() == "x-ms-continuationtoken")
                {
                    return k.Value.First();
                }
            }
            return null;
        }

        private static async Task<CrudResponse> RetrievePages(string url)
        {
            CrudResponse output = new() { StatusCode = HttpStatusCode.InternalServerError };
            List<string> results = new List<string>();
            string token = null;
            do
            {
                // Assuming al least the ApiVersion is included in the url query, there's no need to check whether the "?" sign is present.
                string tokenQueryElement = token is null ? String.Empty : "&continuationToken=" + token;
                CrudResponse result = await Request(Enums.CRUD.Retrieve, url + tokenQueryElement);
                output.StatusCode = result.StatusCode;
                int status = (int)result.StatusCode;
                results.Add(result.Content);
                token = result.NextPageToken;
            }
            while (token != null);
            output.Content = System.Text.Json.JsonSerializer.Serialize(results);
            return output;
        }


    }

}


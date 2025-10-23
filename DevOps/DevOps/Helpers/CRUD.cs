using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace DevOps.Helpers
{
    public static class CRUD
    {
        private static readonly string acceptHeaderValue = $"application/json";

        public static Task<(string, HttpStatusCode)> Create(string url, object itemToCreate) => Request(Enums.CRUD.Create, url, itemToCreate);

        public static Task<(string, HttpStatusCode)> Retrieve(string url) => Request(Enums.CRUD.Retrieve, url);

        public static Task<(string, HttpStatusCode)> Update(string url, object itemToUpdate) => Request(Enums.CRUD.Update, url, itemToUpdate);

        public static Task<(string, HttpStatusCode)> Delete(string url, object itemToUpdate) => Request(Enums.CRUD.Delete, url);

        private static async Task<(string, HttpStatusCode)> Request(Enums.CRUD crudOperation, string url, object request = null)
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
                        return (responseBody, response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                return (ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}


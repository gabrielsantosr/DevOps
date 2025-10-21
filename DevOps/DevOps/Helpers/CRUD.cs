using System.Net.Http.Headers;
using System.Text;

namespace DevOps.Helpers
{
    public static class CRUD
    {
        private static readonly string acceptHeaderValue = $"application/json";
        
        public static async Task<string> Create(string url, object itemToCreate)
        {
            string serializedCreateRequest = System.Text.Json.JsonSerializer.Serialize(itemToCreate);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeaderValue));

                client.DefaultRequestHeaders.Authorization = await Authentication.GetAuthorization();

                using (HttpResponseMessage response = await client.PostAsync(url, new StringContent(serializedCreateRequest, Encoding.UTF8, "application/json")))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
            }
        }
        public static async Task<string> Retrieve(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeaderValue));


                client.DefaultRequestHeaders.Authorization = await Authentication.GetAuthorization();

                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
            }
        }
        public static async Task<string> Update(string url, object itemToUpdate)
        {
            string serializedUpdateRequest = System.Text.Json.JsonSerializer.Serialize(itemToUpdate);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeaderValue));

                client.DefaultRequestHeaders.Authorization = await Authentication.GetAuthorization();

                using (HttpResponseMessage response = await client.PatchAsync(url, new StringContent(serializedUpdateRequest, Encoding.UTF8, "application/json")))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
            }
        }
    }
}

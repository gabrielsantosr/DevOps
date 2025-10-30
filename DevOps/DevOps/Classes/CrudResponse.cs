using System.Net;
using System.Text.Json.Serialization;

namespace DevOps.Classes
{
    public class CrudResponse
    {
        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string NextPageToken { get; set; }

    }
}

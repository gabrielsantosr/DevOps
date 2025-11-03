using System.Text.Json.Serialization;

namespace DevOps.Classes
{
    public class CollectionResult
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
        [JsonPropertyName("value")]
        public List<object> Value { get; set; }
    }
}

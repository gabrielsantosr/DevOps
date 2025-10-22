using System.Text.Json.Serialization;

namespace DevOps.Organization.Project.Git.PR
{
    public class Transition
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("target")]
        public string Target { get; set; }
    }
}

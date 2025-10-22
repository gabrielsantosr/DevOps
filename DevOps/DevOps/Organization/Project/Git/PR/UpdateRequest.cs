using System.Text.Json.Serialization;

namespace DevOps.Organization.Project.Git.PR
{
    internal class UpdateRequest
    {
        [JsonIgnore]
        internal Status Status { get; set; } = Status.NotIncluded; // Should be ignored by DevOps, since it is a not valid value. Using JsonIgnoreCondition.WhenWritingDefault would not make this program able to set the status to NotSet.
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? status { get=> Status == Status.NotIncluded?null: (int)Status; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Title { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string TargetRefName { get; set; }
    }
}

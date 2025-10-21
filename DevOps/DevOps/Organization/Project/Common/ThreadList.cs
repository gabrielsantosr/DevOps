using DevOps.Helpers;
using System.Text.Json.Serialization;



namespace DevOps.Organization.Project.Common;
public class Author
{
    public string displayName { get; set; }
    public string url { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
    public string imageUrl { get; set; }
    public string descriptor { get; set; }
}


public class Comment
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? parentCommentId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Author author { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string commentType { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string content { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? publishedDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? lastUpdatedDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? lastContentUpdatedDate { get; set; }

}

public class ThreadList
{
    public List<Thread> value { get; set; }
    public int count { get; set; }
}

public class Thread
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? publishedDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? lastUpdatedDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Comment> comments { get; set; } = new List<Comment>();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? isDeleted { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? status { get; set; }


}

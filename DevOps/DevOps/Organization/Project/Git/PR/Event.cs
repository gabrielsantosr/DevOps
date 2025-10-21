namespace DevOps.Organization.Project.Git.PR;

public class Account
{
    public string id { get; set; }
    public string baseUrl { get; set; }
}

public class Author
{
    public string name { get; set; }
    public string email { get; set; }
    public DateTime date { get; set; }
}

public class Avatar
{
    public string href { get; set; }
}

public class Collection
{
    public string id { get; set; }
    public string baseUrl { get; set; }
}

public class Committer
{
    public string name { get; set; }
    public string email { get; set; }
    public DateTime date { get; set; }
}

public class CreatedBy
{
    public string displayName { get; set; }
    public string url { get; set; }
    public Links _links { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
    public string imageUrl { get; set; }
    public string descriptor { get; set; }
}

public class DetailedMessage
{
    public string text { get; set; }
    public string html { get; set; }
    public string markdown { get; set; }
}

public class LastMergeCommit
{
    public string commitId { get; set; }
    public Author author { get; set; }
    public Committer committer { get; set; }
    public string comment { get; set; }
    public string url { get; set; }
}

public class LastMergeSourceCommit
{
    public string commitId { get; set; }
    public string url { get; set; }
}

public class LastMergeTargetCommit
{
    public string commitId { get; set; }
    public string url { get; set; }
}

public class Links
{
    public Avatar avatar { get; set; }
    public Web web { get; set; }
    public Statuses statuses { get; set; }
}

public class Message
{
    public string text { get; set; }
    public string html { get; set; }
    public string markdown { get; set; }
}

public class Project
{
    public string id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public string state { get; set; }
    public int revision { get; set; }
    public string visibility { get; set; }
    public DateTime lastUpdateTime { get; set; }
    public string baseUrl { get; set; }
}

public class Repository
{
    public string id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public Project project { get; set; }
    public int size { get; set; }
    public string remoteUrl { get; set; }
    public string sshUrl { get; set; }
    public string webUrl { get; set; }
    public bool isDisabled { get; set; }
    public bool isInMaintenance { get; set; }
}

public class Resource
{
    public Repository repository { get; set; }
    public int pullRequestId { get; set; }
    public int codeReviewId { get; set; }
    public string status { get; set; }
    public CreatedBy createdBy { get; set; }
    public DateTime creationDate { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string sourceRefName { get; set; }
    public string targetRefName { get; set; }
    public string mergeStatus { get; set; }
    public bool isDraft { get; set; }
    public string mergeId { get; set; }
    public LastMergeSourceCommit lastMergeSourceCommit { get; set; }
    public LastMergeTargetCommit lastMergeTargetCommit { get; set; }
    public LastMergeCommit lastMergeCommit { get; set; }
    public List<object> reviewers { get; set; }
    public string url { get; set; }
    public Links _links { get; set; }
    public bool supportsIterations { get; set; }
    public string artifactId { get; set; }
}

public class ResourceContainers
{
    public Collection collection { get; set; }
    public Account account { get; set; }
    public Project project { get; set; }
}

public class Event
{
    public string subscriptionId { get; set; }
    public int notificationId { get; set; }
    public string id { get; set; }
    public string eventType { get; set; }
    public string publisherId { get; set; }
    public Message message { get; set; }
    public DetailedMessage detailedMessage { get; set; }
    public Resource resource { get; set; }
    public string resourceVersion { get; set; }
    public ResourceContainers resourceContainers { get; set; }
    public DateTime createdDate { get; set; }
}

public class Statuses
{
    public string href { get; set; }
}

public class Web
{
    public string href { get; set; }
}


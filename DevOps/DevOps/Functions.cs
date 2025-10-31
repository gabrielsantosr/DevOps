using DevOps.Classes;
using DevOps.Helpers;
using DevOps.Organization.Project.Git.PR;
using DevOps.Organization.Project.Git.PR.Event;
using DevOps.Organization.Project.Git.PR.Thread;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using JSON = System.Text.Json.JsonSerializer;

namespace DevOps;

public class Functions
{
    private readonly ILogger<Functions> _logger;

    public Functions(ILogger<Functions> logger)
    {
        _logger = logger;
    }

    private static List<Transition> allowedTransitions =
        JSON.Deserialize<List<Transition>>(Environment.GetEnvironmentVariable("AllowedBranchTransitions"))
        .FindAll(
            x => 
            !(string.IsNullOrWhiteSpace(x.Source) || string.IsNullOrWhiteSpace(x.Target)    // No empty source or target
            ||
            (x.Source == x.Target && x.Source == Constants.Constants.AnyBranch) // Allowed source equals target when there value is a wildcard,  to by-pass the logic
            ||
            (x.Source != x.Target) // No other source equals target allowance
            ));

    private static readonly string ForbiddenBranchTransitionMessage = Environment.GetEnvironmentVariable("ForbiddenBranchTransitionMessage");

    [Function("UpsertPullRequest")]
    public async Task<IActionResult> UpsertPullRequestEvent([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest req)
    {
        using var reader = new StreamReader(req.Body);
        string serializedBody = await reader.ReadToEndAsync();
        Event pr = JSON.Deserialize<Event>(serializedBody);

        string source = pr.resource.sourceRefName;
        string target = pr.resource.targetRefName;
        string status = pr.resource.status;
        string title = pr.resource.title;
        string description = pr.resource.description;
        string url = pr.resource.url;

        Task<CrudResponse> action = null;

        if (status == "active" && !IsAllowedTranstion(source, target))
        {
            string[] actionValues = (req.Query.ContainsKey("action") ? req.Query["action"].ToString() : "").Split(',');

            foreach (string actionValue in actionValues)
            {
                bool abort = true;
                switch (actionValue)
                {
                    case "title":
                        if (!title.StartsWith(ForbiddenBranchTransitionMessage))
                            action = CRUD.Update(url, new UpdateRequest() { Title = $"{ForbiddenBranchTransitionMessage}| {title}" });
                        break;
                    case "title-abandon":
                        if (!title.StartsWith(ForbiddenBranchTransitionMessage))
                            action = CRUD.Update(url, new UpdateRequest() { Title = $"{ForbiddenBranchTransitionMessage}| {title}", Status = Status.Abandoned });
                        break;
                    case "description":
                        if (!description.StartsWith(ForbiddenBranchTransitionMessage))
                            action = CRUD.Update(url, new UpdateRequest() { Description = $"{ForbiddenBranchTransitionMessage}| {description}" });
                        break;
                    case "description-abandon":
                        if (!description.StartsWith(ForbiddenBranchTransitionMessage))
                            action = CRUD.Update(url, new UpdateRequest() { Description = $"{ForbiddenBranchTransitionMessage}| {description}", Status = Status.Abandoned });
                        break;
                    case "target":
                        string newTarget = GetAllowedTarget(source);
                        if (newTarget != null)
                            action = CRUD.Update(url, new UpdateRequest() { TargetRefName = newTarget });
                        else
                            abort = false;
                        break;
                    case "comment":
                        var comment = new Comment
                        {
                            content = ForbiddenBranchTransitionMessage,
                            commentType = "system",
                        };
                        var commentThread = new PRThread()
                        {
                            status = "closed",
                            comments = new List<Comment>() { comment }
                        };
                        action = CRUD.Create(url + "/threads", commentThread);
                        break;
                }
                if (abort)
                    break;
            }
        }
        if (action is null)
            return new OkResult();
        var result = await action;
        if (result.NextPageToken != null)
        {
            req.HttpContext.Response.Headers.Add(Constants.Constants.NextPageTokenHeaderKey, result.NextPageToken);
        }
        return new OkObjectResult(result.Content) { StatusCode = (int)result.StatusCode };

    }

    private bool IsAllowedTranstion(string source, string target)
    {
        const string any = Constants.Constants.AnyBranch;
        return allowedTransitions.Any(x => (x.Source == source || x.Source == any) && (x.Target == target || x.Target == any));
    }

    private string? GetAllowedTarget(string source)
    {
        const string any = Constants.Constants.AnyBranch;
        return  
            allowedTransitions.Find(x => x.Source == source && x.Target != any)?.Target // Check for exact target-branch match
            ?? 
            allowedTransitions.Find(x => x.Source == any && x.Target != any && x.Target != source)?.Target; // If there is not exact match, checks for a transition with a wildcard source and a specific target.
    }
}
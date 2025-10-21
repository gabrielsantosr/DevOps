using DevOps.Helpers;
using DevOps.Organization.Project.Common;
using DevOps.Organization.Project.Git.PR;
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

    private static readonly string apiVersion = "api-version=" + Environment.GetEnvironmentVariable("ApiVersion");
    private static List<Transition> allowedTransitions = JSON.Deserialize<List<Transition>>(Environment.GetEnvironmentVariable("AllowedBranchTransitions"));
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

        Task<string> action = null;

        if (status == "active" && !allowedTransitions.Any(x => x.Source == source && x.Target == target))
        {
            string actionValue = req.Query.ContainsKey("action") ? req.Query["action"] : string.Empty;

            switch (actionValue)
            {
                case "title":
                    if (!title.StartsWith(ForbiddenBranchTransitionMessage))
                        action = CRUD.Update(url + "?" + apiVersion, new UpdateRequest() { Title = $"{ForbiddenBranchTransitionMessage}| {title}" });
                    break;
                case "title-abandon":
                    if (!title.StartsWith(ForbiddenBranchTransitionMessage))
                        action = CRUD.Update(url + "?" + apiVersion, new UpdateRequest() { Title = $"{ForbiddenBranchTransitionMessage}| {title}", Status = Status.Abandoned });
                    break;
                case "description":
                    if (!description.StartsWith(ForbiddenBranchTransitionMessage))
                        action = CRUD.Update(url + "?" + apiVersion, new UpdateRequest() { Description = $"{ForbiddenBranchTransitionMessage}| {description}" });
                    break;
                case "description-abandon":
                    if (!description.StartsWith(ForbiddenBranchTransitionMessage))
                        action = CRUD.Update(url + "?" + apiVersion, new UpdateRequest() { Description = $"{ForbiddenBranchTransitionMessage}| {description}", Status = Status.Abandoned });
                    break;
                case "comment":
                    var comment = new Organization.Project.Common.Comment
                    {
                        content = ForbiddenBranchTransitionMessage,
                        commentType = "system",
                    };
                    var commentThread = new Organization.Project.Common.Thread()
                    {
                        status = "closed",
                        comments = new List<Comment>() { comment }
                    };
                    action = CRUD.Create(url + "/threads?" + apiVersion, commentThread);
                    break;
            }
        }
        return (action != null) 
            ? new OkObjectResult(await action) 
            : new OkResult();

    }
}
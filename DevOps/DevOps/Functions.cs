using DevOps.Helpers;
using DevOps.Organization.Project.Git.PR;
using DevOps.Organization.Project.Git.PR.Event;
using DevOps.Organization.Project.Git.PR.Thread;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
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

        Task<(string, HttpStatusCode)> action = null;

        if (status == "active" && !allowedTransitions.Any(x => x.Source == source && x.Target == target))
        {
            string[] actionValues = (req.Query.ContainsKey("action") ? req.Query["action"].ToString() : "").Split(',');

            foreach (string actionValue in actionValues)
            {
                bool abort = true;
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
                    case "target":
                        string newTarget = allowedTransitions.Find(x => x.Source == source)?.Target;
                        if (newTarget != null)
                            action = CRUD.Update(url + "?" + apiVersion, new UpdateRequest() { TargetRefName = newTarget });
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
                        action = CRUD.Create(url + "/threads?" + apiVersion, commentThread);
                        break;
                }
                if (abort)
                    break;
            }
        }
        if (action is null)
            return new OkResult();
        var result = await action;
        string resultContent = result.Item1;
        HttpStatusCode statusCode = result.Item2;
        return new OkObjectResult(resultContent) { StatusCode = (int)statusCode };

    }
}
using DevOps.Classes;
using DevOps.Organization.Project.Git.PR.Event;
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

    [Function("UpsertPullRequest")]
    public async Task<IActionResult> UpsertPullRequestEvent([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest req)
    {
        CrudResponse result;
        try
        {
            using var stream = req.Body;
            using var reader = new StreamReader(stream);
            string serializedBody = await reader.ReadToEndAsync();
            Event pr = JSON.Deserialize<Event>(serializedBody);
            string[] actionValues = (req.Query.ContainsKey("action") ? req.Query["action"].ToString() : "").Split(',');
            result = await Handlers.Git.UpsertPullRequestEvent(pr, actionValues);

            if (result?.NextPageToken != null)
            {
                req.HttpContext.Response.Headers.Add(Constants.Constants.NextPageTokenHeaderKey, result.NextPageToken);
            }
        }
        catch (Exception ex)
        {
            result = new CrudResponse() { StatusCode = System.Net.HttpStatusCode.InternalServerError, Content = ex.Message };
        }
        return (result is null) ? new OkResult() : new OkObjectResult(result.Content) { StatusCode = (int)result.StatusCode };
    }

}
using DevOps.Classes;
using DevOps.Helpers;
using DevOps.Organization.Project.Git.PR;
using DevOps.Organization.Project.Git.PR.Event;
using DevOps.Organization.Project.Git.PR.Thread;
using System.Text.RegularExpressions;
using JSON = System.Text.Json.JsonSerializer;

namespace DevOps.Handlers
{
    public class Git
    {

        private static Dictionary<string, RepoConfig> ReposConfig;
        private static string ForbiddenBranchTransitionMessage = Environment.GetEnvironmentVariable("ForbiddenBranchTransitionMessage");
        public static async Task<CrudResponse> UpsertPullRequestEvent(Event prEvent, string[] behaviours)
        {
            string projectName = prEvent.resource.repository.project.name;
            string repoName = prEvent.resource.repository.name;
            string source = prEvent.resource.sourceRefName.Substring(Constants.Constants.BranchPrefix.Length);
            string target = prEvent.resource.targetRefName.Substring(Constants.Constants.BranchPrefix.Length);
            string status = prEvent.resource.status;
            string title = prEvent.resource.title;
            string description = prEvent.resource.description;
            string url = prEvent.resource.url;

            Task<CrudResponse> action = null;

            if (status == "active" && !IsAllowedTransition(projectName, repoName, source, target))
            {
                foreach (string behaviour in behaviours)
                {
                    bool abort = true;
                    switch (behaviour)
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
                            string newTarget = GetDefaultTarget(projectName, repoName, source);
                            if (newTarget != null)
                                action = CRUD.Update(url, new UpdateRequest() { TargetRefName = Constants.Constants.BranchPrefix + newTarget });
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
            return action is null ? null : await action;
        }
        public static bool IsAllowedTransition(string project, string repo, string source, string target)
        {
            var repoConfig = GetRepoConfig(project,repo);

            return
                //There is no configuration set for the repo
                repoConfig is null
                ||
                // There is no branch configuration set for the target
                !repoConfig.ContainsKey(target)
                ||
                repoConfig.Keys.Any(x =>
                // source's default target is target
                (x == source && repoConfig[source]?.DefaultTarget == target)
                ||
                // target allows source
                (x == target && repoConfig[target]?.AllowedSourcesRegex != null && Regex.IsMatch(source, repoConfig[target].AllowedSourcesRegex))
                );
        }

        public static string GetDefaultTarget(string project,string repo, string source)
        {
            string target = null;
            var repoConfig = GetRepoConfig(project, repo);
            if (repoConfig is not null && repoConfig.ContainsKey(source))
            {
                target = repoConfig[source]?.DefaultTarget;
            }
            return target;
        }

        /// <summary>
        /// Returns the first applicable configuration found for the <paramref name="project"/> and <paramref name="repo"/>, or <i>null</i>.<br/>
        /// The configuration is stored as a JSON in the environment variable <i>ReposConfig</i>, and is parsed as a  Dictionary<br/>
        /// Each key of the dictionary is a string and each value is a RepoConfig <br/><br/>
        /// An applicable configuration key can be:  
        /// <list type="number">
        /// <item>"<paramref name="project"/>/<paramref name="repo"/>"</item>
        /// <item>"<b>*</b>/<paramref name="repo"/>"</item>
        /// <item>"<paramref name="project"/>/<b>*</b>"</item>
        /// <item>"<b>*</b>"</item>
        /// </list>
        /// <br/>
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        private static RepoConfig GetRepoConfig(string project, string repo)
        {
            if (ReposConfig is null)
            {
                const string reposConfigVariableName = "ReposConfig";
                try
                {
                    ReposConfig = JSON.Deserialize<Dictionary<string, RepoConfig>>(Environment.GetEnvironmentVariable(reposConfigVariableName));

                }
                catch (Exception ex)
                {
                    throw new Exception($"Error when trying to parse {reposConfigVariableName}");
                }
            }
            RepoConfig repoConfig = null;
            string[] keys = {
                $"{project}/{repo}",
                $"*/{repo}",
                $"{project}/*",
                "*"
            };
            foreach (string key in keys)
            {
                if (ReposConfig.TryGetValue(key, out repoConfig))
                    break;
            }
            return repoConfig;
        }

    }
}

# DevOps
Repo for a Function App comprising the endpoints for DevOps Project WebHooks.
When it needs to make CRUD operations, it can use a Personal Access Token, or even better, authenticate with a managed identity.
## UspertPullRequest
Intended to be set for 'Pull request created' and 'Pull request updated' WebHooks.

The idea is to take action when a pull request is created or updated and the combination of source and target branch are to be avoided.
The allowed transitions should be stored in environment variable `AllowedBranchTransitions` as a serialized list of instances of this project's `DevOps.Organization.Project.Git.PR.Transition` class.
When any source should be allowed into a specific branch, a transition should be included with source equal to `*` and the target to the specific branch name.
When a specific source should be allowed to any branch, a transition should be included with source equal to the specific branch name and the target to `*`.
Including a transition with source and target equal to `*` would by-pass the whole logic and would take no action. That is the only same-source-and-target transition that the program takes into account. Any other is ignored.

If the source-target combination is not found, the PR is active and there is a valid action included in the HTTP query, endpoint behaves according to this table:

| `action`              | behaviour
| --------------------- | -------------------------------------------------------------------------------------------------------- |
| `title`               | If the title is not prefixed with a forbidden message, the forbidden message is added as a prefix.       |
| `title-abandon`       | Same as action _title_, but it also changes the status of the PR to abandoned.                           |
| `description`         | If the description is not prefixed with a forbidden message, the forbidden message is added as a prefix. |
| `description-abandon` | Same as action _description_, but it also changes the status of the PR to abandoned.                     |
| `comment`             | Adds the forbidden comment as a system comment to the pull request.                                      |
| `target`              | Searches, amongst the allowed transitions, one with a valid target for the source branch. If found, the target is automatically changed. First it checks for the first instance where the source matches the PR source and the target is any other than `*`. If not found, it checks for the first instance where the source is `*` and the target is any other than the PR source or `*`. In case `target` action cannot be accomplished because a target branch was not found to match the source, an alternative action can be tried by setting the value to `target,<alternative-action>`, e.g. `target,comment`. |


As of now, the forbiddden message is stored in an environment variable, and I use special emoji chars, which can be included within strings as `\u<char-code>`,which are properly rendered in title, description and comments of PRs.


### References
[WebHooks](https://learn.microsoft.com/en-us/azure/devops/repos/git/create-pr-status-server-with-azure-functions?view=azure-devops)

[Managed Identity setup](https://learn.microsoft.com/en-us/azure/devops/integrate/get-started/authentication/service-principal-managed-identity?view=azure-devops)

Something I didn't find mentioned in the Managed Identity setup link is that, first, it is required to connect DevOps with your Azure tenant from DevOps Organization Settings > Microsoft Entra `https://dev.azure.com/<your-organization-name>/_settings/organizationAad`

#### Technical notes
APIs for all CRUD operations are ready.
They all use the api version set in an environment variable configuration. Given a custom request is added with an api version included, the api version will be overriden with the one set in the configuration.
As of now there is no functionality that requires retrieval of records from DevOps.
For retrievals there is the optional parameter `allPages`, that when set to `true` will recursively query DevOps until there are no more results. In this case, the response will always be an array, even if the result is a single page.
If `allPages` is `false` or is not set, the query returns a single page, and if there where more results, the response includes the `x-ms-continuationtoken` header.

# DevOps
Repo for a Function App comprising the endpoints for DevOps Project WebHooks.
When it needs to make CRUD operations, it can use a Personal Access Token, or even better, authenticate with a managed identity.
## UspertPullRequest
Intended to be set for 'Pull request created' and 'Pull request updated' WebHooks.
The idea is to take action when a pull request is created or updated and the combination of source and target branch are to be avoided.
The allowed transitions should be stored in environment variable `AllowedBranchTransitions` as a serialized list of instances of this project's `DevOps.Organization.Project.Git.PR.Transition` class.

If the source-target combination is not found, the PR is active and there is a valid action included in the HTTP query, endpoint behaves according to this table:

| action              | behaviour
| ------------------- | -------------------------------------------------------------------------------------------------------- |
| title               | If the title is not prefixed with a forbidden message, the forbidden message is added as a prefix.       |
| title-abandon       | Same as action _title_, but it also changes the status of the PR to abandoned.                           |
| description         | If the description is not prefixed with a forbidden message, the forbidden message is added as a prefix. |
| description-abandon | Same as action _description_, but it also changes the status of the PR to abandoned.                     |
| comment             | Adds the forbidden comment as a system comment to the pull request.                                      | 

As of now, the forbiddden message is stored in an environment variable, and I use special emoji chars, which can be included within strings as `\u<char-code>`,which are properly rendered in title, description and comments of PRs.

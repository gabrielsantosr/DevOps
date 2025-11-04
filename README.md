# DevOps
Repo for a Function App comprising the endpoints for DevOps Project WebHooks.
When it needs to make CRUD operations, it can use a Personal Access Token, or even better, authenticate with a managed identity.
## UspertPullRequest
Intended to be set for 'Pull request created' and 'Pull request updated' WebHooks.

The idea is to take action when a pull request is created or updated and the combination of source and target branch are to be avoided.

The query of the WebHook URL should include the action parameter. E.g. `<url>`?action=`<chosen-action>` 

### Actions

| `action`              | behaviour
| --------------------- | -------------------------------------------------------------------------------------------------------- |
| `title`               | If the title is not prefixed with a forbidden message, the forbidden message is added as a prefix.       |
| `title-abandon`       | Same as action _title_, but it also changes the status of the PR to abandoned.                           |
| `description`         | If the description is not prefixed with a forbidden message, the forbidden message is added as a prefix. |
| `description-abandon` | Same as action _description_, but it also changes the status of the PR to abandoned.                     |
| `comment`             | Adds the forbidden comment as a system comment to the pull request.                                      |
| `target`              | If there is a configuration for the source branch, the target branch is updated to the value of its `DefaultTarget` property, if it is not null. In case `target` action cannot be accomplished because a target branch was not found to match the source, an alternative action can be tried by setting the value to `target,<alternative-action>`, e.g. `target,comment`. |

### Configuration
The allowed transitions should be stored in environment variable `ReposConfig`.

#### Sample
```
// As object
{
	"MyProject/MyRepo": {
		"master": {
			"DefaultTarget": null,
			"AllowedSourcesRegex": "^(test|hot-fix)$"
		},
		"test": {
			"DefaultTarget": "master",
			"AllowedSourcesRegex": "^(dev|hot-fix)$"
		},
		"dev": {
			"DefaultTarget": "test",
			"AllowedSourcesRegex": "^(feature\\/.+|hot-fix)$"
		},
		"dont-pull-into-me": {
			"DefaultTarget": null,
			"AllowedSourcesRegex": null
		},
		"do-pull-into-me": {
			"DefaultTarget": null,
			"AllowedSourcesRegex": ".+"
		}
	}
}
// As serialized object (as it should be stored). Pay attention to escaped chars in regexs
"{\"MyProject/MyRepo\":{\"master\":{\"DefaultTarget\":null,\"AllowedSourcesRegex\":\"^(test|hot-fix)$\"},\"test\":{\"DefaultTarget\":\"master\",\"AllowedSourcesRegex\":\"^(dev|hot-fix)$\"},\"dev\":{\"DefaultTarget\":\"test\",\"AllowedSourcesRegex\":\"^(feature\\\\/.+|hot-fix)$\"},\"dont-pull-into-me\":{\"DefaultTarget\":null,\"AllowedSourcesRegex\":null},\"do-pull-into-me\":{\"DefaultTarget\":null,\"AllowedSourcesRegex\":\".+\"}}}"

```
An applicable configuration is selected following the following hierarchical criteria.
| Appicable config keys |
| --------------------- |
| `<project>/<repo>`    |
| `*/<repo>`            |
| `<project>/*`         |
| `*`                   |

Each branch configuration of each repo has a `DefaultTarget` property,  and a `AllowedSourcesRegex` property.

A transition is OK when:
```
No applicable configuration is found
OR
There is no configuration for the target branch
OR
There is a configuration for the source branch and its defaultTarget is the target
OR
The source matches the target configuration AllowedSourceRegex property
```


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

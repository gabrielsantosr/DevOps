using System.Text.RegularExpressions;

namespace DevOps.Classes
{
    public class BranchConfig
    {
        public string DefaultTarget { get; init; }
        public string AllowedSourcesRegex { get; init; }
    }

}
/*
 * Proposed new config structure
{
	"MyRepo1Name": {
		"master": {
			"defaultTarget": "",
			"allowedSourcesRegex": "regex"
		},
		"dev": {
			"defaultTarget": "master",
			"allowedSourcesRegex": "regex"
		}
	},
	"MyRepo2Name": {
		"master": {
			"defaultTarget": "",
			"allowedSourcesRegex": "regex"
		},
		"dev": {
			"defaultTarget": "master",
			"allowedSourcesRegex": "regex"
		}
	}
}
 */
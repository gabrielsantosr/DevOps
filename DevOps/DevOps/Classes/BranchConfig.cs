namespace DevOps.Classes
{
    public class BranchConfig
    {
        public string DefaultTarget { get; set; }
        public string AllowedSourcesRegex { get; set; }
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
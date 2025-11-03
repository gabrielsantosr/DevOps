using System.Text.RegularExpressions;

namespace DevOps.Classes
{
    public class BranchConfig
    {
        public string DefaultTarget { get; init; }
        public string AllowedSourcesRegex { get; init; }

        private const string branchPrefix = "refs/heads/";

        public bool IsAllowedSource(string source)
        {
            source = source.Substring(source.IndexOf(branchPrefix));
            return this.AllowedSourcesRegex != null && Regex.IsMatch(source, this.AllowedSourcesRegex);
        }

		public string GetDefaultTarget()
		{
			return string.IsNullOrWhiteSpace(this.DefaultTarget)?null: branchPrefix + DefaultTarget;
		}

    }

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
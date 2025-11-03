using DevOps.Classes;

namespace Test
{
    public class UnitTest1
    {
        static UnitTest1()
        {
            Init();
        }

        [Theory]
        [InlineData("dev", "test", true)]
        [InlineData("dev", "master", false)]
        [InlineData("dev", "hot-fix", true)]
        [InlineData("test", "dev", false)]
        [InlineData("test", "master", true)]
        [InlineData("test", "hot-fix", true)] // there is no config for target hot-fix, so there is no restriction upon it's source branches
        [InlineData("master", "test", false)]
        [InlineData("master", "dev", false)]
        [InlineData("master", "hot-fix", true)]
        [InlineData("hot-fix", "dev", true)]
        [InlineData("hot-fix", "test", true)]
        [InlineData("hot-fix", "master", true)]
        [InlineData("feature/something", "dev", true)]
        [InlineData("feature/something", "dev", true)]
        [InlineData("feature/something", "test", false)]
        [InlineData("feature/something", "master", false)]
        [InlineData("*", "dev", false)]
        [InlineData("*", "test", false)]
        [InlineData("*", "master", false)]
        [InlineData("*", "hot-fix", true)]
        [InlineData("dev", "dont-pull-into-me", false)]
        [InlineData("test", "dont-pull-into-me", false)]
        [InlineData("master", "dont-pull-into-me", false)]
        [InlineData("dev", "do-pull-into-me", true)]
        [InlineData("test", "do-pull-into-me", true)]
        [InlineData("master", "do-pull-into-me", true)]

        public void IsAllowedTransition_Test(string source, string target, bool expected)
        {
            Assert.Equal(expected, DevOps.Handlers.Git.IsAllowedTransition("MyRepoName", source, target));
        }

        [Theory]
        [InlineData("dev", "test")]
        [InlineData("test", "master")]
        [InlineData("master", null)]
        public void GetDefaultTarget_Test(string source, string expected)
        {
            string actual = DevOps.Handlers.Git.GetDefaultTarget("MyRepoName", source);
            Assert.Equal(expected, actual);
        }

        private static void Init()
        {
            var config = new Dictionary<string, Dictionary<string, BranchConfig>>()
            {
                ["MyRepoName"] = new()
                {
                    ["master"] = new() { AllowedSourcesRegex = "^(test|hot-fix)$" },
                    ["test"] = new() { AllowedSourcesRegex = "^(dev|hot-fix)$", DefaultTarget = "master" },
                    ["dev"] = new() { AllowedSourcesRegex = "^(feature\\/.+|hot-fix)$", DefaultTarget = "test" },
                    ["dont-pull-into-me"] = new() { AllowedSourcesRegex = null },
                    ["do-pull-into-me"] = new() { AllowedSourcesRegex = ".+" }
                }
            };
            string serializedConfig = System.Text.Json.JsonSerializer.Serialize(config);
            Environment.SetEnvironmentVariable("ReposConfig", serializedConfig);
        }
    }
}
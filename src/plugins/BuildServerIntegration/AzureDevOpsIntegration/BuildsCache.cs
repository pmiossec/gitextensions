using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;

namespace AzureDevOpsIntegration
{
    public class BuildsCache
    {
        public string? Id { get; set; }
        public string? BuildDefinitions { get; init; }
        public Dictionary<ObjectId, IBuildInfo> FinishedBuilds { get; } = [];
        public DateTime LastCall { get; set; } = DateTime.MinValue;
    }
}

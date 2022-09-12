using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace AzureDevOpsIntegration
{
    public class BuildsCache
    {
        public string? Id { get; set; }
        public string? BuildDefinitions { get; init; }
        public Dictionary<ObjectId, BuildInfo> FinishedBuilds { get; } = new Dictionary<ObjectId, BuildInfo>();
        public DateTime LastCall { get; set; } = DateTime.MinValue;
    }
}

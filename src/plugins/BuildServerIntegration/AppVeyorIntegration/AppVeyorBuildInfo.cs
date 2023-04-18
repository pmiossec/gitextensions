using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace AppVeyorIntegration
{
    public sealed class AppVeyorBuildInfo : BuildInfo
    {
        private static readonly IBuildDurationFormatter _buildDurationFormatter = new BuildDurationFormatter();

        public string? BuildId { get; set; }
        public ObjectId? CommitId { get; set; }
        public string? AppVeyorBuildReportUrl { get; set; }
        public string? Branch { get; set; }
        public string? BaseApiUrl { get; set; }
        public string? BaseWebUrl { get; set; }
        public string? PullRequestText { get; set; }
        public string? PullRequestTitle { get; set; }
        public string? TestsResultText { get; set; }

        public bool IsRunning => Status == BuildStatus.InProgress;

        public void UpdateDescription()
        {
            Description = _buildDurationFormatter.Format(Duration) + " " + TestsResultText + (!string.IsNullOrWhiteSpace(PullRequestText) ? " " + PullRequestText : string.Empty) + " " + Id;
            Tooltip = $"{Id} {StatusSymbol} ({DisplayStatus})" + (Duration.HasValue ? " - " + _buildDurationFormatter.Format(Duration, false) : string.Empty) + Environment.NewLine
                                    + (!string.IsNullOrWhiteSpace(TestsResultText) ? TestsResultText + Environment.NewLine : string.Empty)
                                    + (!string.IsNullOrWhiteSpace(PullRequestText) ? PullRequestText + ": " + PullRequestTitle + Environment.NewLine : string.Empty);
        }

        private string DisplayStatus => Status != BuildStatus.InProgress ? Status.ToString("G") : "In progress";
    }
}

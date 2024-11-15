using GitExtensions.Extensibility.Git;

namespace GitExtensions.Extensibility.BuildServerIntegration;

public class AggegatedBuildInfo : IBuildInfo
{
    private List<IBuildInfo> _builds;

    public IBuildInfo DefaultBuild { get; private set; }
    public string? Id => DefaultBuild?.Id;
    public DateTime StartDate => DefaultBuild.StartDate;
    public long? Duration => DefaultBuild.Duration;
    public BuildStatus Status => DefaultBuild.Status;
    public string? Description => DefaultBuild.Description;
    public IReadOnlyList<ObjectId> CommitHashList => DefaultBuild.CommitHashList;
    public string? BuildDefinitionName => string.Empty;
    public List<IBuildInfo> Builds
    {
        get => _builds;
        init
        {
            _builds = value;
            SelectDefaultBuild();
        }
    }

    public string? Url => DefaultBuild.Url;
    public bool ShowInBuildReportTab { get; set; } = true;
    public string? Tooltip => (Builds == null || Builds.Count == 0 ? DefaultBuild?.Tooltip : string.Join(Environment.NewLine, Builds.Select(b => b.Tooltip))) + Environment.NewLine + "PR #" + PullRequestId;
    public string? PullRequestId => DefaultBuild.PullRequestId ?? Builds.Find(b => string.IsNullOrEmpty(b.PullRequestId))?.PullRequestId;
    public string? PullRequestUrl => DefaultBuild.PullRequestUrl ?? Builds.Find(b => string.IsNullOrEmpty(b.PullRequestUrl))?.PullRequestUrl;

    public string StatusSymbol => Status switch
    {
        BuildStatus.Success => "✔",
        BuildStatus.Failure => "❌",
        BuildStatus.InProgress => "▶️",
        BuildStatus.Stopped => "⏹️",
        BuildStatus.Unstable => "❗",
        _ => "❓",
    };

    private void SelectDefaultBuild()
    {
        IEnumerable<IGrouping<string, IBuildInfo>> buildsByJob = _builds.GroupBy(b => b.BuildDefinitionName);
        if (buildsByJob.Count() == 1)
        {
            DefaultBuild = _builds.OrderByDescending(b => b.StartDate).FirstOrDefault();
        }
        else
        {
            List<IBuildInfo> lastBuildPerJob = new();
            foreach (IGrouping<string, IBuildInfo> job in buildsByJob)
            {
                lastBuildPerJob.Add(job.OrderByDescending(b => b.StartDate).FirstOrDefault());
            }

            lastBuildPerJob = lastBuildPerJob.OrderByDescending(b => b.StartDate).ToList();

            // If multiple job, select the one broken to prevent hiding errors.
            DefaultBuild = lastBuildPerJob.Find(b => b.Status == BuildStatus.Failure) ?? lastBuildPerJob[0];
        }
    }
}

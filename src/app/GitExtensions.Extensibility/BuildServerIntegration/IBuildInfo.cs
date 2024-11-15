using GitExtensions.Extensibility.Git;

namespace GitExtensions.Extensibility.BuildServerIntegration;

public interface IBuildInfo
{
    IReadOnlyList<ObjectId> CommitHashList { get; }
    string? Description { get; }
    long? Duration { get; }
    string? Id { get; }
    string? PullRequestId { get; }
    string? PullRequestUrl { get; }
    bool ShowInBuildReportTab { get; }
    DateTime StartDate { get; }
    BuildStatus Status { get; }
    string StatusSymbol { get; }
    string? Tooltip { get; }
    string? Url { get; }
    string? BuildDefinitionName { get; }
}

using GitExtensions.Extensibility.BuildServerIntegration;

namespace GitExtUtilsTests;

[TestFixture]
public sealed class AggegatedBuildInfoTests
{
    private static DateTime Date(int day) => new(2000, 7, day, 12, 0, 0, DateTimeKind.Utc);
    private static BuildInfo SuccessBuild(int day, string job = null) => new() { Status = BuildStatus.Success, StartDate = Date(day), BuildDefinitionName = job };

    [Test]
    public void Should_select_only_build_as_default_one()
    {
        IBuildInfo defaultBuild = SuccessBuild(1);
        AggegatedBuildInfo sut = new();

        sut.AddBuilds(new[] { defaultBuild });

        sut.DefaultBuild.Should().Be(defaultBuild);
    }

    [Test]
    public void Should_select_the_more_recent_build_result()
    {
        IBuildInfo moreRecentBuild = SuccessBuild(12);
        AggegatedBuildInfo sut = new();

        sut.AddBuilds([SuccessBuild(10), moreRecentBuild]);

        sut.DefaultBuild.Should().Be(moreRecentBuild);
    }

    [Test]
    public void Should_select_the_last_succeeded_build_result_of_job_when_muliple_jobs()
    {
        IBuildInfo moreRecentBuild = SuccessBuild(12, "1");
        AggegatedBuildInfo sut = new();

        sut.AddBuilds([SuccessBuild(10, "2"), SuccessBuild(10, "1"), moreRecentBuild]);

        sut.DefaultBuild.Should().Be(moreRecentBuild);
    }

    [Test]
    public void Should_select_the_broken_build_result_of_job_when_muliple_jobs_and_broken_is_the_more_recent_of_given_job()
    {
        IBuildInfo moreRecentBrokenBuild = new BuildInfo() { Status = BuildStatus.Failure, StartDate = Date(12), BuildDefinitionName = "1" };
        AggegatedBuildInfo sut = new();

        sut.AddBuilds([SuccessBuild(14, "2"), SuccessBuild(10, "1"), moreRecentBrokenBuild]);

        sut.DefaultBuild.Should().Be(moreRecentBrokenBuild);
    }
}

using FluentAssertions;
using GitCommands.Git;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class DetachedHeadParserTests
    {
        [Test]
        public void ShouldExtractOldVersionOfDetachedHeadOutput()
        {
            DetachedHeadParser.TryParse("(detached from c299581)", out string? sha1).Should().BeTrue();
            Assert.AreEqual("c299581", sha1);
        }

        [Test]
        public void ShouldExtractNewVersionOfDetachedHeadOutput()
        {
            DetachedHeadParser.TryParse("(HEAD detached at c299582)", out string? sha1).Should().BeTrue();
            Assert.AreEqual("c299582", sha1);
        }
    }
}

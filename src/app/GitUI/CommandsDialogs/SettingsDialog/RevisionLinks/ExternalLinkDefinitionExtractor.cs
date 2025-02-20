using GitCommands.ExternalLinks;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.RevisionLinks
{
    public abstract class ExternalLinkDefinitionExtractor : ICloudProviderExternalLinkDefinitionExtractor
    {
        private protected static readonly TranslationString CodeLink = new("{0} - Code");
        private protected static readonly TranslationString BranchLink = new("{0} - Branch");
        private protected static readonly TranslationString IssuesLink = new("{0} - Issues");
        private protected static readonly TranslationString PullRequestsLink = new("{0} - Pull Requests");
        private protected static readonly TranslationString ViewCommitLink = new("Commit in {0}");
        private protected static readonly TranslationString ViewBranchLink = new("Branch in {0}");
        private protected static readonly TranslationString ViewProjectLink = new("Project in {0}");

        public abstract string ServiceName { get; }
        public abstract Image Icon { get; }

        public abstract bool IsValidRemoteUrl(string remoteUrl);
        public abstract IList<ExternalLinkDefinition> GetDefinitions(string remoteUrl);
    }
}

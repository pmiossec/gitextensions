using System.Reactive.Concurrency;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerAdapter : IDisposable
    {
        void Initialize(IBuildServerWatcher buildServerWatcher, SettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null);

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        string UniqueKey { get; }

        IObservable<IBuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null);

        IObservable<IBuildInfo> GetRunningBuilds(IScheduler scheduler);

        /// <summary>
        ///  Provides an extension point for handling the switch of repositories.
        ///  For example, it could be used to clear build changes.
        /// </summary>
        void OnRepositoryChanged()
        {
            // Default implementation: we do nothing
        }
    }
}

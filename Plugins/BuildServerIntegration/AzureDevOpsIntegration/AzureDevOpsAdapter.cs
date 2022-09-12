using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AzureDevOpsIntegration.Settings;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace AzureDevOpsIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class AzureDevOpsIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public AzureDevOpsIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }
    }

    /// <summary>
    /// Provides build server integration for Azure DevOps (or TFS>=2015) into GitExtensions
    /// </summary>
    [Export(typeof(IBuildServerAdapter))]
    [AzureDevOpsIntegrationMetadata(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class AzureDevOpsAdapter : IBuildServerAdapter
    {
        public const string PluginName = "Azure DevOps and Team Foundation Server (since TFS2015)";

        private bool _firstCallForFinishedBuildsWasIgnored = false;
        private IBuildServerWatcher? _buildServerWatcher;
        private IntegrationSettings? _settings;
        private ApiClient? _apiClient;
        private static readonly IBuildDurationFormatter _buildDurationFormatter = new BuildDurationFormatter();
        private JoinableTask<string?>? _buildDefinitionsTask;
        private string? _projectUrl;
        private string? _buildDefinitions;

        // Static variable used to convey the data between the different instances of the class but that doesn't necessarily require synchronization because:
        // * there is only one instance at every given time (link to the revision grid and recreated on revision grid refresh)
        // * data is created by the first instance and used in readonly by the later instances
        private static BuildsCache? _buildsCache;
        private static string? _projectOnErrorKey;

        private readonly TranslationString _buildIntegrationErrorCaption = new("Azure DevOps error");
        private readonly TranslationString _openSettingsButton = new("Open settings");
        private readonly TranslationString _ignoreButton = new("Ignore");
        private readonly TranslationString _badTokenErrorMessage = new(@"The personal access token is invalid or has expired. Update it in the 'Build server integration' settings.

The build server integration has been disabled for this session.");
        private readonly TranslationString _genericErrorMessage = new(@"An error occurred when requesting build server results.

As a consequence, the build server integration has been disabled for this session.

Detail of the error:");

        private Action? _openSettings;
        private string CacheKey
        {
            get
            {
                Validates.NotNull(_settings);
                return _projectUrl + "|" + _settings.BuildDefinitionFilter;
            }
        }

        public void Initialize(IBuildServerWatcher buildServerWatcher, SettingsSource config, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
            if (_buildServerWatcher is not null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;
            _openSettings = openSettings;
            _settings = IntegrationSettings.ReadFrom(config);

            if (!_settings.IsValid())
            {
                return;
            }

            _projectUrl = _buildServerWatcher.ReplaceVariables(_settings.ProjectUrl);

            if (!Uri.IsWellFormedUriString(_projectUrl, UriKind.Absolute) || string.IsNullOrWhiteSpace(_settings.ApiToken))
            {
                return;
            }

            _apiClient = new ApiClient(_projectUrl, _settings.ApiToken);
            if (_buildsCache is null || _buildsCache.Id != CacheKey)
            {
                _buildsCache = null;
                _buildDefinitionsTask = ThreadHelper.JoinableTaskFactory.RunAsync(() => _apiClient.GetBuildDefinitionsAsync(_settings.BuildDefinitionFilter));
            }
            else
            {
                _buildDefinitions = _buildsCache.BuildDefinitions;
            }
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey => _settings?.ProjectUrl ?? throw new InvalidOperationException($"{nameof(AzureDevOpsAdapter)} is not yet initialized");

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            if (!_firstCallForFinishedBuildsWasIgnored)
            {
                _firstCallForFinishedBuildsWasIgnored = true;
                return Observable.Empty<BuildInfo>();
            }

            return GetBuilds(scheduler, sinceDate, false);
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
            => GetBuilds(scheduler, null, true);

        public void OnRepositoryChanged()
        {
            _buildsCache = null;
        }

        private IObservable<BuildInfo> GetBuilds(IScheduler scheduler, DateTime? sinceDate = null, bool running = false)
            => Observable.Create<BuildInfo>((observer, cancellationToken) => ObserveBuildsAsync(sinceDate, running, observer, cancellationToken));

        private async Task ObserveBuildsAsync(DateTime? sinceDate, bool running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            if (_apiClient is null)
            {
                observer.OnCompleted();
                return;
            }

            try
            {
                bool isBuildDefinitionsInitializedSuccessfully = await EnsureBuildDefinitionsIsInitializedAsync(observer);
                if (!isBuildDefinitionsInitializedSuccessfully || _buildsCache == null)
                {
                    return;
                }

                if (running)
                {
                    IEnumerable<Build> builds = await _apiClient.QueryRunningBuildsAsync(_buildDefinitions);
                    AggregateAndDisplayBuilds(builds, updateBuild: false);
                }
                else
                {
                    // Display cached builds results
                    if (!sinceDate.HasValue && _buildsCache.FinishedBuilds.Any())
                    {
                        foreach (KeyValuePair<ObjectId, BuildInfo> buildInfo in _buildsCache.FinishedBuilds)
                        {
                            observer.OnNext(buildInfo.Value);
                        }

                        // Reduce the scope to query only the builds finished after the last one in the cache
                        sinceDate = _buildsCache.LastCall;
                    }

                    IEnumerable<Build> builds = await _apiClient.QueryFinishedBuildsAsync(_buildDefinitions, sinceDate);
                    if (builds.Any())
                    {
                        _buildsCache.LastCall = builds.Max(b => b.FinishTime.Value).AddSeconds(1);
                    }

                    AggregateAndDisplayBuilds(builds, updateBuild: true);
                }

                void AggregateAndDisplayBuilds(IEnumerable<Build> builds, bool updateBuild)
                {
                    IEnumerable<IGrouping<string, Build>> buildsByCommit = builds.GroupBy(b => b.SourceVersion);

                    foreach (IGrouping<string, Build> buildsForACommit in buildsByCommit)
                    {
                        List<Build> buildsToDisplay = buildsForACommit.OrderByDescending(b => b.FinishTime).ToList();
                        Build buildToDisplay = buildsToDisplay.First();
                        BuildInfo buildInfo = CreateBuildInfo(buildToDisplay);

                        if (buildsToDisplay.Count > 1)
                        {
                            buildInfo.Tooltip += Environment.NewLine + string.Join(Environment.NewLine, buildsToDisplay.Skip(1).Select(b => CreateBuildTooltip(b).tooltip));
                        }

                        // Aggregate with previously finished builds results
                        ObjectId? buildHash = buildInfo.CommitHashList.First();
                        if (_buildsCache.FinishedBuilds.TryGetValue(buildHash, out BuildInfo finishedBuild))
                        {
                            buildInfo.Tooltip += Environment.NewLine + finishedBuild.Tooltip;
                        }

                        if (updateBuild)
                        {
                            _buildsCache.FinishedBuilds[buildHash] = buildInfo;
                        }

                        observer.OnNext(buildInfo);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the observer is already stopped
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }

        private async Task<bool> EnsureBuildDefinitionsIsInitializedAsync(IObserver<BuildInfo> observer)
        {
            if (_buildDefinitions is not null)
            {
                return true;
            }

            try
            {
                Validates.NotNull(_buildDefinitionsTask);

                _buildDefinitions = await _buildDefinitionsTask.JoinAsync();

                if (_buildDefinitions is null)
                {
                    observer.OnCompleted();
                    return false;
                }

                _buildsCache = new BuildsCache { Id = CacheKey, BuildDefinitions = _buildDefinitions };
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                string errorMessage = _badTokenErrorMessage.Text;
                DisplayInitializationErrorInRevisionGrid(errorMessage);

                if (_projectOnErrorKey is null || _projectOnErrorKey != CacheKey)
                {
                    _projectOnErrorKey = CacheKey;

                    TaskDialogButton btnOpenSettings = new(_openSettingsButton.Text);
                    TaskDialogButton btnIgnore = new(_ignoreButton.Text);
                    TaskDialogPage page = new()
                    {
                        Heading = errorMessage,
                        Icon = TaskDialogIcon.Error,
                        AllowCancel = true,
                        Caption = _buildIntegrationErrorCaption.Text,
                        Buttons = { btnOpenSettings, btnIgnore }
                    };

                    TaskDialogButton result = TaskDialog.ShowDialog(page);
                    if (result == btnOpenSettings)
                    {
                        _projectOnErrorKey = null;
                        Validates.NotNull(_openSettings);
                        _openSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{_genericErrorMessage.Text}{Environment.NewLine}{ex.Message}";
                DisplayInitializationErrorInRevisionGrid(errorMessage);

                if (_projectOnErrorKey is null || _projectOnErrorKey != CacheKey)
                {
                    _projectOnErrorKey = CacheKey;
                    MessageBox.Show(errorMessage, _buildIntegrationErrorCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return false;

            void DisplayInitializationErrorInRevisionGrid(string errorMessage)
            {
                _apiClient = null;
                observer.OnNext(new BuildInfo { CommitHashList = new[] { ObjectId.WorkTreeId }, Description = errorMessage, Status = BuildInfo.BuildStatus.Failure });
                observer.OnCompleted();
            }
        }

        private static (string duration, string tooltip) CreateBuildTooltip(Build buildDetail)
        {
            string duration = string.Empty;

            if (buildDetail.Status is not null
                && buildDetail.Status != "none"
                && buildDetail.Status != "notStarted"
                && buildDetail.Status != "postponed"
                && buildDetail.StartTime.HasValue)
            {
                if (buildDetail.Status == "inProgress")
                {
                    duration = _buildDurationFormatter.Format((long)(DateTime.UtcNow - buildDetail.StartTime.Value).TotalMilliseconds);
                }
                else
                {
                    duration = buildDetail.FinishTime.HasValue ? _buildDurationFormatter.Format((long)(buildDetail.FinishTime.Value - buildDetail.StartTime.Value).TotalMilliseconds) : "???";
                }
            }

            return (duration, ConvertResult(buildDetail.IsInProgress ? buildDetail.Status : buildDetail.Result) + " " + buildDetail.BuildNumber + " " + duration);
        }

        private static BuildInfo CreateBuildInfo(Build buildDetail)
        {
            (string duration, string tooltip) = CreateBuildTooltip(buildDetail);
            Validates.NotNull(buildDetail.SourceVersion);

            BuildInfo buildInfo = new()
            {
                Id = buildDetail.BuildNumber,
                StartDate = buildDetail.StartTime ?? DateTime.MinValue,
                Status = buildDetail.IsInProgress ? BuildInfo.BuildStatus.InProgress : MapResult(buildDetail.Result),
                Description = duration + " " + buildDetail.BuildNumber,
                Tooltip = tooltip,
                CommitHashList = new[] { ObjectId.Parse(buildDetail.SourceVersion) },
                Url = buildDetail._links?.Web?.Href,
                ShowInBuildReportTab = false
            };

            return buildInfo;
        }

        private static string ConvertResult(string? result)
        {
            return result switch
            {
                "failed" => "❌",
                "canceled" => "🛑",
                "succeeded" => "✔",
                "partiallySucceeded" => "❗",
                Build.StatusCancelling => "🛑",
                Build.StatusInProgress => "⚙",
                Build.StatusNotStarted => "⏸",
                Build.StatusPostponed => "⏱",
                _ => "❓"
            };
        }

        private static BuildInfo.BuildStatus MapResult(string? status)
        {
            return status switch
            {
                "failed" => BuildInfo.BuildStatus.Failure,
                "canceled" => BuildInfo.BuildStatus.Stopped,
                "succeeded" => BuildInfo.BuildStatus.Success,
                "partiallySucceeded" => BuildInfo.BuildStatus.Unstable,
                _ => BuildInfo.BuildStatus.Unknown
            };
        }

        public void Dispose()
        {
            _apiClient?.Dispose();
            GC.SuppressFinalize(this);
        }

        #region TestAccessor
        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            public AzureDevOpsAdapter AzureDevOpsAdapter { get; }

            public TestAccessor(AzureDevOpsAdapter azureDevOpsAdapter)
            {
                AzureDevOpsAdapter = azureDevOpsAdapter;
            }
        }
        #endregion
    }
}

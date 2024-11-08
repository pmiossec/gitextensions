using System.ComponentModel.Composition;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using GitCommands.Utils;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft;

namespace AppVeyorIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class AppVeyorIntegrationMetadataAttribute : BuildServerAdapterMetadataAttribute
    {
        public AppVeyorIntegrationMetadataAttribute(string buildServerType)
            : base(buildServerType)
        {
        }

        public override string? CanBeLoaded
        {
            get
            {
                if (EnvUtils.IsNet4FullOrHigher())
                {
                    return null;
                }

                return ".NET Framework 4 or higher required";
            }
        }
    }

    [Export(typeof(IBuildServerAdapter))]
    [AppVeyorIntegrationMetadataAttribute(PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class AppVeyorAdapter : IBuildServerAdapter
    {
        public const string PluginName = "AppVeyor";
        private const uint ProjectsToRetrieveCount = 25;
        private const string WebSiteUrl = "https://ci.appveyor.com";
        private const string ApiBaseUrl = WebSiteUrl + "/api/projects/";

        private IBuildServerWatcher? _buildServerWatcher;

        private HttpClient? _httpClientAppVeyor;

        private List<AppVeyorBuildInfo>? _allBuilds = [];
        private HashSet<ObjectId>? _fetchBuilds;
        private Func<ObjectId, bool>? _isCommitInRevisionGrid;
        private bool _shouldLoadTestResults;

        public void Initialize(
            IBuildServerWatcher buildServerWatcher,
            SettingsSource config,
            Action openSettings,
            Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
            if (_buildServerWatcher is not null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            _buildServerWatcher = buildServerWatcher;
            _isCommitInRevisionGrid = isCommitInRevisionGrid;
            string? accountName = config.GetString("AppVeyorAccountName", null);
            string? accountToken = config.GetString("AppVeyorAccountToken", null);
            _shouldLoadTestResults = config.GetBool("AppVeyorLoadTestsResults", false);

            _fetchBuilds = [];

            _httpClientAppVeyor = GetHttpClient(WebSiteUrl, accountToken);

            // projectId has format accountName/repoName
            // accountName may be any accessible project (for instance upstream)
            // if AppVeyorAccountName is set, projectNamesSetting may exclude the accountName part
            string projectNamesSetting = config.GetString("AppVeyorProjectName", "");
            List<string> projectNames = _buildServerWatcher.ReplaceVariables(projectNamesSetting)
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p.Contains("/") || !string.IsNullOrEmpty(accountName))
                .Select(p => p.Contains("/") ? p : accountName.Combine("/", p)!)
                .ToList();

            if (projectNames.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(accountToken))
                {
                    // No projectIds in settings, cannot query
                    return;
                }

                // No settings, query projects for this account

                // v2 tokens requires a separate prefix
                // (Documentation specifies that this is applicable for all requests, not the case though)
                string apiBaseUrl = !string.IsNullOrWhiteSpace(accountName) && !string.IsNullOrWhiteSpace(accountToken) && accountToken.StartsWith("v2.")
                    ? $"{WebSiteUrl}/api/account/{accountName}/projects/"
                    : ApiBaseUrl;

                // get the project ids for this account - no possibility to check if they are for the current repo
                string result = ThreadHelper.JoinableTaskFactory.Run(() => GetResponseAsync(_httpClientAppVeyor, apiBaseUrl, CancellationToken.None));
                if (!string.IsNullOrWhiteSpace(result))
                {
                    foreach (JsonNode project in ((JsonArray)JsonArray.Parse(result)).GetValues<JsonNode>())
                    {
                        // "slug" and "name" are normally the same
                        string repoName = project["slug"].ToString();
                        string projectId = accountName.Combine("/", repoName)!;
                        projectNames.Add(projectId);
                    }
                }
            }

            _allBuilds = FilterBuilds(projectNames.SelectMany(project => QueryBuildsResults(project)));

            return;

            static HttpClient GetHttpClient(string baseUrl, string? accountToken)
            {
                HttpClient httpClient = new(new HttpClientHandler { UseDefaultCredentials = true })
                {
                    Timeout = TimeSpan.FromMinutes(2),
                    BaseAddress = new Uri(baseUrl, UriKind.Absolute),
                };
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(accountToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accountToken);
                }

                return httpClient;
            }

            List<AppVeyorBuildInfo> FilterBuilds(IEnumerable<AppVeyorBuildInfo> allBuilds)
            {
                List<AppVeyorBuildInfo> filteredBuilds = [];
                foreach (AppVeyorBuildInfo build in allBuilds.OrderByDescending(b => b.StartDate))
                {
                    Validates.NotNull(build.CommitId);
                    if (!_fetchBuilds.Contains(build.CommitId))
                    {
                        filteredBuilds.Add(build);
                        _fetchBuilds.Add(build.CommitId);
                    }
                }

                return filteredBuilds;
            }
        }

        private IEnumerable<AppVeyorBuildInfo> QueryBuildsResults(string projectId)
        {
            try
            {
                Validates.NotNull(_httpClientAppVeyor);
                string queryUrl = $"{ApiBaseUrl}{projectId}/history?recordsNumber={ProjectsToRetrieveCount}";
                string result = ThreadHelper.JoinableTaskFactory.Run(() => GetResponseAsync(_httpClientAppVeyor, queryUrl, CancellationToken.None));
                return ExtractBuildInfo(projectId, result);
            }
            catch
            {
                return Enumerable.Empty<AppVeyorBuildInfo>();
            }
        }

        internal IEnumerable<AppVeyorBuildInfo> ExtractBuildInfo(string projectId, string? result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return Enumerable.Empty<AppVeyorBuildInfo>();
            }

            Validates.NotNull(_isCommitInRevisionGrid);

            JsonNode content = JsonNode.Parse(result);

            JsonNode projectData = content["project"];
            JsonNode repositoryName = projectData["repositoryName"];
            JsonNode repositoryType = projectData["repositoryType"];

            JsonArray builds = content["builds"].AsArray();
            string baseWebUrl = $"{WebSiteUrl}/project/{projectId}/build/";
            string baseApiUrl = $"{ApiBaseUrl}{projectId}/";

            List<AppVeyorBuildInfo> buildDetails = [];
            foreach (JsonNode b in builds)
            {
                try
                {
                    if (!ObjectId.TryParse((b["pullRequestHeadCommitId"] ?? b["commitId"]).GetValue<string>(),
                            out ObjectId? objectId) || !_isCommitInRevisionGrid(objectId))
                    {
                        continue;
                    }

                    JsonNode pullRequestId = b["pullRequestId"];
                    string version = b["version"].GetValue<string>();
                    BuildStatus status = ParseBuildStatus(b["status"].GetValue<string>());
                    long? duration = null;
                    if (status is (BuildStatus.Success or BuildStatus.Failure))
                    {
                        duration = GetBuildDuration(b);
                    }

                    JsonNode pullRequestTitle = b["pullRequestName"];

                    buildDetails.Add(new AppVeyorBuildInfo
                    {
                        Id = version,
                        BuildId = b["buildId"]!.ToString(),
                        Branch = b["branch"]!.ToString(),
                        CommitId = objectId,
                        CommitHashList = new[] { objectId },
                        Status = status,
                        StartDate = b["started"]?.GetValue<DateTime>() ?? DateTime.MinValue,
                        BaseWebUrl = baseWebUrl,
                        Url = baseWebUrl + version,
                        PullRequestUrl = repositoryType is not null && repositoryName is not null && pullRequestId is not null
                            ? BuildPullRequetUrl(repositoryType.GetValue<string>(), repositoryName.GetValue<string>(),
                                pullRequestId.GetValue<string>())
                            : null,
                        BaseApiUrl = baseApiUrl,
                        AppVeyorBuildReportUrl = baseApiUrl + "build/" + version,
                        PullRequestText = pullRequestId is not null ? "PR#" + pullRequestId.GetValue<string>() : string.Empty,
                        PullRequestTitle = pullRequestTitle is not null ? pullRequestTitle.GetValue<string>() : string.Empty,
                        Duration = duration,
                        TestsResultText = string.Empty
                    });
                }
                catch (Exception)
                {
                    // Failure on reading data on a build detail should not prevent to display the others build results
                }
            }

            return buildDetails;

            static string? BuildPullRequetUrl(string repositoryType, string repositoryName, string pullRequestId)
            {
                return repositoryType.ToLowerInvariant() switch
                {
                    "bitbucket" => $"https://bitbucket.org/{repositoryName}/pull-requests/{pullRequestId}",
                    "github" => $"https://github.com/{repositoryName}/pull/{pullRequestId}",
                    "gitlab" => $"https://gitlab.com/{repositoryName}/merge_requests/{pullRequestId}",
                    "vso" => null,
                    "git" => null,
                    _ => null
                };
            }
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey
        {
            get
            {
                Validates.NotNull(_httpClientAppVeyor);
                return _httpClientAppVeyor.BaseAddress.Host;
            }
        }

        public IObservable<IBuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            // AppVeyor api is different than TeamCity one and all build results are fetch in one call without
            // filter parameters possible (so this call is useless!)
            return Observable.Empty<IBuildInfo>();
        }

        public IObservable<IBuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler);
        }

        private IObservable<BuildInfo> GetBuilds(IScheduler scheduler)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task.Run(
                    () => scheduler.Schedule(() => ObserveBuilds(observer, cancellationToken))));
        }

        private void ObserveBuilds(IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                if (_allBuilds is null)
                {
                    // builds are updated, no requery for new builds
                    return;
                }

                // Display all builds found
                foreach (AppVeyorBuildInfo build in _allBuilds)
                {
                    // Update finished build with tests results
                    if (_shouldLoadTestResults
                        && (build.Status == BuildStatus.Success
                            || build.Status == BuildStatus.Failure))
                    {
                        UpdateDescription(build, cancellationToken);
                    }

                    UpdateDisplay(observer, build);
                }

                // Manage in progress builds...
                List<AppVeyorBuildInfo> inProgressBuilds = _allBuilds.Where(b => b.Status == BuildStatus.InProgress).ToList();

                // Reset current build list - refresh required to see new builds
                _allBuilds = null;
                while (inProgressBuilds.Any() && !cancellationToken.IsCancellationRequested)
                {
                    const int inProgressRefresh = 10000;
                    Thread.Sleep(inProgressRefresh);
                    foreach (AppVeyorBuildInfo build in inProgressBuilds)
                    {
                        UpdateDescription(build, cancellationToken);
                        UpdateDisplay(observer, build);
                    }

                    inProgressBuilds = inProgressBuilds.Where(b => b.Status == BuildStatus.InProgress).ToList();
                }

                observer.OnCompleted();
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

        private static void UpdateDisplay(IObserver<BuildInfo> observer, AppVeyorBuildInfo build)
        {
            build.UpdateDescription();
            observer.OnNext(build);
        }

        private void UpdateDescription(AppVeyorBuildInfo buildDetails, CancellationToken cancellationToken)
        {
            JsonNode buildDetailsParsed = ThreadHelper.JoinableTaskFactory.Run(() => FetchBuildDetailsManagingVersionUpdateAsync(buildDetails, cancellationToken));
            if (buildDetailsParsed is null)
            {
                return;
            }

            JsonNode buildData = buildDetailsParsed["build"];
            IList<JsonNode> buildJobs = (JsonArray)buildData["jobs"];
            JsonNode buildDescription = buildJobs[^1];

            string status = buildDescription["status"].GetValue<string>();
            buildDetails.Status = ParseBuildStatus(status);

            if (!buildDetails.IsRunning)
            {
                buildDetails.Duration = GetBuildDuration(buildData);
            }

            int runTests = buildDescription["passedTestsCount"].GetValue<int>();
            if (runTests != 0)
            {
                int failedTestCount = buildDescription["failedTestsCount"].GetValue<int>();
                buildDetails.TestsResultText = failedTestCount != 0
                    ? $"{failedTestCount} failed / {runTests} tests run"
                    : $"{runTests} tests run";
            }
        }

        private static long GetBuildDuration(JsonNode buildData)
        {
            DateTime? startTime = (buildData["started"] ?? buildData["created"])?.GetValue<DateTime>();
            DateTime? updateTime = buildData["updated"]?.GetValue<DateTime>();
            if (!startTime.HasValue || !updateTime.HasValue)
            {
                return 0;
            }

            return (long)(updateTime.Value - startTime.Value).TotalMilliseconds;
        }

        private async Task<JsonNode> FetchBuildDetailsManagingVersionUpdateAsync(AppVeyorBuildInfo buildDetails, CancellationToken cancellationToken)
        {
            Validates.NotNull(_httpClientAppVeyor);

            try
            {
                Validates.NotNull(buildDetails.AppVeyorBuildReportUrl);
                return JsonNode.Parse(await GetResponseAsync(_httpClientAppVeyor, buildDetails.AppVeyorBuildReportUrl, cancellationToken).ConfigureAwait(false));
            }
            catch
            {
                string buildHistoryUrl = buildDetails.BaseApiUrl + "/history?recordsNumber=1&startBuildId=" + (int.Parse(buildDetails.BuildId) + 1);
                JsonNode builds = JsonNode.Parse(await GetResponseAsync(_httpClientAppVeyor, buildHistoryUrl, cancellationToken).ConfigureAwait(false));

                string version = builds["builds"][0]["version"].GetValue<string>();
                buildDetails.Id = version;
                buildDetails.AppVeyorBuildReportUrl = buildDetails.BaseApiUrl + "/build/" + version;
                buildDetails.Url = buildDetails.BaseWebUrl + version;

                return JsonNode.Parse(await GetResponseAsync(_httpClientAppVeyor, buildDetails.AppVeyorBuildReportUrl, cancellationToken).ConfigureAwait(false));
            }
        }

        private static BuildStatus ParseBuildStatus(string statusValue)
        {
            return statusValue switch
            {
                "success" => BuildStatus.Success,
                "failed" => BuildStatus.Failure,
                "cancelled" => BuildStatus.Stopped,
                "queued" or "running" => BuildStatus.InProgress,
                _ => BuildStatus.Unknown
            };
        }

        private Task<Stream?> GetStreamAsync(HttpClient httpClient, string restServicePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return httpClient.GetAsync(restServicePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                             .ContinueWith(
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                                 task => GetStreamFromHttpResponseAsync(httpClient, task, restServicePath, cancellationToken),
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
                                 cancellationToken,
                                 restServicePath.Contains("github") ? TaskContinuationOptions.None : TaskContinuationOptions.AttachedToParent,
                                 TaskScheduler.Current)
                             .Unwrap();
        }

        private Task<Stream?> GetStreamFromHttpResponseAsync(HttpClient httpClient, Task<HttpResponseMessage> task, string restServicePath, CancellationToken cancellationToken)
        {
            bool retry = task.IsCanceled && !cancellationToken.IsCancellationRequested;

            if (retry)
            {
                return GetStreamAsync(httpClient, restServicePath, cancellationToken);
            }

            if (task.Status == TaskStatus.RanToCompletion && task.CompletedResult().IsSuccessStatusCode)
            {
                return task.CompletedResult().Content.ReadAsStreamAsync();
            }

            return Task.FromResult<Stream?>(null);
        }

        private Task<string> GetResponseAsync(HttpClient httpClient, string relativePath, CancellationToken cancellationToken)
        {
            Task<Stream?> getStreamTask = GetStreamAsync(httpClient, relativePath, cancellationToken);

            TaskContinuationOptions taskContinuationOptions = relativePath.Contains("github") ? TaskContinuationOptions.None : TaskContinuationOptions.AttachedToParent;
            return getStreamTask.ContinueWith(
                task =>
                {
                    if (task.Status != TaskStatus.RanToCompletion)
                    {
                        return string.Empty;
                    }

                    using Stream responseStream = task.Result;

                    if (responseStream is null)
                    {
                        return "";
                    }

                    return new StreamReader(responseStream).ReadToEnd();
                },
                cancellationToken,
                taskContinuationOptions,
                TaskScheduler.Current);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _httpClientAppVeyor?.Dispose();
        }
    }
}

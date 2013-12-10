#define GITTFS
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using TfsInterop.Interface;

namespace TfsIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TfsIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public TfsIntegrationMetadata(string buildServerType)
            : base(buildServerType)
        {
        }

        public override string CanBeLoaded
        {
            get
            {
                if (EnvUtils.IsNet4FullOrHigher())
                {
                    return null;
                }

                return ".Net 4 full framework required";
            }
        }
    }

    [Export(typeof(IBuildServerAdapter))]
    [TfsIntegrationMetadata("Team Foundation Server")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class TfsAdapter : IBuildServerAdapter
    {
        private IBuildServerWatcher _buildServerWatcher;
        private ITfsHelper _tfsHelper;
        private string _tfsServer;
        private string _tfsTeamCollectionName;
        private string _projectName;
        private Regex _tfsBuildDefinitionNameFilter;
        private static bool _debugActivated = false;
#if GITTFS
        private static string WorkingDir { get; set; }
#endif

        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Func<string, bool> isCommitInRevisionGrid)
        {
            if (_buildServerWatcher != null)
            {
                throw new InvalidOperationException("Already initialized");

            if (_debugActivated)
            {
                _debugActivated = true;
                var filename = string.Format("TfsAdapter_debug_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);

                Trace.Listeners.Add(new TextWriterTraceListener(filePath));
            }

            _buildServerWatcher = buildServerWatcher;

#if GITTFS
            WorkingDir = _buildServerWatcher.Get<GitCommands.GitModule>("Module").WorkingDir;
#endif

            _tfsServer = config.GetString("TfsServer", null);
            _tfsTeamCollectionName = config.GetString("TfsTeamCollectionName", null);
            _projectName = _buildServerWatcher.ReplaceVariables(config.GetString("ProjectName", null));
            var tfsBuildDefinitionNameFilterSetting = config.GetString("TfsBuildDefinitionName", "");
            if (!BuildServerSettingsHelper.IsRegexValid(tfsBuildDefinitionNameFilterSetting))
            {
                return;
            }

            _tfsBuildDefinitionNameFilter = new Regex(tfsBuildDefinitionNameFilterSetting, RegexOptions.Compiled);

            if (!string.IsNullOrEmpty(_tfsServer)
                && !string.IsNullOrEmpty(_tfsTeamCollectionName)
                && !string.IsNullOrEmpty(_projectName))
            {
                _tfsHelper = LoadAssemblyAndConnectToServer("TfsInterop.Vs2015")
                    ?? LoadAssemblyAndConnectToServer("TfsInterop.Vs2013")
                    ?? LoadAssemblyAndConnectToServer("TfsInterop.Vs2012");

                if (_tfsHelper == null)
                {
                    Trace.WriteLine("fail to load the good interop assembly :(");
                }
            }
        }

        private ITfsHelper LoadAssemblyAndConnectToServer(string assembly)
        {
            try
            {
                Trace.WriteLine("Try loading " + assembly + ".dll ...");
                var loadedAssembly = Assembly.Load(assembly);
                var tfsHelper = loadedAssembly.CreateInstance("TfsInterop.TfsHelper") as ITfsHelper;
                Trace.WriteLine("Create instance... OK");

                if (tfsHelper != null && tfsHelper.IsDependencyOk())
                {
                    tfsHelper.ConnectToTfsServer(_tfsServer, _tfsTeamCollectionName, _projectName, _tfsBuildDefinitionNameFilter);
                    Trace.WriteLine("Connection... OK");
                    return tfsHelper;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey => _tfsServer + "/" + _tfsTeamCollectionName + "/" + _projectName;

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            return GetBuilds(scheduler, sinceDate, false);
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler, null, true);
        }

        public IObservable<BuildInfo> GetBuilds(IScheduler scheduler, DateTime? sinceDate = null, bool? running = null)
        {
            if (_tfsHelper == null)
            {
                return Observable.Empty<BuildInfo>();
            }

            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task.Run(
                    () => scheduler.Schedule(() => ObserveBuilds(sinceDate, running, observer, cancellationToken))));
        }

        private void ObserveBuilds(DateTime? sinceDate, bool? running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                var builds = _tfsHelper.QueryBuilds(sinceDate, running);

                Parallel.ForEach(builds, detail => { observer.OnNext(CreateBuildInfo(detail)); });
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

        private static BuildInfo CreateBuildInfo(IBuild buildDetail)
        {
            string sha;
#if GITTFS
            if (!buildDetail.Revision.Contains(":"))
            {
                //From TFS Reprository (using git-tfs)
                ////git log -g --grep=";C13" --pretty="%H"
                var p = new Process
                    {
                        StartInfo =
                            {
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                FileName = @"git.exe",
                                Arguments = "log --all --grep=\";" + buildDetail.Revision + "\" --pretty=\"%H\"",
                                WorkingDirectory = WorkingDir
                            }
                    };
                p.Start();
                sha = p.StandardOutput.ReadLine();
                p.WaitForExit();
            }
            else
            {
                //From Git Repository
#endif
                sha = buildDetail.Revision.Substring(buildDetail.Revision.LastIndexOf(":") + 1);
#if GITTFS
            }
#endif

            var buildInfo = new BuildInfo
            {
                Id = buildDetail.Label,
                StartDate = buildDetail.StartDate,
                Status = (BuildInfo.BuildStatus)buildDetail.Status,
                Description = buildDetail.Label + " (" + buildDetail.Description + ")",
                CommitHashList = new[] { sha },
                Url = buildDetail.Url,
                ShowInBuildReportTab = false
            };

            return buildInfo;
        }

        public void Dispose()
        {
            _tfsHelper?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

#if GITTFS
    public static class ObjectExtension
    {
        public static T Get<T>(this object obj, string name)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo field = type.GetField(name, flags);
            if (field != null)
                return (T)field.GetValue(obj);

            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null)
                return (T)property.GetValue(obj, null);

            return default(T);
        }
    }
#endif
}

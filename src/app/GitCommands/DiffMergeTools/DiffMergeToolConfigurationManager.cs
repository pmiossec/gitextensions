﻿using GitCommands.Config;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;

namespace GitCommands.DiffMergeTools
{
    public sealed class DiffMergeToolConfigurationManager
    {
        private readonly Func<IConfigValueStore?> _getFileSettings;
        private readonly Func<string, IEnumerable<string?>, string> _findFileInFolders;

        public DiffMergeToolConfigurationManager(Func<IConfigValueStore?> getFileSettings)
            : this(getFileSettings, PathUtil.FindInFolders)
        {
            _getFileSettings = getFileSettings;
        }

        internal DiffMergeToolConfigurationManager(Func<IConfigValueStore?> getFileSettings, Func<string, IEnumerable<string?>, string> findFileInFolders)
        {
            _getFileSettings = getFileSettings;
            _findFileInFolders = findFileInFolders;
        }

        /// <summary>
        /// Gets the diff tool configured in the effective config under 'diff.guitool'.
        /// </summary>
        public string? ConfiguredDiffTool => _getFileSettings()?.GetValue(SettingKeyString.DiffToolKey);

        /// <summary>
        /// Gets the merge tool configured in the effective config under 'merge.guitool' or 'merge.tool'.
        /// </summary>
        public string? ConfiguredMergeTool
        {
            get
            {
                // Only used in settings, returns native (Windows) Git tool
                string? mergetool = "";
                if (GitVersion.Current.SupportGuiMergeTool)
                {
                    mergetool = _getFileSettings()?.GetValue(SettingKeyString.MergeToolKey);
                }

                // Fallback and older Git
                if (string.IsNullOrEmpty(mergetool))
                {
                    mergetool = _getFileSettings()?.GetValue(SettingKeyString.MergeToolNoGuiKey);
                }

                return mergetool;
            }
        }

        /// <summary>
        /// Configures diff/merge tool.
        /// </summary>
        /// <param name="toolName">The name of the diff/merge tool.</param>
        /// <param name="toolType">Type of the tool.</param>
        /// <param name="toolPath">The location of the tool's executable.</param>
        /// <param name="toolCommand">The command.</param>
        public void ConfigureDiffMergeTool(string toolName, DiffMergeToolType toolType, string toolPath, string toolCommand)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                DebugHelpers.Fail("Diff/merge tool is required");
                return;
            }

            IConfigValueStore fileSettings = _getFileSettings();
            if (fileSettings is null)
            {
                return;
            }

            (string toolKey, string prefix) = GetInfo(toolType);
            fileSettings.SetValue(toolKey, toolName);
            fileSettings.SetPathValue(string.Concat(prefix, ".", toolName, ".path"), toolPath);
            fileSettings.SetPathValue(string.Concat(prefix, ".", toolName, ".cmd"), toolCommand);
        }

        /// <summary>
        /// Gets the command for the diff/merge tool configured in the effective config.
        /// </summary>
        /// <param name="toolName">The name of the diff/merge tool.</param>
        /// <param name="toolType">Type of the tool.</param>
        /// <returns>The command for the diff/merge tool configured in the effective config. </returns>
        public string GetToolCommand(string? toolName, DiffMergeToolType toolType)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                return string.Empty;
            }

            string command = GetToolSetting(toolName, toolType, "cmd");
            if (!string.IsNullOrWhiteSpace(command))
            {
                return command;
            }

            DiffMergeToolConfiguration config = LoadDiffMergeToolConfig(toolName, null);

            return toolType switch
            {
                DiffMergeToolType.Diff => config.FullDiffCommand,
                DiffMergeToolType.Merge => config.FullMergeCommand,
                _ => throw new NotSupportedException()
            };
        }

        /// <summary>
        /// Gets the path to the diff/merge tool configured in the effective config.
        /// </summary>
        /// <param name="toolName">The name of the diff/merge tool.</param>
        /// <param name="toolType">Type of the tool.</param>
        /// <returns>The path to the diff/merge tool configured in the effective config. </returns>
        public string GetToolPath(string? toolName, DiffMergeToolType toolType)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                return string.Empty;
            }

            string path = GetToolSetting(toolName, toolType, "path");
            if (!string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            return LoadDiffMergeToolConfig(toolName, null).Path;
        }

        public DiffMergeToolConfiguration LoadDiffMergeToolConfig(string toolName, string? userSuppliedPath)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                throw new ArgumentException(@"Invalid diff/merge tool requested", nameof(toolName));
            }

            string? fullPath;

            DiffMergeTool diffTool = RegisteredDiffMergeTools.Get(toolName);
            if (diffTool is null)
            {
                string exeName = toolName + ".exe";
                if (!string.IsNullOrWhiteSpace(userSuppliedPath))
                {
                    fullPath = userSuppliedPath;
                }
                else
                {
                    PathUtil.TryFindFullPath(exeName, out fullPath);
                }

                return new DiffMergeToolConfiguration(exeName, fullPath ?? string.Empty, null, null);
            }

            if (!string.IsNullOrWhiteSpace(userSuppliedPath))
            {
                fullPath = userSuppliedPath;
            }
            else
            {
                // query static settings for defined fullPath to executable
                string? command = GetToolSetting(diffTool.Name, DiffMergeToolType.Merge, "path")?.RemoveQuotes();
                if (!string.IsNullOrWhiteSpace(command))
                {
                    fullPath = command;
                }
                else
                {
                    // look for executable in (default) search paths
                    fullPath = _findFileInFolders(diffTool.ExeFileName, diffTool.SearchPaths);
                }
            }

            // last resort fallback to avoid empty string for executable
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                fullPath = diffTool.ExeFileName;
            }

            return new DiffMergeToolConfiguration(diffTool.ExeFileName, fullPath, diffTool.DiffCommand, diffTool.MergeCommand);
        }

        /// <summary>
        /// Unset currently configured diff/merge tool.
        /// </summary>
        /// <param name="toolType">Type of the tool.</param>
        public void UnsetCurrentTool(DiffMergeToolType toolType)
        {
            IConfigValueStore fileSettings = _getFileSettings();
            if (fileSettings is null)
            {
                return;
            }

            (string toolKey, string _) = GetInfo(toolType);
            fileSettings.SetValue(toolKey, "");
        }

        private static (string toolKey, string prefix) GetInfo(DiffMergeToolType toolType)
        {
            return toolType switch
            {
                DiffMergeToolType.Diff => (SettingKeyString.DiffToolKey, "difftool"),
                DiffMergeToolType.Merge => (SettingKeyString.MergeToolKey, "mergetool"),
                _ => throw new NotSupportedException()
            };
        }

        private string? GetToolSetting(string? toolName, DiffMergeToolType toolType, string settingSuffix)
        {
            (string toolKey, string prefix) = GetInfo(toolType);

            if (string.IsNullOrWhiteSpace(toolName))
            {
                toolName = _getFileSettings()?.GetValue(toolKey);
            }

            return string.IsNullOrWhiteSpace(toolName) ?
                string.Empty :
                _getFileSettings()?.GetValue(string.Concat(prefix, ".", toolName, ".", settingSuffix));
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly DiffMergeToolConfigurationManager _manager;

            public TestAccessor(DiffMergeToolConfigurationManager manager)
            {
                _manager = manager;
            }

            public (string toolKey, string prefix) GetInfo(DiffMergeToolType toolType)
                => DiffMergeToolConfigurationManager.GetInfo(toolType);

            public string? GetToolSetting(string? toolName, DiffMergeToolType toolType, string settingSuffix)
                => _manager.GetToolSetting(toolName, toolType, settingSuffix);
        }
    }
}

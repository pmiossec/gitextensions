using System;
using System.ComponentModel.Composition;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildServerAdapterMetadataAttribute : ExportAttribute
    {
        public BuildServerAdapterMetadataAttribute(string buildServerType, string settingsKey)
            : base(typeof(IBuildServerTypeMetadata))
        {
            if (string.IsNullOrEmpty(buildServerType))
            {
                throw new ArgumentException();
            }

            BuildServerType = buildServerType;
            SettingsKey = settingsKey;
        }

        public string BuildServerType { get; }
        public string SettingsKey { get; }

        [CanBeNull]
        public virtual string CanBeLoaded => null;
    }
}
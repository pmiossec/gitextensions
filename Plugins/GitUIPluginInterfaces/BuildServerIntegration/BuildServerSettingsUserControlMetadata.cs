using System;
using System.ComponentModel.Composition;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildServerSettingsUserControlMetadata : BuildServerAdapterMetadataAttribute
    {
        public BuildServerSettingsUserControlMetadata(string buildServerType, string settingsKey)
            : base(buildServerType, settingsKey)
        {
        }
    }
}
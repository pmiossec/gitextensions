namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerTypeMetadata
    {
        string BuildServerType { get; }
        string SettingsKey { get; }

        /// <summary>
        /// returns null if can be loaded, the reason if can't
        /// </summary>
        string CanBeLoaded { get; }
    }
}
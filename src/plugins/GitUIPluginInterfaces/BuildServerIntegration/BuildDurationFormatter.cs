namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildDurationFormatter
    {
        string Format(long? durationMilliseconds, bool wrapWithParenthesis = true);
    }

    public class BuildDurationFormatter : IBuildDurationFormatter
    {
        public string Format(long? durationMilliseconds, bool wrapWithParenthesis)
        {
            if (durationMilliseconds.HasValue)
            {
                string timeText = TimeSpan.FromMilliseconds(durationMilliseconds.Value).ToString(@"mm\:ss");
                return wrapWithParenthesis ? $"({timeText})" : timeText;
            }

            return string.Empty;
        }
    }
}

namespace GitUI
{
    public sealed class WindowsThumbnailToolbarButtons
    {
        public WindowsThumbnailToolbarButtons(WindowsThumbnailToolbarButton commit, WindowsThumbnailToolbarButton fetch, WindowsThumbnailToolbarButton pull, WindowsThumbnailToolbarButton push)
        {
            Commit = commit;
            Fetch = fetch;
            Pull = pull;
            Push = push;
        }

        public WindowsThumbnailToolbarButton Commit { get; }
        public WindowsThumbnailToolbarButton Fetch { get; }
        public WindowsThumbnailToolbarButton Pull { get; }
        public WindowsThumbnailToolbarButton Push { get; }
    }
}

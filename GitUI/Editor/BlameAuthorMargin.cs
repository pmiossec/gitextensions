using GitExtUtils.GitUI;
using GitUI.Properties;
using ICSharpCode.TextEditor;

namespace GitUI.Editor
{
    /// <summary>
    /// This class display avatars in the gutter in a blame control.
    /// </summary>
    public class BlameAuthorMargin : AbstractMargin
    {
        private static readonly int AgeBucketMarkerWidth = Convert.ToInt32(4 * DpiUtil.ScaleX);
        private Task<Image?>[]? _avatars;
        private Image?[] _avatarsToDisplay;
        private readonly Color _backgroundColor;
        private List<GitBlameEntry>? _blameLines;
        private readonly Dictionary<int, SolidBrush> _brushs = new();
        private bool _isVisible = true;
        private Image _noAuthorImage;

        public BlameAuthorMargin(TextArea textArea) : base(textArea)
        {
            _backgroundColor = SystemColors.Window;
        }

        public int LineHeight => textArea.TextView.FontHeight;
        public override int Width => LineHeight + AgeBucketMarkerWidth + DpiUtil.Scale(2);

        public override bool IsVisible => _isVisible;

        public void Initialize(IEnumerable<GitBlameEntry> blameLines)
        {
            _noAuthorImage = (Image)new Bitmap(Images.User80, LineHeight, LineHeight);
            _blameLines = blameLines.ToList();
            _avatars = _blameLines.Select(a => a.Avatar).ToArray();
            _avatarsToDisplay = new Image?[_avatars.Length];

            // Build brushes
            foreach (var blameLine in _blameLines)
            {
                if (!_brushs.ContainsKey(blameLine.AgeBucketIndex))
                {
                    _brushs.Add(blameLine.AgeBucketIndex, new SolidBrush(blameLine.AgeBucketColor));
                }
            }

            Task.Run(async () =>
            {
                bool stillAvatarToProcess;

                do
                {
                    stillAvatarToProcess = false;
                    for (int i = 0; i < _avatarsToDisplay.Length; i++)
                    {
                        Task<Image?> avatarTask = _avatars[i];
                        if (_avatarsToDisplay[i] == null && avatarTask != null)
                        {
                            if (avatarTask.IsCompleted)
                            {
                                var avatar = await avatarTask.ConfigureAwait(true);
                                if (avatar is Bitmap bitmapAvatar)
                                {
                                    bitmapAvatar.SetResolution(DpiUtil.DpiX, DpiUtil.DpiY);
                                }

                                _avatarsToDisplay[i] = avatar;
                            }
                            else
                            {
                                stillAvatarToProcess = true;
                            }
                        }
                    }

                    await Task.Delay(200).ConfigureAwait(true);
                }
                while (stillAvatarToProcess);
            }).FileAndForget();

            ////var firstLineAvatar = _avatars.First();

            // Update the resolution otherwise the image is not drawn at the good size :(
            ////foreach (var avatar in _avatars)
            ////{
            ////    if (avatar is Bitmap bitmapAvatar)
            ////    {
            ////        bitmapAvatar.SetResolution(DpiUtil.DpiX, DpiUtil.DpiY);
            ////    }
            ////}
        }

        public void SetVisiblity(bool isVisible)
        {
            _isVisible = isVisible;
        }

        public override void Paint(Graphics g, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0 || _blameLines is null || _blameLines.Count == 0)
            {
                return;
            }

            g.Clear(_backgroundColor);

            if (_avatars is null || _avatars.Length == 0)
            {
                return;
            }

            var verticalOffset = textArea.VirtualTop.Y;
            var lineStart = verticalOffset / LineHeight;
            var negativeOffset = (lineStart * LineHeight) - verticalOffset;
            var lineCount = (int)Math.Ceiling((double)(rect.Height - negativeOffset) / LineHeight);

            for (int i = 0; i < lineCount; i++)
            {
                if (lineStart + i >= _avatars.Length)
                {
                    break;
                }

                int y = negativeOffset + (i * LineHeight);
                g.FillRectangle(_brushs[_blameLines[lineStart + i].AgeBucketIndex], 0, y, AgeBucketMarkerWidth, LineHeight);

                if (_avatars[lineStart + i] is not null)
                {
                    g.DrawImage(_avatarsToDisplay[lineStart + i] ?? _noAuthorImage, new Point(AgeBucketMarkerWidth, y));
                }
            }

            base.Paint(g, rect);
        }
    }
}

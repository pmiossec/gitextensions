using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace GitUI
{
    internal sealed class RevisionGridToolTipProvider
    {
        ////private readonly ToolTip _toolTip = new ToolTip();
        private readonly HtmlToolTip _toolTip = new HtmlToolTip();

        private readonly Dictionary<Point, bool> _isTruncatedByCellPos = new Dictionary<Point, bool>();
        private readonly RevisionDataGridView _gridView;

        public RevisionGridToolTipProvider(RevisionDataGridView gridView)
        {
            _gridView = gridView;
        }

        public void OnCellMouseEnter()
        {
            _toolTip.Active = false;
            _toolTip.AutoPopDelay = 32767;
        }

        private const string TooltipStyle = @"<style type=""text/css"">
 .branch { color: DarkRed; font-weight: bold; }
 .remote { color: Green; font-weight: bold; }
 .hash   { color: Gray; }
 .tag    { color: DarkBlue; font-weight: bold; }
</style>";

        public void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
        {
            var revision = _gridView.GetRevision(e.RowIndex);

            if (revision == null)
            {
                return;
            }

            var oldText = _toolTip.GetToolTip(_gridView);
            var newText = GetToolTipText();
            newText = newText.Replace("\n", "<br>\n");
            if (!string.IsNullOrWhiteSpace(newText))
            {
                newText += TooltipStyle;
            }

            if (newText != oldText)
            {
                _toolTip.SetToolTip(_gridView, newText);
            }

            if (!_toolTip.Active)
            {
                _toolTip.Active = true;
            }

            return;

            string GetToolTipText()
            {
                if (_gridView.Columns[e.ColumnIndex].Tag is ColumnProvider provider &&
                    provider.TryGetToolTip(e, revision, out var toolTip) &&
                    !string.IsNullOrWhiteSpace(toolTip))
                {
                    int lineCount = 0;
                    for (int pos = 0, length = toolTip.Length; pos < length; ++pos)
                    {
                        if (toolTip[pos] == '\n' && ++lineCount == 30)
                        {
                            return toolTip.Substring(0, pos + 1) + ">>> TOOLTIP TRUNCATED <<<";
                        }
                    }

                    return toolTip;
                }

                if (_isTruncatedByCellPos.TryGetValue(new Point(e.ColumnIndex, e.RowIndex), out var showToolTip)
                    && showToolTip)
                {
                    return _gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue?.ToString() ?? "";
                }

                // no tooltip unless always active or truncated
                return "";
            }
        }

        public void Clear()
        {
            _isTruncatedByCellPos.Clear();
        }

        public void SetTruncation(int columnIndex, int rowIndex, bool truncated)
        {
            _isTruncatedByCellPos[new Point(columnIndex, rowIndex)] = truncated;
        }
    }
}
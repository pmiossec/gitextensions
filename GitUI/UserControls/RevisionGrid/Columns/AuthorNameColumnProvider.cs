using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class AuthorNameColumnProvider : ColumnProvider
    {
        private readonly RevisionGridControl _grid;
        private readonly AuthorRevisionHighlighting _authorHighlighting;

        public AuthorNameColumnProvider(RevisionGridControl grid, AuthorRevisionHighlighting authorHighlighting)
            : base("Author Name")
        {
            _grid = grid;
            _authorHighlighting = authorHighlighting;

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                HeaderText = "Author Name",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = DpiUtil.Scale(130),
                MinimumWidth = DpiUtil.Scale(25)
            };
        }

        public override void Refresh(int rowHeight, in VisibleRowRange range) => Column.Visible = AppSettings.ShowAuthorNameColumn;

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            if (!revision.IsArtificial)
            {
                Font font = _authorHighlighting.IsHighlighted(revision) ? style.BoldFont : style.NormalFont;

                _grid.DrawColumnText(e, (string)e.FormattedValue, font, style.ForeColor, e.CellBounds.ReduceLeft(ColumnLeftMargin));
            }
        }

        public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            e.Value = revision.Author ?? "";
            e.FormattingApplied = true;
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
            => TryGetAuthorAndCommitterToolTip(revision, out toolTip);

        public static bool TryGetAuthorAndCommitterToolTip(GitRevision revision, out string? toolTip)
        {
            if (revision.ObjectId.IsArtificial)
            {
                toolTip = default;
                return false;
            }

            if (revision.AuthorDate == revision.CommitDate && revision.Author == revision.Committer && revision.AuthorEmail == revision.CommitterEmail)
            {
                toolTip = $"{revision.AuthorDate:g} {revision.Author} <{revision.AuthorEmail}> {TranslatedStrings.AuthoredAndCommitted}";
            }
            else
            {
                toolTip =
                    $"{revision.AuthorDate:g} {revision.Author} <{revision.AuthorEmail}> {TranslatedStrings.Authored}\n" +
                    $"{revision.CommitDate:g} {revision.Committer} <{revision.CommitterEmail}> {TranslatedStrings.Committed}";
            }

            return true;
        }
    }
}

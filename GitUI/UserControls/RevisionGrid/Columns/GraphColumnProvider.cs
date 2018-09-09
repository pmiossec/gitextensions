using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class GraphColumnProvider : ColumnProvider
    {
        private const int MaxLanes = 40;

        private static readonly int _nodeDimension = DpiUtil.Scale(10);
        private static readonly int _laneWidth = DpiUtil.Scale(16);
        private static readonly int _laneLineWidth = DpiUtil.Scale(2);

        private readonly JunctionStyler _junctionStyler = new JunctionStyler(new JunctionColorProvider());

        private readonly RevisionGridControl _grid;
        private readonly GraphModel _graphModel;

        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyleCache;
        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyle;
        private int _cacheCount; // Number of elements in the cache.
        private int _cacheCountMax; // Number of elements allowed in the cache. Is based on control height.
        private int _cacheHead = -1; // The 'slot' that is the head of the circular bitmap
        private int _cacheHeadRow; // The node row that is in the head slot
        [CanBeNull] private Bitmap _graphBitmap;
        [CanBeNull] private Graphics _graphBitmapGraphics;

        public GraphColumnProvider(RevisionGridControl grid, GraphModel graphModel)
            : base("Graph")
        {
            _grid = grid;
            _graphModel = graphModel;

            // TODO is it worth creating a lighter-weight column type?

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False
            };
        }

        public RevisionGraphDrawStyleEnum RevisionGraphDrawStyle
        {
            get
            {
                if (_revisionGraphDrawStyle == RevisionGraphDrawStyleEnum.HighlightSelected)
                {
                    return RevisionGraphDrawStyleEnum.HighlightSelected;
                }

                if (AppSettings.RevisionGraphDrawNonRelativesGray)
                {
                    return RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
                }

                return RevisionGraphDrawStyleEnum.Normal;
            }
            set { _revisionGraphDrawStyle = value; }
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in (Brush backBrush, Color foreColor, Font normalFont, Font boldFont) style)
        {
            if (AppSettings.ShowRevisionGridGraphColumn &&
                e.State.HasFlag(DataGridViewElementStates.Visible) &&
                e.RowIndex >= 0 &&
                _graphModel.Count != 0 &&
                _graphModel.Count > e.RowIndex &&
                PaintGraphCell(e.RowIndex, e.CellBounds, e.Graphics))
            {
                e.Handled = true;
            }

            return;

            bool PaintGraphCell(int rowIndex, Rectangle cellBounds, Graphics graphics)
            {
                // Draws the required row into _graphBitmap, or retrieves an equivalent one from the cache.

                int height = _cacheCountMax * rowHeight;
                int width = Column.Width;

                if (width <= 0 || height <= 0)
                {
                    return false;
                }

                EnsureCacheIsLargeEnough();

                // Compute how much the head needs to move to show the requested item.
                int neededHeadAdjustment = rowIndex - _cacheHead;
                if (neededHeadAdjustment > 0)
                {
                    neededHeadAdjustment -= _cacheCountMax - 1;
                    if (neededHeadAdjustment < 0)
                    {
                        neededHeadAdjustment = 0;
                    }
                }

                var newRows = _cacheCount < _cacheCountMax
                    ? (rowIndex - _cacheCount) + 1
                    : 0;

                // Adjust the head of the cache
                _cacheHead = _cacheHead + neededHeadAdjustment;
                _cacheHeadRow = (_cacheHeadRow + neededHeadAdjustment) % _cacheCountMax;
                if (_cacheHeadRow < 0)
                {
                    _cacheHeadRow = _cacheCountMax + _cacheHeadRow;
                }

                int start;
                int end;
                if (newRows > 0)
                {
                    start = _cacheHead + _cacheCount;
                    _cacheCount = Math.Min(_cacheCount + newRows, _cacheCountMax);
                    end = _cacheHead + _cacheCount;
                }
                else if (neededHeadAdjustment > 0)
                {
                    end = _cacheHead + _cacheCount;
                    start = Math.Max(_cacheHead, end - neededHeadAdjustment);
                }
                else if (neededHeadAdjustment < 0)
                {
                    start = _cacheHead;
                    end = start + Math.Min(_cacheCountMax, -neededHeadAdjustment);
                }
                else
                {
                    // Item already in the cache
                    CreateRectangle();
                    return true;
                }

                if (!DrawVisibleGraph())
                {
                    return false;
                }

                CreateRectangle();
                return true;

                void CreateRectangle()
                {
                    var cellRect = new Rectangle(
                        0,
                        ((_cacheHeadRow + rowIndex - _cacheHead) % _cacheCountMax) * rowHeight,
                        width,
                        rowHeight);

                    graphics.DrawImage(
                        _graphBitmap,
                        cellBounds,
                        cellRect,
                        GraphicsUnit.Pixel);
                }

                bool DrawVisibleGraph()
                {
                    for (var index = start; index < end; index++)
                    {
                        var row = _graphModel.GetLaneRow(index);

                        if (row == null)
                        {
                            // This shouldn't be happening...If it does, clear the cache so we
                            // eventually pick it up.
                            Debug.WriteLine("Draw lane {0} NO DATA", index);
                            ClearDrawCache();
                            return false;
                        }

                        // Get the x,y value of the current item's upper left in the cache
                        var curCacheRow = (_cacheHeadRow + index - _cacheHead) % _cacheCountMax;
                        var x = ColumnLeftMargin;
                        var y = curCacheRow * rowHeight;

                        var laneRect = new Rectangle(0, y, width, rowHeight);
                        var oldClip = _graphBitmapGraphics.Clip;

                        if (index == start || curCacheRow == 0)
                        {
                            // Draw previous row first. Clip top to row. We also need to clear the area
                            // before we draw since nothing else would clear the top 1/2 of the item to draw.
                            _graphBitmapGraphics.RenderingOrigin = new Point(x, y - rowHeight);
                            _graphBitmapGraphics.Clip = new Region(laneRect);
                            _graphBitmapGraphics.Clear(Color.Transparent);
                            DrawItem(_graphBitmapGraphics, _graphModel.GetLaneRow(index - 1));
                            _graphBitmapGraphics.Clip = oldClip;
                        }

                        if (index == end - 1)
                        {
                            // Use a custom clip for the last row
                            _graphBitmapGraphics.Clip = new Region(laneRect);
                        }

                        _graphBitmapGraphics.RenderingOrigin = new Point(x, y);

                        var success = DrawItem(_graphBitmapGraphics, row);

                        _graphBitmapGraphics.Clip = oldClip;

                        if (!success)
                        {
                            ClearDrawCache();
                            return false;
                        }
                    }

                    return true;
                }

                void EnsureCacheIsLargeEnough()
                {
                    if (_graphBitmap == null ||

                        // Resize the bitmap when the with or height is changed. The height won't change very often.
                        // The with changes more often, when branches become visible/invisible.
                        // Try to be 'smart' and not resize the bitmap for each little change. Enlarge when needed
                        // but never shrink the bitmap since the huge performance hit is worse than the little extra memory.
                        _graphBitmap.Width < width || _graphBitmap.Height != height)
                    {
                        if (_graphBitmap != null)
                        {
                            _graphBitmap.Dispose();
                            _graphBitmap = null;
                        }

                        if (_graphBitmapGraphics != null)
                        {
                            _graphBitmapGraphics.Dispose();
                            _graphBitmapGraphics = null;
                        }

                        _graphBitmap = new Bitmap(
                            Math.Max(width, _laneWidth * 3),
                            height,
                            PixelFormat.Format32bppPArgb);
                        _graphBitmapGraphics = Graphics.FromImage(_graphBitmap);
                        _graphBitmapGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                        _cacheHead = 0;
                        _cacheCount = 0;
                    }
                }

                bool DrawItem(Graphics g, ILaneRow row)
                {
                    if (row == null || row.NodeLane == -1)
                    {
                        return false;
                    }

                    // Clip to the area we're drawing in, but draw 1 pixel past so
                    // that the top/bottom of the line segment's anti-aliasing isn't
                    // visible in the final rendering.
                    int top = g.RenderingOrigin.Y + (rowHeight / 2);
                    var laneRect = new Rectangle(0, top, width, rowHeight);
                    Region oldClip = g.Clip;
                    var newClip = new Region(laneRect);
                    newClip.Intersect(oldClip);
                    g.Clip = newClip;
                    g.Clear(Color.Transparent);

                    // Getting RevisionGraphDrawStyle results in call to AppSettings. This is not very cheap, cache.
                    _revisionGraphDrawStyleCache = RevisionGraphDrawStyle;

                    var oldSmoothingMode = g.SmoothingMode;

                    for (int lane = 0; lane < row.Count; lane++)
                    {
                        int mid = g.RenderingOrigin.X + (int)((lane + 0.5) * _laneWidth);

                        for (int item = 0; item < row.LaneInfoCount(lane); item++)
                        {
                            LaneInfo laneInfo = row[lane, item];

                            _junctionStyler.UpdateJunctionColors(laneInfo.Junctions, _revisionGraphDrawStyleCache);

                            // Create the brush for drawing the line
                            Brush laneBrush = null;
                            try
                            {
                                laneBrush = _junctionStyler.GetLaneBrush();

                                // Precalculate line endpoints
                                bool sameLane = laneInfo.ConnectLane == lane;
                                int x0 = mid;
                                int y0 = top - 1;
                                int x1 = sameLane ? x0 : mid + ((laneInfo.ConnectLane - lane) * _laneWidth);
                                int y1 = top + rowHeight;

                                var p0 = new Point(x0, y0);
                                var p1 = new Point(x1, y1);

                                using (var lanePen = new Pen(laneBrush, _laneLineWidth))
                                {
                                    if (sameLane)
                                    {
                                        g.SmoothingMode = SmoothingMode.None;
                                        g.DrawLine(lanePen, p0, p1);
                                    }
                                    else
                                    {
                                        // Anti-aliasing seems to introduce an offset of two thirds
                                        // of a pixel to the right - compensate it.
                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        float offset = -0.667F;

                                        // Left shifting int is fast equivalent of dividing by two,
                                        // thus computing the average of y0 and y1.
                                        var yMid = (y0 + y1) >> 1;
                                        var c0 = new PointF(offset + x0, yMid);
                                        var c1 = new PointF(offset + x1, yMid);
                                        var e0 = new PointF(offset + p0.X, p0.Y);
                                        var e1 = new PointF(offset + p1.X, p1.Y);
                                        g.DrawBezier(lanePen, e0, c0, c1, e1);
                                    }
                                }
                            }
                            finally
                            {
                                laneBrush?.Dispose();
                            }
                        }
                    }

                    // Reset graphics options
                    g.Clip = oldClip;

                    // Draw node
                    var nodeRect = new Rectangle(
                        g.RenderingOrigin.X + ((_laneWidth - _nodeDimension) / 2) + (row.NodeLane * _laneWidth),
                        g.RenderingOrigin.Y + ((rowHeight - _nodeDimension) / 2),
                        _nodeDimension,
                        _nodeDimension);

                    Color? nodeColor = null;

                    _junctionStyler.UpdateJunctionColors(row.Node.Ancestors, _revisionGraphDrawStyleCache);
                    bool highlight = (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.DrawNonRelativesGray && row.Node.Ancestors.Any(j => j.IsRelative)) ||
                                     (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.HighlightSelected && row.Node.Ancestors.Any(j => j.IsHighlighted)) ||
                                     (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.Normal);
                    var nodeBrush = _junctionStyler.GetNodeBrush(nodeRect, highlight);

                    var square = row.Node.HasRef;
                    var hasOutline = row.Node.IsCheckedOut;

                    if (square)
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.FillRectangle(nodeBrush, nodeRect);
                    }
                    else //// Circle
                    {
                        nodeRect.Width = nodeRect.Height = _nodeDimension - 1;

                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.FillEllipse(nodeBrush, nodeRect);
                    }

                    if (hasOutline)
                    {
                        nodeRect.Inflate(1, 1);

                        var outlineColor = nodeColor == null
                            ? Color.Black
                            : ColorHelper.MakeColorDarker(nodeColor.Value, 0.3);

                        using (var pen = new Pen(outlineColor, 2))
                        {
                            if (square)
                            {
                                g.SmoothingMode = SmoothingMode.None;
                                g.DrawRectangle(pen, nodeRect);
                            }
                            else //// Circle
                            {
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.DrawEllipse(pen, nodeRect);
                            }
                        }
                    }

                    if (row.Node.Revision == null)
                    {
                        nodeRect.Inflate(1, 1);

                        using (var pen = new Pen(Color.Red, 2))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(Brushes.White, nodeRect);
                            g.DrawEllipse(pen, nodeRect);
                        }
                    }

                    nodeBrush.Dispose();

                    g.SmoothingMode = oldSmoothingMode;

                    return true;
                }
            }
        }

        public override void Clear()
        {
            lock (_graphModel)
            {
                _graphModel.Clear();
                ////_graphDataCount = 0;
                _cacheHead = -1;
                _cacheHeadRow = 0;
                ClearDrawCache();
            }
        }

        public override void Refresh(int rowHeight, in VisibleRowRange range)
        {
            // Hide graph column when there it is disabled OR when a filter is active
            // allowing for special case when history of a single file is being displayed
            Column.Visible
                = AppSettings.ShowRevisionGridGraphColumn &&
                  !_grid.ShouldHideGraph(inclBranchFilter: false);

            ClearDrawCache();
            UpdateGraphColumnWidth(range);
        }

        public override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
            ClearDrawCache();
        }

        public void HighlightBranch(ObjectId id)
        {
            _graphModel.HighlightBranch(id);
        }

        public override void OnVisibleRowsChanged(in VisibleRowRange range)
        {
            // Keep an extra page in the cache
            _cacheCountMax = (range.Count * 2) + 1;
            UpdateGraphColumnWidth(range);
        }

        // TODO when rendering, if we notice a row has too many lanes, trigger updating the column's width

        private void UpdateGraphColumnWidth(in VisibleRowRange range)
        {
            if (!Column.Visible)
            {
                return;
            }

            int laneCount = 1;
            lock (_graphModel)
            {
                foreach (var index in range)
                {
                    var laneRow = _graphModel.GetLaneRow(index);
                    if (laneRow != null)
                    {
                        laneCount = Math.Max(laneRow.Count, laneCount);
                    }
                }
            }

            // When 'git log --first-parent' filtration is enabled and when only current
            // branch needed to be rendered (and this filter actually works),
            // it is much more readable to limit max lanes to 1.
            int maxLanes =
                (AppSettings.ShowFirstParent &&
                 AppSettings.ShowCurrentBranchOnly &&
                 AppSettings.BranchFilterEnabled)
                    ? 1
                    : MaxLanes;

            laneCount = Math.Min(Math.Max(1, laneCount), maxLanes);
            var columnWidth = (_laneWidth * laneCount) + ColumnLeftMargin;
            if (Column.Width != columnWidth && columnWidth > Column.MinimumWidth)
            {
                Column.Width = columnWidth;
            }
        }

        private void ClearDrawCache()
        {
            _cacheHead = 0;
            _cacheCount = 0;
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            toolTip = GetLaneInfo(e.X - ColumnLeftMargin, e.RowIndex);
            return true;

            string GetLaneInfo(int x, int rowIndex)
            {
                int lane = x / _laneWidth;
                var laneInfoText = new StringBuilder();
                lock (_graphModel)
                {
                    ILaneRow laneRow = _graphModel.GetLaneRow(rowIndex);
                    if (laneRow != null)
                    {
                        Node node = null;
                        if (lane == laneRow.NodeLane)
                        {
                            node = laneRow.Node;
                            if (!node.Revision.IsArtificial)
                            {
                                laneInfoText.Append("* ");
                            }
                        }
                        else if (lane >= 0 && lane < laneRow.Count)
                        {
                            for (int laneInfoIndex = 0, laneInfoCount = laneRow.LaneInfoCount(lane); laneInfoIndex < laneInfoCount; ++laneInfoIndex)
                            {
                                // search for next node below this row
                                LaneInfo laneInfo = laneRow[lane, laneInfoIndex];
                                Junction firstJunction = laneInfo.Junctions.First();
                                for (int nodeIndex = 0, nodeCount = firstJunction.NodeCount; nodeIndex < nodeCount; ++nodeIndex)
                                {
                                    Node laneNode = firstJunction[nodeIndex];
                                    if (laneNode.Index > rowIndex)
                                    {
                                        node = laneNode;
                                        break; // from for (nodes)
                                    }
                                }
                            }
                        }

                        if (node != null)
                        {
                            if (!node.Revision.IsArtificial)
                            {
                                laneInfoText.AppendLine(CommitIdColumnProvider.Format(node.Revision.Guid));

                                var references = new References(node);

                                if (references.CommittedTo.IsNotNullOrWhitespace())
                                {
                                    laneInfoText.AppendFormat("\nBranch: {0}", references.CommittedTo);
                                    if (references.MergedWith.IsNotNullOrWhitespace())
                                    {
                                        laneInfoText.AppendFormat(" (merged with {0})", references.MergedWith);
                                    }
                                }

                                laneInfoText.Append(Format("\nContained in branches: {0}\n", references.Branches));
                                laneInfoText.Append(Format("\nContained in tags: {0}\n", references.Tags));

                                string Format<T>(string format, IEnumerable<T> list)
                                {
                                    return list.Any() ? string.Format(format, string.Join(", ", list)) : string.Empty;
                                }

                                // laneInfoText.Append(references.DebugInfo);

                                laneInfoText.AppendLine().AppendLine();
                            }

                            laneInfoText.Append(node.Revision.Body ?? node.Revision.Subject);
                        }
                    }
                }

                return laneInfoText.ToString();
            }
        }

        private class Reference
        {
            internal readonly string LocalName;
            internal bool IsLocal { get; private set; } = false;
            internal List<string> Remotes { get; } = new List<string>();

            internal Reference(string localName)
            {
                LocalName = localName;
            }

            internal void Add([NotNull] IGitRef gitReference)
            {
                if (gitReference.IsHead)
                {
                    IsLocal = true;
                }
                else if (gitReference.IsRemote)
                {
                    Remotes.Add(gitReference.Remote);
                }
            }

            public override string ToString()
            {
                var list = new StringBuilder();

                if (Remotes.Any())
                {
                    if (IsLocal)
                    {
                        list.Append("[");
                    }

                    list.Append(Remotes.Join("|")).Append("/");

                    if (IsLocal)
                    {
                        list.Append("]");
                    }
                }

                list.Append(LocalName);

                return list.ToString();
            }
        }

        private class References
        {
            private HashSet<Node> _visitedNodes = new HashSet<Node>();
            private Dictionary<string, Reference> _referenceByName = new Dictionary<string, Reference>();
            internal List<Reference> Tags { get; } = new List<Reference>();
            internal List<Reference> Branches { get; } = new List<Reference>();
            internal string CommittedTo { get; private set; }
            internal string MergedWith { get; private set; }
            internal StringBuilder DebugInfo = new StringBuilder();

            internal References([NotNull] Node node)
            {
                AddReferencesOf(node, previousDescJunction: null);
            }

            private void AddReferencesOf([NotNull] Node node, [CanBeNull] Junction previousDescJunction)
            {
                if (_visitedNodes.Add(node))
                {
                    CheckForMerge(node, previousDescJunction);
                    node.Revision.Refs.ForEach(reference => Add(reference, node, previousDescJunction));
                    foreach (var descJunction in node.Descendants)
                    {
                        // iterate the inner nodes (i.e. excluding the youngest) beginning with the oldest
                        bool nodeFound = false;
                        for (int nodeIndex = descJunction.NodeCount - 1; nodeIndex > 0; --nodeIndex)
                        {
                            Node innerNode = descJunction[nodeIndex];
                            if (nodeFound)
                            {
                                innerNode.Revision.Refs.ForEach(reference => Add(reference, node, descJunction));
                            }
                            else
                            {
                                nodeFound = innerNode == node;
                            }
                        }

                        // handle the youngest and its descendants
                        AddReferencesOf(descJunction.Youngest, descJunction);
                    }
                }
            }

            private void Add([NotNull] IGitRef gitReference, [NotNull] Node node, [CanBeNull] Junction descJunction)
            {
                Reference reference;
                if (!_referenceByName.TryGetValue(gitReference.LocalName, out reference))
                {
                    reference = new Reference(gitReference.LocalName);
                    _referenceByName.Add(gitReference.LocalName, reference);
                    if (gitReference.IsTag)
                    {
                        Tags.Add(reference);
                    }
                    else if (gitReference.IsHead || gitReference.IsRemote)
                    {
                        Branches.Add(reference);
                        if (CommittedTo == null)
                        {
                            CheckForMerge(node, descJunction);
                            CommittedTo = CommittedTo ?? gitReference.Name;
                        }
                    }
                    else if (gitReference.IsStash && CommittedTo == null)
                    {
                        CommittedTo = gitReference.Name;
                    }
                }

                reference.Add(gitReference);
            }

            /// <summary>
            /// Checks whether the commit message is a merge message
            /// though only if CommittedTo has not been set yet
            /// and then if its a merge message, sets CommittedTo and MergedWith.
            ///
            /// MergedWith is set if it is the current node, i.e. on the first call.
            /// MergedWith is set to string.Empty if it is no merge.
            /// First/second branch does not matter because it is the message of the current node.
            /// </summary>
            /// <param name="node">the node of the revision to evaluate</param>
            /// <param name="descJunction">
            /// the descending junction the node is part of
            /// (used for the decision whether the node belongs the first or second branch of the merge)
            /// </param>
            private void CheckForMerge([NotNull] Node node, [CanBeNull] Junction descJunction)
            {
                if (CommittedTo == null)
                {
                    bool isTheFirstBranch = descJunction == null || node.Ancestors.Count == 0 || node.Ancestors.First() == descJunction;
                    string mergedInto;
                    string mergedWith;
                    (mergedInto, mergedWith) = ParseMergeMessage(node, appendPullRequest: isTheFirstBranch);

                    if (mergedInto != null)
                    {
                        CommittedTo = isTheFirstBranch ? mergedInto : mergedWith;
                    }

                    if (MergedWith == null)
                    {
                        MergedWith = mergedWith ?? string.Empty;
                    }

                    if (CommittedTo != null)
                    {
                        DebugInfo.AppendFormat("  node: {0}, branch: {1}, descJunction {2} isTheFirstBranch {3}\n",
                            node.Revision.Guid.Substring(0, 8), CommittedTo, descJunction?.ToString() ?? "(null)", isTheFirstBranch);
                        foreach (var ancJunction in node.Ancestors)
                        {
                            DebugInfo.AppendFormat("  {2}anc {0} -> {1} {3}\n",
                                ancJunction.Oldest.Revision.Guid.Substring(0, 8),
                                ancJunction.Youngest.Revision.Guid.Substring(0, 8),
                                ancJunction == descJunction ? ">\x200A" : "   ",
                                ancJunction == descJunction ? CommittedTo + " " + MergedWith : "");
                        }
                    }
                }
            }

            private static readonly Regex _merge = new Regex("(?i)^merged? (pull request (.*) from )?(.*branch )?'?([^ ']+)'?( into (.*))?\\.?$");

            private static (string into, string with) ParseMergeMessage([NotNull] Node node, bool appendPullRequest = false)
            {
                string into = null;
                string with = null;
                Match match = _merge.Match(node.Revision.Subject);
                if (match.Success)
                {
                    var matchPullRequest = match.Groups[2];
                    var matchWith = match.Groups[4];
                    var matchInto = match.Groups[6];
                    into = matchInto.Success ? matchInto.Value : "master";
                    with = matchWith.Success ? matchWith.Value : "?";
                    if (appendPullRequest && matchPullRequest.Success)
                    {
                        with += string.Format(" by pull request {0}", matchPullRequest);
                    }
                }

                return (into, with);
            }
        }
    }
}
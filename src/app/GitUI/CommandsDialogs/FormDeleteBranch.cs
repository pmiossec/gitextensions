using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteBranch : GitExtensionsDialog
    {
        private readonly TranslationString _deleteBranchCaption = new("Delete Branches");
        private readonly TranslationString _cannotDeleteCurrentBranchMessage = new("Cannot delete the branch “{0}” which you are currently on.");
        private readonly TranslationString _deleteBranchConfirmTitle = new("Delete Confirmation");
        private readonly TranslationString _deleteBranchQuestion = new("The selected branch(es) have not been merged into HEAD.\r\nProceed?");
        private readonly TranslationString _useReflogHint = new("The commits are available from the reflog that you could use to restore the deleted branch(es)");
        private readonly TranslationString _deleteBranchNotInReflogQuestion = new("At least one of the selected branch(es) have not been merged into HEAD\r\n and is not in the reflog.\r\n\r\nSo you won't be able to recover the commit(s) easily.\r\n\r\nProceed?");
        private readonly TranslationString _useRecoverLostObjectsHint = new("This commit will only be recoverable using \"Recover lost objects\" feature but be aware that git could remove it at any time!");
        private readonly TranslationString _restoreUsingReflogAvailable = new("This branch can be restored using the reflog");
        private readonly TranslationString _warningNotInReflog = new("Warning! The head of this branch is not in the reflog!\r\nCommits won't be recoverable easily!!");

        private readonly IEnumerable<string> _defaultBranches;
        private string? _currentBranch;
        private HashSet<string>? _mergedBranches;
        private IReadOnlyList<string> _reflogHashes;

        public FormDeleteBranch(IGitUICommands commands, IEnumerable<string> defaultBranches)
            : base(commands, enablePositionRestore: false)
        {
            _defaultBranches = defaultBranches;

            InitializeComponent();

            MinimumSize = new Size(Width, PreferredMinimumHeight);

            InitializeComplete();
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            _reflogHashes = Module.GetReflogHashes();

            Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Heads).ToList();
            if (AppSettings.DontConfirmDeleteUnmergedBranch)
            {
                // no need to fill _mergedBranches
                _currentBranch = Module.GetSelectedBranch();
            }
            else
            {
                _mergedBranches = [];
                foreach (string branch in Module.GetMergedBranches())
                {
                    if (branch.StartsWith("* "))
                    {
                        _currentBranch = branch.Trim('*', ' ');
                    }
                    else
                    {
                        _mergedBranches.Add(branch.Trim());
                    }
                }
            }

            if (_defaultBranches is not null)
            {
                Branches.SetSelectedText(_defaultBranches.Join(" "));
            }

            Branches.Focus();

            CheckSelectedBranches();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            IGitRef[] selectedBranches = Branches.GetSelectedBranches().ToArray();
            if (!selectedBranches.Any())
            {
                return;
            }

            if (_currentBranch is not null && selectedBranches.Any(branch => branch.Name == _currentBranch))
            {
                MessageBox.Show(this, string.Format(_cannotDeleteCurrentBranchMessage.Text, _currentBranch), _deleteBranchCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool areAllInReflog = selectedBranches.All(b => _reflogHashes.Contains(b.ObjectId.ToString()));

            if (!areAllInReflog || !AppSettings.DontConfirmDeleteUnmergedBranch)
            {
                Validates.NotNull(_mergedBranches);

                // always treat branches as unmerged if there is no current branch (HEAD is detached)
                bool hasUnmergedBranches = _currentBranch is null || DetachedHeadParser.IsDetachedHead(_currentBranch)
                    || selectedBranches.Any(branch => !_mergedBranches.Contains(branch.Name));
                if (hasUnmergedBranches)
                {
                    TaskDialogPage page = new()
                    {
                        Text = areAllInReflog ? _deleteBranchQuestion.Text : _deleteBranchNotInReflogQuestion.Text,
                        Caption = _deleteBranchConfirmTitle.Text,
                        Icon = TaskDialogIcon.Warning,
                        Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                        DefaultButton = TaskDialogButton.No,
                        Footnote = areAllInReflog ? _useReflogHint.Text : _useRecoverLostObjectsHint.Text,
                        SizeToContent = true,
                    };

                    bool isConfirmed = TaskDialog.ShowDialog(Handle, page) == TaskDialogButton.Yes;
                    if (!isConfirmed)
                    {
                        return;
                    }
                }
            }

            IGitCommand cmd = Commands.DeleteBranch(selectedBranches, force: true);
            bool success = UICommands.StartCommandLineProcessDialog(Owner, cmd);
            if (success)
            {
                Close();
            }
        }

        private void Branches_SelectedValueChanged(object sender, EventArgs e)
        {
            CheckSelectedBranches();
        }

        private void CheckSelectedBranches()
        {
            IGitRef[] selectedBranches = Branches.GetSelectedBranches().ToArray();
            if (!selectedBranches.Any())
            {
                return;
            }

            foreach (IGitRef selectedBranch in selectedBranches)
            {
                if (!_reflogHashes.Contains(selectedBranch.ObjectId.ToString()))
                {
                    labelWarning.Text = _warningNotInReflog.Text;
                    labelWarning.ForeColor = Color.Orange;
                    return;
                }
            }

            labelWarning.Text = _restoreUsingReflogAvailable.Text;
            labelWarning.ForeColor = Color.Green;
        }
    }
}

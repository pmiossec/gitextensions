using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git.Commands;
using GitCommands.Patches;
using GitExtUtils.GitUI.Theming;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormRebaseConflicts : GitModuleForm
    {
        #region Mnemonics
        // Available: GHJLNVWXYZ
        // A Add files
        // B Abort
        // C Continue rebase
        // D Ignore date
        // E Specific range
        // F From
        // I Interactive
        // K Skip
        // M Committer date
        // O Commit...
        // P Preserve Merges
        // Q Autosquash
        // R Rebase on
        // S Solve conflicts
        // T To
        // U Auto stash
        #endregion

        #region Translation
        private readonly TranslationString _continueRebaseText = new("&Continue rebase");
        private readonly TranslationString _solveConflictsText = new("&Solve conflicts");

        private readonly TranslationString _solveConflictsText2 = new(">&Solve conflicts<");
        private readonly TranslationString _continueRebaseText2 = new(">&Continue rebase<");

        private readonly TranslationString _noBranchSelectedText = new("Please select a branch");

        private readonly TranslationString _branchUpToDateText =
            new("Current branch a is up to date." + Environment.NewLine + "Nothing to rebase.");
        private readonly TranslationString _branchUpToDateCaption = new("Rebase");

        private readonly TranslationString _hoverShowImageLabelText = new("Hover to see scenario when fast forward is possible.");
        #endregion

        private static readonly List<PatchFile> Skipped = new();

        private readonly string? _defaultBranch;
        private readonly string? _defaultToBranch;
        private readonly bool _startRebaseImmediately;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormRebaseConflicts()
        {
            InitializeComponent();
        }

        public FormRebaseConflicts(GitUICommands commands, string? defaultBranch)
            : base(commands)
        {
            _defaultBranch = defaultBranch;
            InitializeComponent();
            SolveMergeconflicts.BackColor = OtherColors.MergeConflictsColor;
            SolveMergeconflicts.SetForeColorForBackColor();
            PanelLeftImage.Image1 = Properties.Images.HelpCommandRebase.AdaptLightness();
            InitializeComplete();
            PanelLeftImage.Visible = !AppSettings.DontShowHelpImages;
            PanelLeftImage.IsOnHoverShowImage2NoticeText = _hoverShowImageLabelText.Text;
            PatchGrid.SetSkipped(Skipped);
            if (AppSettings.AlwaysShowAdvOpt)
            {
                ShowOptions_LinkClicked(this, null!);
            }

            Shown += FormRebase_Shown;
        }

        private void FormRebase_Shown(object sender, EventArgs e)
        {
            PatchGrid.SelectCurrentlyApplyingPatch();
        }

        public FormRebaseConflicts(GitUICommands commands, string? from, string? to, string? defaultBranch, bool interactive = false, bool startRebaseImmediately = true)
            : this(commands, defaultBranch)
        {
            _defaultToBranch = to;
            _startRebaseImmediately = startRebaseImmediately;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var selectedHead = Module.GetSelectedBranch();
            Currentbranch.Text = selectedHead;

            EnableButtons();

            if (_startRebaseImmediately)
            {
                OkClick(this, EventArgs.Empty);
            }
            else
            {
                ShowOptions_LinkClicked(this, null!);
            }
        }

        private void EnableButtons()
        {
            if (Module.InTheMiddleOfRebase())
            {
                if (Height < 200)
                {
                    Height = 500;
                }

                AddFiles.Enabled = true;
                Commit.Enabled = true;
                Resolved.Enabled = !Module.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = Module.InTheMiddleOfConflictedMerge();
                Skip.Enabled = true;
                Abort.Enabled = true;
            }
            else
            {
                AddFiles.Enabled = false;
                Commit.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }

            SolveMergeconflicts.Visible = Module.InTheMiddleOfConflictedMerge();

            Resolved.Text = _continueRebaseText.Text;
            Mergetool.Text = _solveConflictsText.Text;
            Resolved.ForeColor = SystemColors.ControlText;
            Mergetool.ForeColor = SystemColors.ControlText;
            ContinuePanel.BackColor = Color.Transparent;
            MergeToolPanel.BackColor = Color.Transparent;

            var highlightColor = Color.Yellow.AdaptBackColor();

            if (Module.InTheMiddleOfConflictedMerge())
            {
                AcceptButton = Mergetool;
                Mergetool.Focus();
                Mergetool.Text = _solveConflictsText2.Text;
                MergeToolPanel.BackColor = highlightColor;
            }
            else if (Module.InTheMiddleOfRebase())
            {
                AcceptButton = Resolved;
                Resolved.Focus();
                Resolved.Text = _continueRebaseText2.Text;
                ContinuePanel.BackColor = highlightColor;
            }
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            UICommands.StartResolveConflictsDialog(this);
            EnableButtons();
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            UICommands.StartAddFilesDialog(this);
        }

        private void ResolvedClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, arguments: GitCommandHelpers.ContinueRebaseCmd(), Module.WorkingDir, input: null, useDialogSettings: true);

                if (!Module.InTheMiddleOfRebase())
                {
                    Close();
                }

                EnableButtons();
                PatchGrid.Initialize();
            }
        }

        private void SkipClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                var applyingPatch = PatchGrid.PatchFiles.FirstOrDefault(p => p.IsNext);
                if (applyingPatch is not null)
                {
                    applyingPatch.IsSkipped = true;
                    Skipped.Add(applyingPatch);
                }

                FormProcess.ShowDialog(this, arguments: GitCommandHelpers.SkipRebaseCmd(), Module.WorkingDir, input: null, useDialogSettings: true);

                if (!Module.InTheMiddleOfRebase())
                {
                    Close();
                }

                EnableButtons();

                PatchGrid.RefreshGrid();
            }
        }

        private void AbortClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, arguments: GitCommandHelpers.AbortRebaseCmd(), Module.WorkingDir, input: null, useDialogSettings: true);

                if (!Module.InTheMiddleOfRebase())
                {
                    Skipped.Clear();
                    Close();
                }

                EnableButtons();
                PatchGrid.Initialize();
            }
        }

        private void SolveMergeConflictsClick(object sender, EventArgs e)
        {
            MergetoolClick(sender, e);
        }

        private void Commit_Click(object sender, EventArgs e)
        {
            UICommands.StartCommitDialog(this);
            EnableButtons();
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FormRebase _form;

            public TestAccessor(FormRebase form)
            {
                _form = form;
            }
        }
    }
}

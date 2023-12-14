using System.ComponentModel;
using GitCommands;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;

namespace GitUI
{
    public partial class BranchComboBox : GitExtensionsControl
    {
        private readonly TranslationString _branchCheckoutError = new("Branch '{0}' is not selectable, this branch has been removed from the selection.");
        private Func<IReadOnlyList<IGitRef>> _branchLoader;

        public BranchComboBox()
        {
            InitializeComponent();
            InitializeComplete();

            branches.DisplayMember = nameof(IGitRef.Name);
        }

        internal void SetLoader(Func<IReadOnlyList<IGitRef>> branchLoader)
        {
            _branchLoader = branchLoader;
        }

        [Browsable(true)]
        [Category("Action")]
        [Description("Invoked when branch selection changed")]
        public event EventHandler SelectedValueChanged;

        private IReadOnlyList<IGitRef>? _branchesToSelect;
        public IReadOnlyList<IGitRef>? BranchesToSelect
        {
            get
            {
                return _branchesToSelect;
            }
            set
            {
                _branchesToSelect = value;
                LoadBranches();
            }
        }

        private void LoadBranches()
        {
            if (_branchesToSelect is not null)
            {
                branches.Items.AddRange(_branchesToSelect.ToArray());
            }
        }

        public IEnumerable<IGitRef> GetSelectedBranches()
        {
            foreach (string branch in branches.Text.LazySplit(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                IGitRef gitHead = _branchesToSelect.FirstOrDefault(g => g.Name == branch);
                if (gitHead is null)
                {
                    MessageBox.Show(string.Format(_branchCheckoutError.Text, branch), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    yield return gitHead;
                }
            }
        }

        public string GetSelectedText()
        {
            return branches.Text;
        }

        public void SetSelectedText(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            branches.Text = text;
        }

        private void selectMultipleBranchesButton_Click(object sender, EventArgs e)
        {
            PopulateBranches();
            Validates.NotNull(_branchesToSelect);

            using FormSelectMultipleBranches formSelectMultipleBranches = new(_branchesToSelect);
            foreach (IGitRef branch in GetSelectedBranches())
            {
                formSelectMultipleBranches.SelectBranch(branch.Name);
            }

            formSelectMultipleBranches.ShowDialog(this);
            string branchesText = string.Empty;
            foreach (GitRef branch in formSelectMultipleBranches.GetSelectedBranches())
            {
                if (!string.IsNullOrEmpty(branchesText))
                {
                    branchesText += " ";
                }

                branchesText += branch.Name;
            }

            branches.Text = branchesText;

            SelectedValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void branches_SelectedValueChanged(object sender, EventArgs e)
        {
            SelectedValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void branches_Enter(object sender, EventArgs e)
        {
            PopulateBranches();
        }

        private bool _loaded = false;
        private void PopulateBranches()
        {
            // TODO: perf load only when opening dropdown or clicking multiselect button
            if (!_loaded)
            {
                _loaded = true;
                BranchesToSelect = _branchLoader();
            }
        }
    }
}

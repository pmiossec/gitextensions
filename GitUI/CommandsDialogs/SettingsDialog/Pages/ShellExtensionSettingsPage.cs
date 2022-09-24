using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ShellExtensionSettingsPage : SettingsPageWithHeader
    {
        private const char Checked_InMenu = '0';
        private const char Indeterminate_InSubMenu = '1';
        private const char Unchecked_NotInMenu = '2';

        private readonly TranslationString _noItems = new("no items");
        private readonly TranslationString _menuHelp = new(@"* Checked: at top level for direct access
* Intermediate: in a cascaded context menu
* Unchecked: not added to the menu");

        private bool _isLoading = false;
        public ShellExtensionSettingsPage()
        {
            InitializeComponent();
            Text = "Shell extension";
            InitializeComplete();

            // when the dock is set in the designer it causes weird visual artifacts in scaled Windows environments
            chlMenuEntries.Dock = DockStyle.Fill;

            toolTip1.SetToolTip(menuHelp, _menuHelp.Text);
        }

        protected override void SettingsToPage()
        {
            _isLoading = true;
            for (int i = 0; i < AppSettings.CascadeShellMenuItems.Length; i++)
            {
                switch (AppSettings.CascadeShellMenuItems[i])
                {
                    case Checked_InMenu:
                        chlMenuEntries.SetItemCheckState(i, CheckState.Checked);
                        break;
                    case Indeterminate_InSubMenu:
                        chlMenuEntries.SetItemCheckState(i, CheckState.Indeterminate);
                        break;
                    case Unchecked_NotInMenu:
                        chlMenuEntries.SetItemCheckState(i, CheckState.Unchecked);
                        break;
                }
            }

            _isLoading = false;

            cbAlwaysShowAllCommands.Checked = AppSettings.AlwaysShowAllCommands;

            UpdatePreview();
        }

        protected override void PageToSettings()
        {
            string l_CascadeShellMenuItems = "";

            for (int i = 0; i < chlMenuEntries.Items.Count; i++)
            {
                switch (chlMenuEntries.GetItemCheckState(i))
                {
                    case CheckState.Indeterminate:
                        l_CascadeShellMenuItems += Indeterminate_InSubMenu;
                        break;
                    case CheckState.Checked:
                        l_CascadeShellMenuItems += Checked_InMenu;
                        break;
                    case CheckState.Unchecked:
                        l_CascadeShellMenuItems += Unchecked_NotInMenu;
                        break;
                }
            }

            AppSettings.CascadeShellMenuItems = l_CascadeShellMenuItems;
            AppSettings.AlwaysShowAllCommands = cbAlwaysShowAllCommands.Checked;
        }

        private void chlMenuEntries_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            string topLevel = "";
            string cascaded = "";

            for (int i = 0; i < chlMenuEntries.Items.Count; i++)
            {
                switch (chlMenuEntries.GetItemCheckState(i))
                {
                    case CheckState.Checked:
                        topLevel += "GitExt " + chlMenuEntries.Items[i] + "\r\n";
                        break;
                    case CheckState.Indeterminate:
                        cascaded += "       " + chlMenuEntries.Items[i] + "\r\n";
                        break;
                }
            }

            labelPreview.Text = topLevel;
            if (!string.IsNullOrWhiteSpace(cascaded))
            {
                labelPreview.Text += "Git Extensions > \r\n" + cascaded;
            }
            else if (string.IsNullOrWhiteSpace(topLevel))
            {
                labelPreview.Text += $"({_noItems.Text})";
            }
        }

        private void chlMenuEntries_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            switch (e.CurrentValue)
            {
                case CheckState.Checked:
                    e.NewValue = CheckState.Unchecked;
                    break;

                case CheckState.Indeterminate:
                    e.NewValue = CheckState.Checked;
                    break;

                case CheckState.Unchecked:
                    e.NewValue = CheckState.Indeterminate;
                    break;
            }
        }
    }
}

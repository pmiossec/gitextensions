namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class ShellExtensionSettingsPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components is not null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Panel panel1;
            this.labelPreview = new System.Windows.Forms.Label();
            this.lblMenuEntries = new System.Windows.Forms.Label();
            this.chlMenuEntries = new System.Windows.Forms.CheckedListBox();
            this.cbAlwaysShowAllCommands = new System.Windows.Forms.CheckBox();
            this.tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.gbShellExtensions = new System.Windows.Forms.GroupBox();
            this.tlpnlCascadingMenu = new System.Windows.Forms.TableLayoutPanel();
            this.menuHelp = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            panel1 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            this.tlpnlMain.SuspendLayout();
            this.gbShellExtensions.SuspendLayout();
            this.tlpnlCascadingMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.menuHelp)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(this.labelPreview);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(323, 71);
            panel1.Margin = new System.Windows.Forms.Padding(2);
            panel1.Name = "panel1";
            panel1.Padding = new System.Windows.Forms.Padding(3);
            panel1.Size = new System.Drawing.Size(624, 271);
            panel1.TabIndex = 0;
            // 
            // labelPreview
            // 
            this.labelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPreview.Location = new System.Drawing.Point(3, 3);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(618, 265);
            this.labelPreview.TabIndex = 1;
            this.labelPreview.Text = "...";
            // 
            // lblMenuEntries
            // 
            this.lblMenuEntries.AutoSize = true;
            this.lblMenuEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMenuEntries.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblMenuEntries.Location = new System.Drawing.Point(3, 45);
            this.lblMenuEntries.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.lblMenuEntries.Name = "lblMenuEntries";
            this.lblMenuEntries.Size = new System.Drawing.Size(237, 24);
            this.lblMenuEntries.TabIndex = 2;
            this.lblMenuEntries.Text = "Configuration of items in the context menu";
            this.lblMenuEntries.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chlMenuEntries
            // 
            this.chlMenuEntries.CheckOnClick = true;
            this.tlpnlCascadingMenu.SetColumnSpan(this.chlMenuEntries, 2);
            this.chlMenuEntries.FormattingEnabled = true;
            this.chlMenuEntries.Items.AddRange(new object[] {
            "Add files...",
            "Apply patch...",
            "Open repository",
            "Create branch...",
            "Checkout branch...",
            "Checkout revision...",
            "Clone...",
            "Commit...",
            "Create new repository...",
            "Open with difftool",
            "File history",
            "Pull/Fetch...",
            "Push...",
            "Reset file changes..",
            "Revert",
            "Settings",
            "View stash",
            "View changes"});
            this.chlMenuEntries.Location = new System.Drawing.Point(3, 72);
            this.chlMenuEntries.Name = "chlMenuEntries";
            this.chlMenuEntries.Size = new System.Drawing.Size(315, 94);
            this.chlMenuEntries.TabIndex = 3;
            this.chlMenuEntries.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chlMenuEntries_ItemCheck);
            this.chlMenuEntries.SelectedValueChanged += new System.EventHandler(this.chlMenuEntries_SelectedValueChanged);
            // 
            // cbAlwaysShowAllCommands
            // 
            this.cbAlwaysShowAllCommands.AutoSize = true;
            this.tlpnlCascadingMenu.SetColumnSpan(this.cbAlwaysShowAllCommands, 3);
            this.cbAlwaysShowAllCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbAlwaysShowAllCommands.Location = new System.Drawing.Point(3, 3);
            this.cbAlwaysShowAllCommands.Name = "cbAlwaysShowAllCommands";
            this.cbAlwaysShowAllCommands.Size = new System.Drawing.Size(943, 19);
            this.cbAlwaysShowAllCommands.TabIndex = 1;
            this.cbAlwaysShowAllCommands.Text = "Always show all commands";
            this.cbAlwaysShowAllCommands.UseVisualStyleBackColor = true;
            // 
            // tlpnlMain
            // 
            this.tlpnlMain.AutoSize = true;
            this.tlpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlMain.ColumnCount = 1;
            this.tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlMain.Controls.Add(this.gbShellExtensions, 0, 0);
            this.tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlMain.Location = new System.Drawing.Point(8, 8);
            this.tlpnlMain.Name = "tlpnlMain";
            this.tlpnlMain.RowCount = 1;
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 508F));
            this.tlpnlMain.Size = new System.Drawing.Size(971, 382);
            this.tlpnlMain.TabIndex = 0;
            // 
            // gbShellExtensions
            // 
            this.gbShellExtensions.AutoSize = true;
            this.gbShellExtensions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbShellExtensions.Controls.Add(this.tlpnlCascadingMenu);
            this.gbShellExtensions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbShellExtensions.Location = new System.Drawing.Point(3, 3);
            this.gbShellExtensions.Name = "gbShellExtensions";
            this.gbShellExtensions.Padding = new System.Windows.Forms.Padding(8);
            this.gbShellExtensions.Size = new System.Drawing.Size(965, 376);
            this.gbShellExtensions.TabIndex = 0;
            this.gbShellExtensions.TabStop = false;
            this.gbShellExtensions.Text = "Cascaded Context Menu";
            // 
            // tlpnlCascadingMenu
            // 
            this.tlpnlCascadingMenu.AutoSize = true;
            this.tlpnlCascadingMenu.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlCascadingMenu.ColumnCount = 3;
            this.tlpnlCascadingMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlCascadingMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlCascadingMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlCascadingMenu.Controls.Add(this.menuHelp, 1, 2);
            this.tlpnlCascadingMenu.Controls.Add(this.label1, 2, 2);
            this.tlpnlCascadingMenu.Controls.Add(this.cbAlwaysShowAllCommands, 0, 0);
            this.tlpnlCascadingMenu.Controls.Add(this.lblMenuEntries, 0, 2);
            this.tlpnlCascadingMenu.Controls.Add(this.chlMenuEntries, 0, 3);
            this.tlpnlCascadingMenu.Controls.Add(panel1, 2, 3);
            this.tlpnlCascadingMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlCascadingMenu.Location = new System.Drawing.Point(8, 24);
            this.tlpnlCascadingMenu.Name = "tlpnlCascadingMenu";
            this.tlpnlCascadingMenu.RowCount = 4;
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.Size = new System.Drawing.Size(949, 344);
            this.tlpnlCascadingMenu.TabIndex = 0;
            // 
            // menuHelp
            // 
            this.menuHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.menuHelp.Image = global::GitUI.Properties.Resources.information;
            this.menuHelp.Location = new System.Drawing.Point(240, 50);
            this.menuHelp.Margin = new System.Windows.Forms.Padding(0, 5, 3, 3);
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.Size = new System.Drawing.Size(16, 16);
            this.menuHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.menuHelp.TabIndex = 13;
            this.menuHelp.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(324, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(622, 24);
            this.label1.TabIndex = 4;
            this.label1.Text = "Context menu preview:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ShellExtensionSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tlpnlMain);
            this.Name = "ShellExtensionSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(987, 398);
            panel1.ResumeLayout(false);
            this.tlpnlMain.ResumeLayout(false);
            this.tlpnlMain.PerformLayout();
            this.gbShellExtensions.ResumeLayout(false);
            this.gbShellExtensions.PerformLayout();
            this.tlpnlCascadingMenu.ResumeLayout(false);
            this.tlpnlCascadingMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.menuHelp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMenuEntries;
        private System.Windows.Forms.CheckedListBox chlMenuEntries;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.CheckBox cbAlwaysShowAllCommands;
        private System.Windows.Forms.TableLayoutPanel tlpnlCascadingMenu;
        private System.Windows.Forms.TableLayoutPanel tlpnlMain;
        private System.Windows.Forms.GroupBox gbShellExtensions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox menuHelp;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

namespace GitUI.CommandsDialogs
{
    partial class FormRebaseConflicts
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblRebase = new System.Windows.Forms.Label();
            this.lblCurrent = new System.Windows.Forms.Label();
            this.AddFiles = new System.Windows.Forms.Button();
            this.Resolved = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.Skip = new System.Windows.Forms.Button();
            this.Mergetool = new System.Windows.Forms.Button();
            this.PanelCurrentBranch = new System.Windows.Forms.FlowLayoutPanel();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.PatchGrid = new GitUI.PatchGrid();
            this.lblCommitsToReapply = new System.Windows.Forms.Label();
            this.SolveMergeconflicts = new System.Windows.Forms.Button();
            this.ContinuePanel = new System.Windows.Forms.Panel();
            this.MergeToolPanel = new System.Windows.Forms.Panel();
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.PanelLeftImage = new GitUI.Help.HelpImageDisplayUserControl();
            this.PanelRight = new System.Windows.Forms.FlowLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.Commit = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.PanelMiddle = new System.Windows.Forms.TableLayoutPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.PanelCurrentBranch.SuspendLayout();
            this.ContinuePanel.SuspendLayout();
            this.MergeToolPanel.SuspendLayout();
            this.MainLayout.SuspendLayout();
            this.PanelRight.SuspendLayout();
            this.PanelMiddle.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRebase
            // 
            this.lblRebase.AutoSize = true;
            this.lblRebase.Location = new System.Drawing.Point(3, 0);
            this.lblRebase.Name = "lblRebase";
            this.lblRebase.Size = new System.Drawing.Size(261, 15);
            this.lblRebase.TabIndex = 3;
            this.lblRebase.Text = "Rebase current branch on top of another branch";
            // 
            // lblCurrent
            // 
            this.lblCurrent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCurrent.AutoSize = true;
            this.lblCurrent.Location = new System.Drawing.Point(3, 5);
            this.lblCurrent.Name = "lblCurrent";
            this.lblCurrent.Size = new System.Drawing.Size(90, 15);
            this.lblCurrent.TabIndex = 5;
            this.lblCurrent.Text = "Current branch:";
            // 
            // AddFiles
            // 
            this.AddFiles.Location = new System.Drawing.Point(3, 64);
            this.AddFiles.Name = "AddFiles";
            this.AddFiles.Size = new System.Drawing.Size(162, 25);
            this.AddFiles.TabIndex = 34;
            this.AddFiles.Text = "&Add files";
            this.AddFiles.UseVisualStyleBackColor = true;
            this.AddFiles.Click += new System.EventHandler(this.AddFilesClick);
            // 
            // Resolved
            // 
            this.Resolved.Location = new System.Drawing.Point(0, 4);
            this.Resolved.Name = "Resolved";
            this.Resolved.Size = new System.Drawing.Size(161, 25);
            this.Resolved.TabIndex = 38;
            this.Resolved.Text = "&Continue rebase";
            this.Resolved.UseVisualStyleBackColor = true;
            this.Resolved.Click += new System.EventHandler(this.ResolvedClick);
            // 
            // Abort
            // 
            this.Abort.Location = new System.Drawing.Point(3, 218);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(162, 25);
            this.Abort.TabIndex = 40;
            this.Abort.Text = "A&bort";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.AbortClick);
            // 
            // Skip
            // 
            this.Skip.Location = new System.Drawing.Point(3, 187);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(162, 25);
            this.Skip.TabIndex = 39;
            this.Skip.Text = "S&kip currently applying commit";
            this.Skip.UseVisualStyleBackColor = true;
            this.Skip.Click += new System.EventHandler(this.SkipClick);
            // 
            // Mergetool
            // 
            this.Mergetool.Location = new System.Drawing.Point(0, 4);
            this.Mergetool.Name = "Mergetool";
            this.Mergetool.Size = new System.Drawing.Size(161, 25);
            this.Mergetool.TabIndex = 32;
            this.Mergetool.Text = "&Solve conflicts";
            this.Mergetool.UseVisualStyleBackColor = true;
            this.Mergetool.Click += new System.EventHandler(this.MergetoolClick);
            // 
            // PanelCurrentBranch
            // 
            this.PanelCurrentBranch.AutoSize = true;
            this.PanelCurrentBranch.Controls.Add(this.lblCurrent);
            this.PanelCurrentBranch.Controls.Add(this.Currentbranch);
            this.PanelCurrentBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelCurrentBranch.Location = new System.Drawing.Point(0, 15);
            this.PanelCurrentBranch.Margin = new System.Windows.Forms.Padding(0);
            this.PanelCurrentBranch.Name = "PanelCurrentBranch";
            this.PanelCurrentBranch.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.PanelCurrentBranch.Size = new System.Drawing.Size(684, 25);
            this.PanelCurrentBranch.TabIndex = 4;
            this.PanelCurrentBranch.WrapContents = false;
            // 
            // Currentbranch
            // 
            this.Currentbranch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Currentbranch.AutoSize = true;
            this.Currentbranch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.Currentbranch.Location = new System.Drawing.Point(99, 5);
            this.Currentbranch.Name = "Currentbranch";
            this.Currentbranch.Size = new System.Drawing.Size(0, 15);
            this.Currentbranch.TabIndex = 3;
            // 
            // PatchGrid
            // 
            this.PatchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PatchGrid.IsManagingRebase = false;
            this.PatchGrid.Location = new System.Drawing.Point(3, 67);
            this.PatchGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.PatchGrid.Name = "PatchGrid";
            this.PatchGrid.Size = new System.Drawing.Size(678, 397);
            this.PatchGrid.TabIndex = 26;
            // 
            // lblCommitsToReapply
            // 
            this.lblCommitsToReapply.AutoSize = true;
            this.lblCommitsToReapply.Location = new System.Drawing.Point(3, 40);
            this.lblCommitsToReapply.Name = "lblCommitsToReapply";
            this.lblCommitsToReapply.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lblCommitsToReapply.Size = new System.Drawing.Size(120, 25);
            this.lblCommitsToReapply.TabIndex = 25;
            this.lblCommitsToReapply.Text = "Commits to re-apply:";
            // 
            // SolveMergeconflicts
            // 
            this.SolveMergeconflicts.BackColor = System.Drawing.Color.Salmon;
            this.SolveMergeconflicts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SolveMergeconflicts.Location = new System.Drawing.Point(3, 249);
            this.SolveMergeconflicts.Name = "SolveMergeconflicts";
            this.SolveMergeconflicts.Size = new System.Drawing.Size(161, 49);
            this.SolveMergeconflicts.TabIndex = 41;
            this.SolveMergeconflicts.Text = "There are unresolved merge conflicts\r\n";
            this.SolveMergeconflicts.UseVisualStyleBackColor = false;
            this.SolveMergeconflicts.Visible = false;
            this.SolveMergeconflicts.Click += new System.EventHandler(this.SolveMergeConflictsClick);
            // 
            // ContinuePanel
            // 
            this.ContinuePanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ContinuePanel.Controls.Add(this.Resolved);
            this.ContinuePanel.Location = new System.Drawing.Point(3, 147);
            this.ContinuePanel.Name = "ContinuePanel";
            this.ContinuePanel.Size = new System.Drawing.Size(160, 34);
            this.ContinuePanel.TabIndex = 37;
            // 
            // MergeToolPanel
            // 
            this.MergeToolPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.MergeToolPanel.Controls.Add(this.Mergetool);
            this.MergeToolPanel.Location = new System.Drawing.Point(3, 3);
            this.MergeToolPanel.Name = "MergeToolPanel";
            this.MergeToolPanel.Size = new System.Drawing.Size(161, 34);
            this.MergeToolPanel.TabIndex = 31;
            // 
            // MainLayout
            // 
            this.MainLayout.ColumnCount = 3;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.Controls.Add(this.PanelLeftImage, 0, 0);
            this.MainLayout.Controls.Add(this.PanelRight, 2, 0);
            this.MainLayout.Controls.Add(this.PanelMiddle, 1, 0);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayout.Location = new System.Drawing.Point(0, 0);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 1;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 472F));
            this.MainLayout.Size = new System.Drawing.Size(1159, 472);
            this.MainLayout.TabIndex = 0;
            // 
            // PanelLeftImage
            // 
            this.PanelLeftImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelLeftImage.AutoSize = true;
            this.PanelLeftImage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelLeftImage.Image1 = global::GitUI.Properties.Images.HelpCommandRebase;
            this.PanelLeftImage.Image2 = null;
            this.PanelLeftImage.IsExpanded = true;
            this.PanelLeftImage.IsOnHoverShowImage2 = false;
            this.PanelLeftImage.IsOnHoverShowImage2NoticeText = "Hover to see scenario when fast forward is possible.";
            this.PanelLeftImage.Location = new System.Drawing.Point(3, 3);
            this.PanelLeftImage.MinimumSize = new System.Drawing.Size(289, 418);
            this.PanelLeftImage.Name = "PanelLeftImage";
            this.PanelLeftImage.Size = new System.Drawing.Size(289, 466);
            this.PanelLeftImage.TabIndex = 1;
            this.PanelLeftImage.UniqueIsExpandedSettingsId = "Rebase";
            // 
            // PanelRight
            // 
            this.PanelRight.AutoSize = true;
            this.PanelRight.Controls.Add(this.MergeToolPanel);
            this.PanelRight.Controls.Add(this.panel3);
            this.PanelRight.Controls.Add(this.AddFiles);
            this.PanelRight.Controls.Add(this.Commit);
            this.PanelRight.Controls.Add(this.panel4);
            this.PanelRight.Controls.Add(this.ContinuePanel);
            this.PanelRight.Controls.Add(this.Skip);
            this.PanelRight.Controls.Add(this.Abort);
            this.PanelRight.Controls.Add(this.SolveMergeconflicts);
            this.PanelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelRight.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PanelRight.Location = new System.Drawing.Point(988, 3);
            this.PanelRight.Name = "PanelRight";
            this.PanelRight.Size = new System.Drawing.Size(168, 466);
            this.PanelRight.TabIndex = 27;
            this.PanelRight.WrapContents = false;
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(3, 43);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(10, 15);
            this.panel3.TabIndex = 33;
            // 
            // Commit
            // 
            this.Commit.Location = new System.Drawing.Point(3, 95);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(162, 25);
            this.Commit.TabIndex = 35;
            this.Commit.Text = "C&ommit...";
            this.Commit.UseVisualStyleBackColor = true;
            this.Commit.Click += new System.EventHandler(this.Commit_Click);
            // 
            // panel4
            // 
            this.panel4.Location = new System.Drawing.Point(3, 126);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(10, 15);
            this.panel4.TabIndex = 36;
            // 
            // PanelMiddle
            // 
            this.PanelMiddle.ColumnCount = 1;
            this.PanelMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.PanelMiddle.Controls.Add(this.lblRebase, 0, 0);
            this.PanelMiddle.Controls.Add(this.PanelCurrentBranch, 0, 1);
            this.PanelMiddle.Controls.Add(this.PatchGrid, 0, 5);
            this.PanelMiddle.Controls.Add(this.lblCommitsToReapply, 0, 4);
            this.PanelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelMiddle.Location = new System.Drawing.Point(298, 3);
            this.PanelMiddle.Name = "PanelMiddle";
            this.PanelMiddle.RowCount = 6;
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.PanelMiddle.Size = new System.Drawing.Size(684, 466);
            this.PanelMiddle.TabIndex = 2;
            // 
            // FormRebaseConflicts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(1159, 472);
            this.Controls.Add(this.MainLayout);
            this.MinimumSize = new System.Drawing.Size(1175, 510);
            this.Name = "FormRebaseConflicts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rebase";
            this.PanelCurrentBranch.ResumeLayout(false);
            this.PanelCurrentBranch.PerformLayout();
            this.ContinuePanel.ResumeLayout(false);
            this.MergeToolPanel.ResumeLayout(false);
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.PanelRight.ResumeLayout(false);
            this.PanelMiddle.ResumeLayout(false);
            this.PanelMiddle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblRebase;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.Button AddFiles;
        private System.Windows.Forms.Button Resolved;
        private System.Windows.Forms.Button Abort;
        private System.Windows.Forms.Button Skip;
        private System.Windows.Forms.Button Mergetool;
        private PatchGrid PatchGrid;
        private System.Windows.Forms.Label lblCommitsToReapply;
        private System.Windows.Forms.Button SolveMergeconflicts;
        private System.Windows.Forms.Panel ContinuePanel;
        private System.Windows.Forms.Panel MergeToolPanel;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.FlowLayoutPanel PanelRight;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.FlowLayoutPanel PanelCurrentBranch;
        private System.Windows.Forms.Label Currentbranch;
        private System.Windows.Forms.TableLayoutPanel PanelMiddle;
        private Help.HelpImageDisplayUserControl PanelLeftImage;
        private System.Windows.Forms.Button Commit;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

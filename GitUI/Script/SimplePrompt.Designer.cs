namespace GitUI.Script
{
    partial class SimplePrompt
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
            if (disposing && (components != null))
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
			this.btn_OK = new System.Windows.Forms.Button();
			this.txt_UserInput = new System.Windows.Forms.TextBox();
			this.InputLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btn_OK
			// 
			this.btn_OK.Location = new System.Drawing.Point(388, 94);
			this.btn_OK.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.btn_OK.Name = "btn_OK";
			this.btn_OK.Size = new System.Drawing.Size(150, 46);
			this.btn_OK.TabIndex = 0;
			this.btn_OK.Text = "&OK";
			this.btn_OK.UseVisualStyleBackColor = true;
			this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
			// 
			// txt_UserInput
			// 
			this.txt_UserInput.Location = new System.Drawing.Point(24, 42);
			this.txt_UserInput.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.txt_UserInput.Name = "txt_UserInput";
			this.txt_UserInput.Size = new System.Drawing.Size(510, 31);
			this.txt_UserInput.TabIndex = 1;
			// 
			// InputLabel
			// 
			this.InputLabel.AutoSize = true;
			this.InputLabel.Location = new System.Drawing.Point(20, 10);
			this.InputLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.InputLabel.Name = "InputLabel";
			this.InputLabel.Size = new System.Drawing.Size(259, 25);
			this.InputLabel.TabIndex = 2;
			this.InputLabel.Text = "Please specify your input:";
			// 
			// SimplePrompt
			// 
			this.AcceptButton = this.btn_OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(562, 146);
			this.Controls.Add(this.InputLabel);
			this.Controls.Add(this.txt_UserInput);
			this.Controls.Add(this.btn_OK);
			this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.Name = "SimplePrompt";
			this.Text = "SimplePrompt";
			this.Shown += new System.EventHandler(this.SimplePrompt_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.TextBox txt_UserInput;
        private System.Windows.Forms.Label InputLabel;
    }
}
using System;
using System.Windows.Forms;

namespace GitHub3
{
    public partial class UserInputPrompt : Form
    {
        public string UserInput { get; private set; } = "";

        public UserInputPrompt(string title, string label, bool isPassword = false)
        {
            InitializeComponent();
            Text = title;
            labelInput.Text = label;
            if (isPassword)
            {
                txt_UserInput.PasswordChar = '*';
            }
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            UserInput = txt_UserInput.Text;
            Close();
        }

        private void SimplePrompt_Shown(object sender, EventArgs e)
        {
            txt_UserInput.Focus();
        }
    }
}

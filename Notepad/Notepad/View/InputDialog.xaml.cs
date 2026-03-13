using System.Windows;

namespace Notepad.View
{
    public partial class InputDialog : Window
    {
        public string InputText => txtInput.Text;

        public InputDialog(string title, string prompt, string defaultText = "")
        {
            InitializeComponent();

            Title = title;
            lblPrompt.Text = prompt;
            txtInput.Text = defaultText;
            txtInput.Focus();
            txtInput.SelectAll();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
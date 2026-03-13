using System.Windows;

namespace Notepad.View
{
    public partial class ReplaceDialog : Window
    {
        public string FindText => txtFind.Text;
        public string ReplaceText => txtReplace.Text;

        public ReplaceDialog()
        {
            InitializeComponent();
            txtFind.Focus();
        }

        private void BtnReplace_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
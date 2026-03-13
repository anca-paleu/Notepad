using System.Windows;

namespace Notepad.View
{
    public partial class FindDialog : Window
    {
        public string SearchText => txtSearch.Text;

        public FindDialog()
        {
            InitializeComponent();
            txtSearch.Focus(); 
        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; 
        }
    }
}
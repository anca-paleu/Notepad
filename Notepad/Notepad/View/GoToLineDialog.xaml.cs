using System.Windows;

namespace Notepad.View
{
    public partial class GoToLineDialog : Window
    {
        public int LineNumber { get; private set; }

        public GoToLineDialog()
        {
            InitializeComponent();
            txtLine.Focus();
        }

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtLine.Text, out int line))
            {
                LineNumber = line;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter a valid number.", "Go To Line", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
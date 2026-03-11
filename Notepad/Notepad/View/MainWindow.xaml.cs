using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Notepad.ViewModels;

namespace Notepad
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = (MainViewModel)DataContext;

            vm.ScrollToSearchResult += (index, length) =>
            {
                var textBox = FindActiveTextBox();
                if (textBox == null) return;

                textBox.Focus();
                textBox.CaretIndex = index;
                textBox.Select(index, length);

                int lineIndex = textBox.GetLineIndexFromCharacterIndex(index);
                if (lineIndex >= 0)
                    textBox.ScrollToLine(lineIndex);
            };
        }

        private TextBox FindActiveTextBox()
        {
            // Cauta TextBox-ul doar in continutul tab-ului activ
            // MainTabControl e numele pe care l-am adaugat in XAML
            var selectedContent = MainTabControl.SelectedContent as DependencyObject;
            return FindChild<TextBox>(selectedContent);
        }

        private static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;

                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
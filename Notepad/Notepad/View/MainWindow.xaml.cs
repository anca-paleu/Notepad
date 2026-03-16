using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Notepad.ViewModels;

namespace Notepad
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += OnWindowClosing;
            this.Loaded += (s, e) =>
            {
                var vm = DataContext as MainViewModel;
                if (vm == null) return;

                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(vm.IsFolderExplorerVisible))
                    {
                        if (!vm.IsFolderExplorerVisible)
                        {
                            ((Grid)Content).ColumnDefinitions[0].Width = new GridLength(0);
                        }
                    }
                };

                vm.ScrollToSearchResult += (index, length) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var textBox = FindActiveTextBox();
                        if (textBox == null) return;
                        textBox.Focus();
                        textBox.CaretIndex = index;
                        textBox.Select(index, length);
                        int lineIndex = textBox.GetLineIndexFromCharacterIndex(index);
                        if (lineIndex >= 0)
                            textBox.ScrollToLine(lineIndex);
                    }), DispatcherPriority.Background);
                };
            };
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;
            if (!vm.CanClose())
                e.Cancel = true;
        }

        private TextBox FindActiveTextBox()
        {
            return FindChild<TextBox>(MainTabControl);
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

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Focus();
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
    }
}
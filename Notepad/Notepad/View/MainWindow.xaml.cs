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

                vm.ScrollToLine += charIndex =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var textBox = FindActiveTextBox();
                        if (textBox == null) return;
                        textBox.Focus();
                        textBox.CaretIndex = charIndex;
                        textBox.Select(charIndex, 0);
                        int lineIndex = textBox.GetLineIndexFromCharacterIndex(charIndex);
                        if (lineIndex >= 0)
                            textBox.ScrollToLine(lineIndex);
                    }), DispatcherPriority.Background);
                };

                vm.RequestSelectedText = () => FindActiveTextBox()?.SelectedText ?? "";
                vm.RequestSelectionStart = () => FindActiveTextBox()?.SelectionStart ?? -1;
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
    }
}
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Notepad.ViewModels
{
    public class DialogService
    {
        private Window CreateDialog(string title, int height) => new Window
        {
            Title = title,
            Height = height,
            Width = 350,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        private DockPanel CreateRow(string label, out TextBox textBox)
        {
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
            row.Children.Add(new TextBlock { Text = label, Width = 90, VerticalAlignment = VerticalAlignment.Center });
            textBox = new TextBox { VerticalAlignment = VerticalAlignment.Center };
            row.Children.Add(textBox);
            return row;
        }

        private Button CreateButton(string label, int width, Action onClick)
        {
            var btn = new Button { Content = label, Width = width, Margin = new Thickness(0, 0, 5, 0) };
            btn.Click += (s, e) => onClick();
            return btn;
        }

        public void ShowFind(Action<string> onFind)
        {
            var win = CreateDialog("Find", 130);
            var panel = new StackPanel { Margin = new Thickness(15) };
            panel.Children.Add(CreateRow("Find:", out var searchBox));
            panel.Children.Add(CreateButton("Find", 75, () => { onFind(searchBox.Text); win.Close(); }));
            win.Content = panel;
            win.ShowDialog();
        }

        public void ShowReplace(Action<string, string> onReplace)
        {
            var win = CreateDialog("Replace", 170);
            var panel = new StackPanel { Margin = new Thickness(15) };
            panel.Children.Add(CreateRow("Find:", out var searchBox));
            panel.Children.Add(CreateRow("Replace with:", out var replaceBox));

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttons.Children.Add(CreateButton("Replace", 75, () => { onReplace(searchBox.Text, replaceBox.Text); win.Close(); }));
            buttons.Children.Add(CreateButton("Cancel", 75, () => win.Close()));
            panel.Children.Add(buttons);

            win.Content = panel;
            win.ShowDialog();
        }

        public void ShowAbout()
        {
            var win = CreateDialog("About", 150);
            var panel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock { Text = "Paleu Anca-Nicoleta", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center });
            panel.Children.Add(new TextBlock { Text = "Grupa 10LF243", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 0) });

            var link = new Hyperlink { NavigateUri = new Uri("mailto:anca.paleu@student.unitbv.ro") };
            link.Inlines.Add("anca.paleu@student.unitbv.ro");
            link.RequestNavigate += (s, e) => Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });

            var linkBlock = new TextBlock { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 0) };
            linkBlock.Inlines.Add(link);
            panel.Children.Add(linkBlock);

            win.Content = panel;
            win.ShowDialog();
        }
    }
}
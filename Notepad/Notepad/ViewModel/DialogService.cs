using System;
using System.Windows;
using Microsoft.Win32;
using Notepad.View;

namespace Notepad.ViewModels
{
    public class DialogService
    {
        private Window MainWindow => Application.Current.MainWindow;
        private const string DefaultFileFilter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

        public void ShowFind(Action<string> onFind)
        {
            var dialog = new FindDialog { Owner = MainWindow };
            if (dialog.ShowDialog() == true) onFind(dialog.SearchText);
        }

        public void ShowReplace(Action<string, string> onReplace)
        {
            var dialog = new ReplaceDialog { Owner = MainWindow };
            if (dialog.ShowDialog() == true) onReplace(dialog.FindText, dialog.ReplaceText);
        }

        public void ShowAbout()
        {
            var dialog = new AboutDialog { Owner = MainWindow };
            dialog.ShowDialog();
        }

        public string ShowInput(string title, string prompt, string defaultText = "")
        {
            var dialog = new InputDialog(title, prompt, defaultText) { Owner = MainWindow };
            if (dialog.ShowDialog() == true)
            {
                return dialog.InputText;
            }
            return null;
        }
        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool? AskToSave(string fileName)
        {
            var result = MessageBox.Show(MainWindow,
                $"Do you want to save changes to \"{fileName}\"?",
                "Notepad", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) return true;
            if (result == MessageBoxResult.No) return false;
            return null;
        }

        public string ShowOpenFileDialog()
        {
            var dialog = new OpenFileDialog { Filter = DefaultFileFilter };
            if (dialog.ShowDialog(MainWindow) == true)
            {
                return dialog.FileName;
            }
            else
            {
                return null;
            }
        }

        public string ShowSaveFileDialog(string defaultFileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = defaultFileName
            };

            if (dialog.ShowDialog(MainWindow) == true)
            {
                return dialog.FileName;
            }
            else
            {
                return null;
            }
        }
    }
}
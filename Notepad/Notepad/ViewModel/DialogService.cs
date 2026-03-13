using System;
using System.Windows;
using Notepad.View;
namespace Notepad.ViewModels
{
    public class DialogService
    {
        private Window MainWindow => Application.Current.MainWindow;

        public void ShowFind(Action<string> onFind)
        {
            var dialog = new FindDialog { Owner = MainWindow };
            if (dialog.ShowDialog() == true)
            {
                onFind(dialog.SearchText);
            }
        }

        public void ShowReplace(Action<string, string> onReplace)
        {
            var dialog = new ReplaceDialog { Owner = MainWindow };
            if (dialog.ShowDialog() == true)
            {
                onReplace(dialog.FindText, dialog.ReplaceText);
            }
        }

        public void ShowGoToLine(Action<int> onGoTo)
        {
            var dialog = new GoToLineDialog { Owner = MainWindow };
            if (dialog.ShowDialog() == true)
            {
                onGoTo(dialog.LineNumber);
            }
        }

        public void ShowAbout()
        {
            var dialog = new AboutDialog { Owner = MainWindow };
            dialog.ShowDialog();
        }
    }
}
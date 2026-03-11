using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Notepad.Model;
using System.Collections.ObjectModel;

namespace Notepad.ViewModels
{
    public class FileOperations
    {
        private readonly ObservableCollection<DocumentModel> _documents;
        private readonly System.Action<DocumentModel> _setSelected;
        private readonly System.Func<DocumentModel> _getSelected;

        public FileOperations(ObservableCollection<DocumentModel> documents,
                              System.Func<DocumentModel> getSelected,
                              System.Action<DocumentModel> setSelected)
        {
            _documents = documents;
            _getSelected = getSelected;
            _setSelected = setSelected;
        }

        public void CreateNewFile()
        {
            int number = 1;
            while (_documents.Any(d => d.FileName == $"new {number}" || d.FileName == $"new {number}*"))
                number++;

            var newTab = new DocumentModel
            {
                FileName = $"new {number}",
                TextContent = "",
                IsModified = false
            };

            _documents.Add(newTab);
            _setSelected(newTab);
        }

        public bool SaveFile()
        {
            var selected = _getSelected();
            if (selected == null) return false;

            if (string.IsNullOrEmpty(selected.FilePath))
                return SaveFileAs();

            File.WriteAllText(selected.FilePath, selected.TextContent);
            selected.IsModified = false;
            return true;
        }

        public bool SaveFileAs()
        {
            var selected = _getSelected();
            if (selected == null) return false;

            var dialog = new SaveFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.FileName = selected.FileName;

            if (dialog.ShowDialog() == true)
            {
                selected.FilePath = dialog.FileName;
                selected.FileName = Path.GetFileName(dialog.FileName);
                File.WriteAllText(selected.FilePath, selected.TextContent);
                selected.IsModified = false;
                return true;
            }

            return false;
        }

        public void OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                foreach (var doc in _documents)
                {
                    if (doc.FilePath == dialog.FileName)
                    {
                        _setSelected(doc);
                        return;
                    }
                }

                string text = File.ReadAllText(dialog.FileName);
                var opened = new DocumentModel
                {
                    FilePath = dialog.FileName,
                    FileName = Path.GetFileName(dialog.FileName),
                    TextContent = text,
                    IsModified = false
                };

                _documents.Add(opened);
                _setSelected(opened);
            }
        }

        public void CloseFile()
        {
            var selected = _getSelected();
            if (selected == null) return;

            if (selected.IsModified)
            {
                var result = MessageBox.Show(
                    $"Save file \"{selected.FileName}\"?",
                    "Notepad", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.Yes)
                {
                    bool didSave = SaveFile();
                    if (!didSave) return;
                }
            }

            _documents.Remove(selected);

            if (_documents.Count == 0)
                CreateNewFile();
            else
                _setSelected(_documents[_documents.Count - 1]);
        }

        public void CloseAllFiles()
        {
            if (!ConfirmAndSaveAll()) return;
            _documents.Clear();
            CreateNewFile();
        }

        public bool ConfirmAndSaveAll()
        {
            foreach (var doc in _documents.ToList())
            {
                if (doc.IsModified)
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"Save file \"{doc.FileName}\"?",
                        "Notepad",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Cancel) return false;

                    if (result == MessageBoxResult.Yes)
                    {
                        if (string.IsNullOrEmpty(doc.FilePath))
                        {
                            SaveFileDialog dialog = new SaveFileDialog();
                            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                            dialog.FileName = doc.FileName;

                            if (dialog.ShowDialog() == true)
                            {
                                doc.FilePath = dialog.FileName;
                                doc.FileName = Path.GetFileName(dialog.FileName);
                                File.WriteAllText(doc.FilePath, doc.TextContent);
                                doc.IsModified = false;
                            }
                            else return false;
                        }
                        else
                        {
                            File.WriteAllText(doc.FilePath, doc.TextContent);
                            doc.IsModified = false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
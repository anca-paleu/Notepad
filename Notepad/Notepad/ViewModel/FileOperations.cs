using System;
using System.IO;
using System.Linq;
using Notepad.Model;
using System.Collections.ObjectModel;

namespace Notepad.ViewModels
{
    public class FileOperations
    {
        private readonly ObservableCollection<DocumentModel> _documents;
        private readonly Action<DocumentModel> _setSelected;
        private readonly Func<DocumentModel> _getSelected;
        private readonly DialogService _dialogService;
        private const string DefaultNewFileName = "new";
        public FileOperations(ObservableCollection<DocumentModel> documents,
                              Func<DocumentModel> getSelected,
                              Action<DocumentModel> setSelected,
                              DialogService dialogService) 
        {
            _documents = documents;
            _getSelected = getSelected;
            _setSelected = setSelected;
            _dialogService = dialogService;
        }

        public void CreateNewFile()
        {
            int number = 1;
            while (_documents.Any(d => d.FileName == $"{DefaultNewFileName} {number}" ||
                                       d.FileName == $"{DefaultNewFileName} {number}*"))
            {
                number++;
            }

            var newTab = new DocumentModel
            {
                FileName = $"{DefaultNewFileName} {number}",
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

            try
            {
                File.WriteAllText(selected.FilePath, selected.TextContent);
                selected.IsModified = false;
                return true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Could not save the file.\nError: {ex.Message}", "Save Error");
                return false;
            }
        }

        public bool SaveFileAs()
        {
            var selected = _getSelected();
            if (selected == null) return false;

            string filePath = _dialogService.ShowSaveFileDialog(selected.FileName);

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    File.WriteAllText(filePath, selected.TextContent);

                    selected.FilePath = filePath;
                    selected.FileName = Path.GetFileName(filePath);
                    selected.IsModified = false;
                    return true;
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Could not save the file as '{filePath}'.\nError: {ex.Message}", "Save As Error");
                    return false;
                }
            }

            return false;
        }

        public void OpenFile()
        {
            string filePath = _dialogService.ShowOpenFileDialog();

            if (!string.IsNullOrEmpty(filePath))
            {
                foreach (var doc in _documents)
                {
                    if (doc.FilePath == filePath)
                    {
                        _setSelected(doc);
                        return;
                    }
                }

                try
                {
                    string text = File.ReadAllText(filePath);
                    var opened = new DocumentModel
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        TextContent = text,
                        IsModified = false
                    };

                    _documents.Add(opened);
                    _setSelected(opened);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Could not open the file.\nError: {ex.Message}", "Open Error");
                }
            }
        }

        public void CloseFile()
        {
            var selected = _getSelected();
            if (selected == null) return;

            if (selected.IsModified)
            {
                bool? shouldSave = _dialogService.AskToSave(selected.FileName);

                if (shouldSave == null) return; 
                if (shouldSave == true)
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
                    bool? shouldSave = _dialogService.AskToSave(doc.FileName);

                    if (shouldSave == null) return false;

                    if (shouldSave == true)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(doc.FilePath))
                            {
                                string filePath = _dialogService.ShowSaveFileDialog(doc.FileName);
                                if (!string.IsNullOrEmpty(filePath))
                                {
                                    File.WriteAllText(filePath, doc.TextContent);
                                    doc.FilePath = filePath;
                                    doc.FileName = Path.GetFileName(filePath);
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
                        catch (Exception ex)
                        {
                            _dialogService.ShowError($"Could not save the file '{doc.FileName}'.\nError: {ex.Message}", "Save Error");
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
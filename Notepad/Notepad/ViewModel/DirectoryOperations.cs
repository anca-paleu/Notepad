using System;
using System.IO;
using System.Windows;
using Notepad.Model;
using System.Collections.ObjectModel;

namespace Notepad.ViewModels
{
    public class DirectoryOperations
    {
        private readonly ObservableCollection<DocumentModel> _documents;
        private readonly Action<DocumentModel> _setSelected;
        private readonly Func<DocumentModel> _getSelected;
        private readonly DialogService _dialogService; 

        public string ClipboardFolderPath { get; private set; }

        public DirectoryOperations(ObservableCollection<DocumentModel> documents,
                                   Func<DocumentModel> getSelected,
                                   Action<DocumentModel> setSelected,
                                   DialogService dialogService)
        {
            _documents = documents;
            _getSelected = getSelected;
            _setSelected = setSelected;
            _dialogService = dialogService;
        }

        public void NewFileInFolder(object param)
        {
            try
            {
                if (param is DirectoryItem folder && folder.IsDirectory)
                {
                    string baseName = "NewFile";
                    string extension = ".txt";
                    string defaultName = baseName + extension;

                    string checkPath = Path.Combine(folder.FullPath, defaultName);
                    int counter = 1;

                    while (File.Exists(checkPath))
                    {
                        defaultName = $"{baseName} ({counter}){extension}";
                        checkPath = Path.Combine(folder.FullPath, defaultName);
                        counter++;
                    }

                    string fileName = _dialogService.ShowInput("New File", "Enter file name with extension:", defaultName);

                    if (string.IsNullOrWhiteSpace(fileName)) return;

                    string newFilePath = Path.Combine(folder.FullPath, fileName);

                    if (File.Exists(newFilePath))
                    {
                        _dialogService.ShowWarning($"A file named '{fileName}' already exists in this folder.", "File Exists");
                        return;
                    }

                    File.WriteAllText(newFilePath, string.Empty);
                    RefreshFolder(folder);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to create a new file.\nError: {ex.Message}", "Create File Error");
            }
        }

        public void OpenFileFromTree(object param)
        {
            if (param is DirectoryItem node)
            {
                if (Directory.Exists(node.FullPath)) return;

                foreach (var doc in _documents)
                {
                    if (doc.FilePath == node.FullPath) { _setSelected(doc); return; }
                }

                try
                {
                    string content = File.ReadAllText(node.FullPath);
                    var newTab = new DocumentModel
                    {
                        FileName = node.Name,
                        FilePath = node.FullPath,
                        TextContent = content,
                        IsModified = false
                    };
                    _documents.Add(newTab);
                    _setSelected(newTab);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Could not open the file '{node.Name}'.\nError: {ex.Message}", "Open File Error");
                }
            }
        }

        public void CopyPath(object param)
        {
            if (param is DirectoryItem folder && folder.IsDirectory && !string.IsNullOrEmpty(folder.FullPath))
            {
                try
                {
                    Clipboard.SetText(folder.FullPath);
                }
                catch (System.Runtime.InteropServices.COMException) { }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Copy error: {ex.Message}", "Error");
                }
            }
        }

        public void CopyFolder(object param)
        {
            try
            {
                if (param is DirectoryItem folder && folder.IsDirectory)
                {
                    ClipboardFolderPath = folder.FullPath;
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to copy the folder.\nError: {ex.Message}", "Copy Folder Error");
            }
        }

        public void PasteFolder(object param)
        {
            try
            {
                if (param is DirectoryItem dest && dest.IsDirectory)
                {
                    if (string.IsNullOrEmpty(ClipboardFolderPath) || !Directory.Exists(ClipboardFolderPath)) return;

                    string sourceName = new DirectoryInfo(ClipboardFolderPath).Name;
                    string destPath = Path.Combine(dest.FullPath, sourceName);

                    if (destPath.StartsWith(ClipboardFolderPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _dialogService.ShowWarning("Cannot copy a folder into itself or into one of its subfolders.", "Action Not Allowed");
                        return;
                    }

                    CopyDirectory(ClipboardFolderPath, destPath);
                    RefreshFolder(dest);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"An error occurred during paste.\nError: {ex.Message}", "Paste Folder Error");
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) return;

            Directory.CreateDirectory(destinationDir);

            foreach (var file in dir.GetFiles())
            {
                try { file.CopyTo(Path.Combine(destinationDir, file.Name), true); }
                catch (UnauthorizedAccessException) { }
            }

            foreach (var sub in dir.GetDirectories())
            {
                try { CopyDirectory(sub.FullName, Path.Combine(destinationDir, sub.Name)); }
                catch (UnauthorizedAccessException) { }
            }
        }

        public ObservableCollection<DirectoryItem> GetLogicalDrives()
        {
            var drives = new ObservableCollection<DirectoryItem>();
            foreach (var drive in Directory.GetLogicalDrives())
            {
                var driveItem = new DirectoryItem(LoadChildren) { Name = drive, FullPath = drive, IsDirectory = true };
                driveItem.Children.Add(new DirectoryItem(null) { Name = "..." });
                drives.Add(driveItem);
            }
            return drives;
        }

        public void LoadChildren(DirectoryItem item)
        {
            if (item.Children.Count == 1 && item.Children[0].Name == "...")
            {
                item.Children.Clear();

                try
                {
                    foreach (var dir in Directory.GetDirectories(item.FullPath))
                    {
                        var subDir = new DirectoryItem(LoadChildren)
                        {
                            Name = new DirectoryInfo(dir).Name,
                            FullPath = dir,
                            IsDirectory = true
                        };
                        subDir.Children.Add(new DirectoryItem(null) { Name = "..." });
                        item.Children.Add(subDir);
                    }

                    foreach (var file in Directory.GetFiles(item.FullPath))
                    {
                        item.Children.Add(new DirectoryItem(null)
                        {
                            Name = Path.GetFileName(file),
                            FullPath = file,
                            IsDirectory = false
                        });
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (Exception ex)
                {
                    _dialogService.ShowWarning($"Failed to load '{item.Name}'.\nError: {ex.Message}", "Directory Error");
                }
            }
        }

        private void RefreshFolder(DirectoryItem folder)
        {
            folder.Children.Clear();
            folder.Children.Add(new DirectoryItem(null) { Name = "..." });
            folder.IsExpanded = false;
            folder.IsExpanded = true;
        }
    }
}
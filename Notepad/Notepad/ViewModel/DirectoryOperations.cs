using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Notepad.Model;
using System.Collections.ObjectModel;

namespace Notepad.ViewModels
{
    public class DirectoryOperations
    {
        private readonly ObservableCollection<DocumentModel> _documents;
        private readonly Action<DocumentModel> _setSelected;
        private readonly Func<DocumentModel> _getSelected;
        public string ClipboardFolderPath { get; private set; }

        public DirectoryOperations(ObservableCollection<DocumentModel> documents,
                                   Func<DocumentModel> getSelected,
                                   Action<DocumentModel> setSelected)
        {
            _documents = documents;
            _getSelected = getSelected;
            _setSelected = setSelected;
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
                    MessageBox.Show($"Error opening file: {ex.Message}");
                }
            }
        }

        public void NewFileInFolder(object param)
        {
            try
            {
                if (param is DirectoryItem folder && folder.IsDirectory)
                {
                    string newFileName = "NewFile.txt";
                    string newFilePath = Path.Combine(folder.FullPath, newFileName);

                    int counter = 1;
                    while (File.Exists(newFilePath))
                    {
                        newFileName = $"NewFile ({counter}).txt";
                        newFilePath = Path.Combine(folder.FullPath, newFileName);
                        counter++;
                    }

                    File.WriteAllText(newFilePath, string.Empty);
                    RefreshFolder(folder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CopyPath(object param)
        {
            if (param is DirectoryItem folder && folder.IsDirectory && !string.IsNullOrEmpty(folder.FullPath))
            {
                try { Clipboard.SetDataObject(folder.FullPath, true); }
                catch (System.Runtime.InteropServices.COMException) { }
                catch (Exception ex) { MessageBox.Show($"Copy error: {ex.Message}"); }
            }
        }

        public void CopyFolder(object param)
        {
            try
            {
                if (param is DirectoryItem folder && folder.IsDirectory)
                {
                    ClipboardFolderPath = folder.FullPath;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A problem occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show("Cannot copy a folder into itself or into one of its subfolders.", "Action Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    CopyDirectory(ClipboardFolderPath, destPath);
                    RefreshFolder(dest);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Paste error: {ex.Message}", "Paste Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            try
            {
                var dir = new DirectoryInfo(sourceDir);
                if (!dir.Exists) return;
                Directory.CreateDirectory(destinationDir);
                try { foreach (var file in dir.GetFiles()) file.CopyTo(Path.Combine(destinationDir, file.Name), true); } catch { }
                try { foreach (var sub in dir.GetDirectories()) CopyDirectory(sub.FullName, Path.Combine(destinationDir, sub.Name)); } catch { }
            }
            catch { }
        }

        private void RefreshFolder(DirectoryItem folder)
        {
            folder.Children.Clear();
            folder.Children.Add(new DirectoryItem { Name = "..." });
            folder.IsExpanded = false;
            folder.IsExpanded = true;
        }
    }
}
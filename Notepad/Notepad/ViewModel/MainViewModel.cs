using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Notepad.Model;
using Notepad.ViewModel;
using System.Linq;


namespace Notepad.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private DocumentModel _selectedDocument;
        public ObservableCollection<DocumentModel> Documents { get; set; }

        public DocumentModel SelectedDocument
        {
            get { return _selectedDocument; }
            set
            {
                _selectedDocument = value;
                OnPropertyChanged();
            }
        }


        public ICommand NewFileCommand { get; }
        public ICommand CloseFileCommand { get; }
        public ICommand SaveCommand { get; } 
        public ICommand SaveAsCommand { get; }

        public ICommand OpenFileCommand { get; }
        public ICommand ViewStandardCommand { get; }
        public ICommand ViewFolderExplorerCommand { get; }

        public ObservableCollection<DirectoryItem> Directories { get; set; }

        public ICommand OpenFileFromTreeCommand { get; }

        private string _clipboardFolderPath;
        public ICommand NewFileInFolderCommand { get; }
        public ICommand CopyPathCommand { get; }
        public ICommand CopyFolderCommand { get; }
        public ICommand PasteFolderCommand { get; }


        public MainViewModel()
        {
            Documents = new ObservableCollection<DocumentModel>();

            NewFileCommand = new RelayCommand(param => CreateNewFile());

            CloseFileCommand = new RelayCommand(param => CloseFile());

            SaveCommand = new RelayCommand(param => SaveFile());

            SaveAsCommand = new RelayCommand(param => SaveFileAs());

            OpenFileCommand = new RelayCommand(param => OpenFile());

            IsFolderExplorerVisible = false;

            Directories = new ObservableCollection<DirectoryItem>();

            foreach (var drive in Directory.GetLogicalDrives())
            {
                var driveItem = new DirectoryItem { Name = drive, FullPath = drive, IsDirectory = true};

                driveItem.Children.Add(new DirectoryItem { Name = "..." });

                Directories.Add(driveItem);
            }

            ViewStandardCommand = new RelayCommand(param => IsFolderExplorerVisible = false);

            ViewFolderExplorerCommand = new RelayCommand(param => IsFolderExplorerVisible = true);

            OpenFileFromTreeCommand = new RelayCommand(OpenFileFromTree);

            NewFileInFolderCommand = new RelayCommand(NewFileInFolder);

            CopyPathCommand = new RelayCommand(CopyPath);

            CopyFolderCommand = new RelayCommand(CopyFolder);

            PasteFolderCommand = new RelayCommand(PasteFolder, param => !string.IsNullOrEmpty(_clipboardFolderPath) && Directory.Exists(_clipboardFolderPath));

            CreateNewFile();
        }

        private void CreateNewFile()
        {
            int newNumber = Documents.Count + 1;

            var newTab = new DocumentModel
            {
                FileName = $"new {newNumber}",
                TextContent = "",
                IsModified = false
            };

            Documents.Add(newTab);
            SelectedDocument = newTab; 
        }

        private bool SaveFile()
        {
            if (SelectedDocument == null) return false;

            if (string.IsNullOrEmpty(SelectedDocument.FilePath))
            {
                return SaveFileAs();
            }

            File.WriteAllText(SelectedDocument.FilePath, SelectedDocument.TextContent);
            SelectedDocument.IsModified = false; 
            return true;
        }

        private bool SaveFileAs()
        {
            if (SelectedDocument == null) return false;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.FileName = SelectedDocument.FileName;

            if (dialog.ShowDialog() == true)
            {
                SelectedDocument.FilePath = dialog.FileName; 
                SelectedDocument.FileName = Path.GetFileName(dialog.FileName);

                File.WriteAllText(SelectedDocument.FilePath, SelectedDocument.TextContent); 
                SelectedDocument.IsModified = false; 
                return true;
            }

            return false; 
        }

        private void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                foreach (var doc in Documents)
                {
                    if (doc.FilePath == dialog.FileName)
                    {
                        SelectedDocument = doc;
                        return;
                    }
                }

                string textFisier = File.ReadAllText(dialog.FileName);

                var openedTab = new DocumentModel
                {
                    FilePath = dialog.FileName,
                    FileName = Path.GetFileName(dialog.FileName),
                    TextContent = textFisier,
                    IsModified = false
                };

                Documents.Add(openedTab);
                SelectedDocument = openedTab;
            }
        }
        private void CloseFile()
        {
            if (SelectedDocument == null || Documents.Count <= 1) return;

            bool canClose = true; 

            if (SelectedDocument.IsModified)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Save file \"{SelectedDocument.FileName}\"?",
                    "Notepad",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool didSave = SaveFile();
                    if (!didSave) canClose = false;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    canClose = false;
                }
            }

            if (canClose)
            {
                Documents.Remove(SelectedDocument);
                SelectedDocument = Documents[Documents.Count - 1]; 
            }
        }

        private bool _isFolderExplorerVisible;
        public bool IsFolderExplorerVisible
        {
            get { return _isFolderExplorerVisible; }
            set
            {
                _isFolderExplorerVisible = value;
                OnPropertyChanged();
            }
        }


        private void OpenFileFromTree(object param)
        {
            if (param is DirectoryItem node)
            {
                if (System.IO.Directory.Exists(node.FullPath)) return;

                var existingTab = Documents.FirstOrDefault(d => d.FilePath == node.FullPath);
                if (existingTab != null)
                {
                    SelectedDocument = existingTab;
                    return;
                }

                try
                {
                    string content = System.IO.File.ReadAllText(node.FullPath);

                    var newTab = new DocumentModel
                    {
                        FileName = node.Name,
                        FilePath = node.FullPath,
                        TextContent = content,
                        IsModified = false
                    };

                    Documents.Add(newTab);
                    SelectedDocument = newTab;
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error opening file: {ex.Message}");
                }
            }
        }
        private void NewFileInFolder(object param)
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

                    folder.Children.Clear();
                    folder.Children.Add(new DirectoryItem { Name = "..." });
                    folder.IsExpanded = false;
                    folder.IsExpanded = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CopyPath(object param)
        {
            if (param is DirectoryItem folder && folder.IsDirectory && !string.IsNullOrEmpty(folder.FullPath))
            {
                try
                {
                    Clipboard.SetDataObject(folder.FullPath, true);
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Copy error: {ex.Message}", "Notepad", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CopyFolder(object param)
        {
            try
            {
                if (param is DirectoryItem folder && folder.IsDirectory)
                {
                    _clipboardFolderPath = folder.FullPath;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A problem occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PasteFolder(object param)
        {
            try
            {
                if (param is DirectoryItem destinationFolder && destinationFolder.IsDirectory)
                {
                    if (string.IsNullOrEmpty(_clipboardFolderPath) || !Directory.Exists(_clipboardFolderPath)) return;

                    string sourceFolderName = new DirectoryInfo(_clipboardFolderPath).Name;
                    string destFolderPath = Path.Combine(destinationFolder.FullPath, sourceFolderName);

                    if (destFolderPath.StartsWith(_clipboardFolderPath, System.StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Cannot copy a folder into itself or into one of its subfolders.", "Action Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning); return;
                    }

                    CopyDirectory(_clipboardFolderPath, destFolderPath);

                    destinationFolder.Children.Clear();
                    destinationFolder.Children.Add(new DirectoryItem { Name = "..." });
                    destinationFolder.IsExpanded = false;
                    destinationFolder.IsExpanded = true;
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
                DirectoryInfo dir = new DirectoryInfo(sourceDir);
                if (!dir.Exists) return;

                Directory.CreateDirectory(destinationDir);

                try
                {
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        string targetFilePath = Path.Combine(destinationDir, file.Name);
                        file.CopyTo(targetFilePath, true);
                    }
                }
                catch {}

                try
                {
                    foreach (DirectoryInfo subDir in dir.GetDirectories())
                    {
                        string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                        CopyDirectory(subDir.FullName, newDestinationDir);
                    }
                }
                catch {}
            }
            catch
            {
            }
        }
    }
}
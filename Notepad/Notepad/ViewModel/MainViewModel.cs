using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Win32;
using Notepad.Model;
using Notepad.ViewModel;
using Notepad.ViewModels;

namespace Notepad.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private DocumentModel _selectedDocument;
        public ObservableCollection<DocumentModel> Documents { get; set; }

        public DocumentModel SelectedDocument
        {
            get { return _selectedDocument; }
            set { _selectedDocument = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; OnPropertyChanged(); }
        }

        private string _replaceText;
        public string ReplaceText
        {
            get { return _replaceText; }
            set { _replaceText = value; OnPropertyChanged(); }
        }

        private bool _searchAllTabs;
        public bool SearchAllTabs
        {
            get { return _searchAllTabs; }
            set { _searchAllTabs = value; OnPropertyChanged(); }
        }

        private bool _isFolderExplorerVisible;
        public bool IsFolderExplorerVisible
        {
            get { return _isFolderExplorerVisible; }
            set { _isFolderExplorerVisible = value; OnPropertyChanged(); }
        }

        public ObservableCollection<DirectoryItem> Directories { get; set; }

        public ICommand NewFileCommand { get; }
        public ICommand CloseFileCommand { get; }
        public ICommand CloseAllFilesCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand ViewStandardCommand { get; }
        public ICommand ViewFolderExplorerCommand { get; }
        public ICommand OpenFileFromTreeCommand { get; }
        public ICommand NewFileInFolderCommand { get; }
        public ICommand CopyPathCommand { get; }
        public ICommand CopyFolderCommand { get; }
        public ICommand PasteFolderCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand ReplaceAllCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand AboutCommand { get; }

        public MainViewModel()
        {
            Documents = new ObservableCollection<DocumentModel>();

            var fileOps = new FileOperations(Documents, () => SelectedDocument, d => SelectedDocument = d);
            var searchOps = new SearchOperations(Documents, () => SelectedDocument, d => SelectedDocument = d);
            var dirOps = new DirectoryOperations(Documents, () => SelectedDocument, d => SelectedDocument = d);

            NewFileCommand = new RelayCommand(param => fileOps.CreateNewFile());
            CloseFileCommand = new RelayCommand(param => fileOps.CloseFile());
            CloseAllFilesCommand = new RelayCommand(param => fileOps.CloseAllFiles());
            SaveCommand = new RelayCommand(param => fileOps.SaveFile());
            SaveAsCommand = new RelayCommand(param => fileOps.SaveFileAs());
            OpenFileCommand = new RelayCommand(param => fileOps.OpenFile());

            OpenFileFromTreeCommand = new RelayCommand(dirOps.OpenFileFromTree);
            NewFileInFolderCommand = new RelayCommand(dirOps.NewFileInFolder);
            CopyPathCommand = new RelayCommand(dirOps.CopyPath);
            CopyFolderCommand = new RelayCommand(dirOps.CopyFolder);
            PasteFolderCommand = new RelayCommand(dirOps.PasteFolder,
                param => !string.IsNullOrEmpty(dirOps.ClipboardFolderPath) && Directory.Exists(dirOps.ClipboardFolderPath));

            ViewStandardCommand = new RelayCommand(param => IsFolderExplorerVisible = false);
            ViewFolderExplorerCommand = new RelayCommand(param => IsFolderExplorerVisible = true);

            ExitCommand = new RelayCommand(param => Application.Current.Shutdown());

            var dialogService = new DialogService();

            FindCommand = new RelayCommand(param => dialogService.ShowFind(text => searchOps.Find(text, SearchAllTabs)));

            ReplaceCommand = new RelayCommand(param => dialogService.ShowReplace(
                (s, r) => searchOps.Replace(s, r, SearchAllTabs)));

            ReplaceAllCommand = new RelayCommand(param => dialogService.ShowReplace(
                (s, r) => searchOps.ReplaceAll(s, r, SearchAllTabs)));

            AboutCommand = new RelayCommand(param => dialogService.ShowAbout());

            Directories = new ObservableCollection<DirectoryItem>();
            foreach (var drive in Directory.GetLogicalDrives())
            {
                var driveItem = new DirectoryItem { Name = drive, FullPath = drive, IsDirectory = true };
                driveItem.Children.Add(new DirectoryItem { Name = "..." });
                Directories.Add(driveItem);
            }

            IsFolderExplorerVisible = false;
            fileOps.CreateNewFile();
        }
    }
}

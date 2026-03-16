using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Notepad.Model;
using Notepad.ViewModel;

namespace Notepad.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private DocumentModel _selectedDocument;
        private FileOperations _fileOps;
        private SearchOperations _searchOps;
        private bool _closeConfirmed = false;

        public ObservableCollection<DocumentModel> Documents { get; set; }

        public DocumentModel SelectedDocument
        {
            get { return _selectedDocument; }
            set { _selectedDocument = value; OnPropertyChanged(); }
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

        public event Action<int, int> ScrollToSearchResult;

        public MainViewModel()
        {
            Documents = new ObservableCollection<DocumentModel>();

            var dialogService = new DialogService();

            _fileOps = new FileOperations(Documents, () => SelectedDocument, d => SelectedDocument = d, dialogService);
            _searchOps = new SearchOperations(Documents, () => SelectedDocument, d => SelectedDocument = d);
            var dirOps = new DirectoryOperations(Documents, () => SelectedDocument, d => SelectedDocument = d, dialogService);

            _searchOps.SearchResultFound += (index, length) =>
            {
                if (ScrollToSearchResult != null)
                {
                    ScrollToSearchResult(index, length);
                }
            };

            NewFileCommand = new RelayCommand(param => _fileOps.CreateNewFile());

            CloseFileCommand = new RelayCommand(param =>
            {
                if (param is DocumentModel doc)
                {
                    var prev = SelectedDocument;
                    SelectedDocument = doc;
                    _fileOps.CloseFile();
                    if (prev != doc && Documents.Contains(prev))
                        SelectedDocument = prev;
                }
                else
                    _fileOps.CloseFile();
            });

            CloseAllFilesCommand = new RelayCommand(param => _fileOps.CloseAllFiles());
            SaveCommand = new RelayCommand(param => _fileOps.SaveFile());
            SaveAsCommand = new RelayCommand(param => _fileOps.SaveFileAs());
            OpenFileCommand = new RelayCommand(param => _fileOps.OpenFile());

            OpenFileFromTreeCommand = new RelayCommand(dirOps.OpenFileFromTree);
            NewFileInFolderCommand = new RelayCommand(dirOps.NewFileInFolder);
            CopyPathCommand = new RelayCommand(dirOps.CopyPath);
            CopyFolderCommand = new RelayCommand(dirOps.CopyFolder);
            PasteFolderCommand = new RelayCommand(dirOps.PasteFolder,
                param => !string.IsNullOrEmpty(dirOps.ClipboardFolderPath) && Directory.Exists(dirOps.ClipboardFolderPath));

            ViewStandardCommand = new RelayCommand(param => IsFolderExplorerVisible = false);
            ViewFolderExplorerCommand = new RelayCommand(param => IsFolderExplorerVisible = true);

            ExitCommand = new RelayCommand(param =>
            {
                if (_fileOps.ConfirmAndSaveAll())
                {
                    _closeConfirmed = true;
                    Application.Current.Shutdown();
                }
            });

            FindCommand = new RelayCommand(param => dialogService.ShowFind(text =>
            {
                _searchOps.Find(text, SearchAllTabs);
            }));

            ReplaceCommand = new RelayCommand(param => dialogService.ShowReplace(
                (s, r) => _searchOps.Replace(s, r, SearchAllTabs)));

            ReplaceAllCommand = new RelayCommand(param => dialogService.ShowReplace(
                (s, r) => _searchOps.ReplaceAll(s, r, SearchAllTabs)));

            AboutCommand = new RelayCommand(param => dialogService.ShowAbout());

            Directories = dirOps.GetLogicalDrives();

            IsFolderExplorerVisible = false;
            _fileOps.CreateNewFile();
        }

        public bool CanClose()
        {
            if (_closeConfirmed) return true;
            return _fileOps.ConfirmAndSaveAll();
        }
    }
}
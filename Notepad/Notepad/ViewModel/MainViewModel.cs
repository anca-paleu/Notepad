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

        private string _lastSearchText = "";
        public string LastSearchText
        {
            get => _lastSearchText;
            set
            {
                _lastSearchText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
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
        public ICommand FindNextCommand { get; }
        public ICommand FindPreviousCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand ReplaceAllCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand ToUpperCaseCommand { get; }
        public ICommand ToLowerCaseCommand { get; }
        public ICommand RemoveEmptyLinesCommand { get; }
        public ICommand GoToLineCommand { get; }
        public ICommand ToggleReadOnlyCommand { get; }

        public event Action<int, int> ScrollToSearchResult;
        public event Action<int> ScrollToLine;
        public Func<string> RequestSelectedText { get; set; }
        public Func<int> RequestSelectionStart { get; set; }

        public MainViewModel()
        {
            Documents = new ObservableCollection<DocumentModel>();

            _fileOps = new FileOperations(Documents, () => SelectedDocument, d => SelectedDocument = d);
            _searchOps = new SearchOperations(Documents, () => SelectedDocument, d => SelectedDocument = d);
            var dirOps = new DirectoryOperations(Documents, () => SelectedDocument, d => SelectedDocument = d);
            var textOps = new TextOperations(
                () => SelectedDocument,
                () => RequestSelectedText?.Invoke() ?? "",
                () => RequestSelectionStart?.Invoke() ?? -1);

            _searchOps.SearchResultFound += (index, length) => ScrollToSearchResult?.Invoke(index, length);
            textOps.ScrollToLine += charIndex => ScrollToLine?.Invoke(charIndex);

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

            var dialogService = new DialogService();

            FindCommand = new RelayCommand(param => dialogService.ShowFind(text =>
            {
                if (!string.IsNullOrEmpty(text))
                    LastSearchText = text;
                _searchOps.Find(text, SearchAllTabs);
            }));

            FindNextCommand = new RelayCommand(
                param => _searchOps.FindNext(LastSearchText, SearchAllTabs),
                param => !string.IsNullOrEmpty(LastSearchText));

            FindPreviousCommand = new RelayCommand(
                param => _searchOps.FindPrevious(LastSearchText, SearchAllTabs),
                param => !string.IsNullOrEmpty(LastSearchText));

            ReplaceCommand = new RelayCommand(param => dialogService.ShowReplace(
                (s, r) => _searchOps.Replace(s, r, SearchAllTabs)));

            ReplaceAllCommand = new RelayCommand(param => dialogService.ShowReplace(
                (s, r) => _searchOps.ReplaceAll(s, r, SearchAllTabs)));

            AboutCommand = new RelayCommand(param => dialogService.ShowAbout());

            CopyCommand = new RelayCommand(param => textOps.Copy(), param => !string.IsNullOrEmpty(RequestSelectedText?.Invoke()));
            CutCommand = new RelayCommand(param => textOps.Cut(), param => !string.IsNullOrEmpty(RequestSelectedText?.Invoke()));
            PasteCommand = new RelayCommand(param => textOps.Paste(), param => textOps.HasInternalClipboard);

            ToUpperCaseCommand = new RelayCommand(param => textOps.ToUpperCase());
            ToLowerCaseCommand = new RelayCommand(param => textOps.ToLowerCase());
            RemoveEmptyLinesCommand = new RelayCommand(param => textOps.RemoveEmptyLines());
            ToggleReadOnlyCommand = new RelayCommand(param => textOps.ToggleReadOnly());
            GoToLineCommand = new RelayCommand(param => dialogService.ShowGoToLine(line => textOps.GoToLine(line)));

            Directories = new ObservableCollection<DirectoryItem>();
            foreach (var drive in Directory.GetLogicalDrives())
            {
                var driveItem = new DirectoryItem { Name = drive, FullPath = drive, IsDirectory = true };
                driveItem.Children.Add(new DirectoryItem { Name = "..." });
                Directories.Add(driveItem);
            }

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
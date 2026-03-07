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

            FindCommand = new RelayCommand(param => searchOps.Find(SearchText, SearchAllTabs));
            ReplaceCommand = new RelayCommand(param => searchOps.Replace(SearchText, ReplaceText, SearchAllTabs));
            ReplaceAllCommand = new RelayCommand(param => searchOps.ReplaceAll(SearchText, ReplaceText, SearchAllTabs));

            OpenFileFromTreeCommand = new RelayCommand(dirOps.OpenFileFromTree);
            NewFileInFolderCommand = new RelayCommand(dirOps.NewFileInFolder);
            CopyPathCommand = new RelayCommand(dirOps.CopyPath);
            CopyFolderCommand = new RelayCommand(dirOps.CopyFolder);
            PasteFolderCommand = new RelayCommand(dirOps.PasteFolder,
                param => !string.IsNullOrEmpty(dirOps.ClipboardFolderPath) && Directory.Exists(dirOps.ClipboardFolderPath));

            ViewStandardCommand = new RelayCommand(param => IsFolderExplorerVisible = false);
            ViewFolderExplorerCommand = new RelayCommand(param => IsFolderExplorerVisible = true);

            ExitCommand = new RelayCommand(param => Application.Current.Shutdown());

            AboutCommand = new RelayCommand(param =>
            {
                var window = new Window
                {
                    Title = "About",
                    Height = 150,
                    Width = 350,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                var panel = new System.Windows.Controls.StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20)
                };
                panel.Children.Add(new System.Windows.Controls.TextBlock { Text = "Paleu Anca-Nicoleta", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center });
                panel.Children.Add(new System.Windows.Controls.TextBlock { Text = "Grupa 10LF243", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 0) });
                var link = new Hyperlink { NavigateUri = new Uri("mailto:anca.paleu@student.unitbv.ro") };
                link.Inlines.Add("anca.paleu@student.unitbv.ro");
                link.RequestNavigate += (s, e) => Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                var linkBlock = new System.Windows.Controls.TextBlock { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 0) };
                linkBlock.Inlines.Add(link);
                panel.Children.Add(linkBlock);
                window.Content = panel;
                window.ShowDialog();
            });

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

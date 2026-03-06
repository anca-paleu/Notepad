using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Notepad.Model;
using Notepad.ViewModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;


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

        public MainViewModel()
        {
            Documents = new ObservableCollection<DocumentModel>();

            NewFileCommand = new RelayCommand(param => CreateNewFile());

            CloseFileCommand = new RelayCommand(param => CloseFile());

            SaveCommand = new RelayCommand(param => SaveFile());

            SaveAsCommand = new RelayCommand(param => SaveFileAs());

            OpenFileCommand = new RelayCommand(param => OpenFile());

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

    }
}
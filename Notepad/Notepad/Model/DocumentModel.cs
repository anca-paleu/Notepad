using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Notepad.Model
{
    public class DocumentModel : INotifyPropertyChanged
    {
        private string _fileName;
        private string _textContent;
        private string _filePath;
        private bool _isModified;

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; } 
        }

        public string TextContent
        {
            get { return _textContent; }
            set
            {
                _textContent = value;
                OnPropertyChanged();
                IsModified = true;
            }
        }

        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                _isModified = value;
                OnPropertyChanged(nameof(DisplayName)); 
            }
        }

        public string DisplayName
        {
            get
            {
                if (_isModified)
                    return _fileName + " *";
                else
                    return _fileName;       
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
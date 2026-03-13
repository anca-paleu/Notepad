using System;
using System.Collections.ObjectModel;
using Notepad.ViewModel;

namespace Notepad.Model
{
    public class DirectoryItem : ObservableObject
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public ObservableCollection<DirectoryItem> Children { get; set; }

        private bool _isExpanded;

        private readonly Action<DirectoryItem> _loadChildrenAction;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged();

                if (_isExpanded && _loadChildrenAction != null)
                {
                    _loadChildrenAction(this);
                }
            }
        }

        public DirectoryItem(Action<DirectoryItem> loadChildrenAction = null)
        {
            Children = new ObservableCollection<DirectoryItem>();
            _loadChildrenAction = loadChildrenAction;
        }
    }
}
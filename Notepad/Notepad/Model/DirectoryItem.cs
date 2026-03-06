using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Notepad.Model
{
    public class DirectoryItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string FullPath { get; set; }

        public ObservableCollection<DirectoryItem> Children { get; set; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged();

                if (_isExpanded)
                {
                    LoadChildren();
                }
            }
        }

        public DirectoryItem()
        {
            Children = new ObservableCollection<DirectoryItem>();
        }

        private void LoadChildren()
        {
            if (Children.Count == 1 && Children[0].Name == "...")
            {
                Children.Clear();

                try
                {
                    foreach (var dir in Directory.GetDirectories(FullPath))
                    {
                        var subDir = new DirectoryItem
                        {
                            Name = new DirectoryInfo(dir).Name,
                            FullPath = dir
                        };
                        subDir.Children.Add(new DirectoryItem { Name = "..." });
                        Children.Add(subDir);
                    }

                    foreach (var file in Directory.GetFiles(FullPath))
                    {
                        Children.Add(new DirectoryItem
                        {
                            Name = Path.GetFileName(file),
                            FullPath = file
                        });
                    }
                }
                catch
                {
                    { }
                }

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
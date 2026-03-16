using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using Notepad.Model;

namespace Notepad.ViewModels
{
    public class SearchOperations
    {
        private readonly ObservableCollection<DocumentModel> _documents;
        private readonly Func<DocumentModel> _getSelected;
        private readonly Action<DocumentModel> _setSelected;

        private int _lastFoundIndex = -1;
        private string _lastSearchText = "";
        private DocumentModel _lastSearchDoc = null;

        public SearchOperations(ObservableCollection<DocumentModel> documents,
                                Func<DocumentModel> getSelected,
                                Action<DocumentModel> setSelected)
        {
            _documents = documents;
            _getSelected = getSelected;
            _setSelected = setSelected;
        }

        public void Find(string searchText, bool allTabs)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            if (allTabs)
            {
                bool found = false;

                foreach (var doc in _documents)
                {
                    int index = -1;
                    if (doc.TextContent != null)
                    {
                        index = doc.TextContent.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                    }
                    if (index >= 0)
                    {
                        _setSelected(doc);
                        _lastSearchDoc = doc;
                        _lastFoundIndex = index;
                        if (SearchResultFound != null)
                        {
                            SearchResultFound(index, searchText.Length);
                        }
                        found = true;
                        break;
                    }
                }

                if (!found)
                    {
                        MessageBox.Show($"\"{searchText}\" not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
            }
            else
            {
                var selected = _getSelected();
                if (selected == null) return;
                int index;

                if (selected.TextContent != null)
                {
                    index = selected.TextContent.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    index = -1;
                }
                if (index >= 0)
                {
                    _lastSearchDoc = selected;
                    _lastFoundIndex = index;
                    if (SearchResultFound != null)
                    {
                        SearchResultFound(index, searchText.Length);
                    }
                }
                else
                {
                    MessageBox.Show($"\"{searchText}\" not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public event Action<int, int> SearchResultFound;

        public void Replace(string searchText, string replaceText, bool allTabs)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            string safeReplaceText = "";
            if (replaceText != null)
            {
                safeReplaceText = replaceText;
            }

            string pattern = $@"\b{Regex.Escape(searchText)}\b";

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            if (allTabs)
            {
                foreach (var doc in _documents)
                {
                    if (doc.TextContent == null) continue;

                    var match = regex.Match(doc.TextContent);

                    if (match.Success)
                    {
                        doc.TextContent = regex.Replace(doc.TextContent, safeReplaceText, 1);
                        break;
                    }
                }
            }
            else
            {
                var selected = _getSelected();
                if (selected == null) return;
                if (selected.TextContent == null) return;

                var match = regex.Match(selected.TextContent);

                if (match.Success)
                {
                    selected.TextContent = regex.Replace(selected.TextContent, safeReplaceText, 1);
                }
                else
                {
                    MessageBox.Show($"\"{searchText}\" not found.", "Replace", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public void ReplaceAll(string searchText, string replaceText, bool allTabs)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            string safeReplaceText = "";
            if (replaceText != null)
            {
                safeReplaceText = replaceText;
            }

            string pattern = $@"\b{Regex.Escape(searchText)}\b";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            if (allTabs)
            {
                foreach (var doc in _documents)
                {
                    if (doc.TextContent != null)
                    {
                        doc.TextContent = regex.Replace(doc.TextContent, safeReplaceText);
                    }
                }
            }
            else
            {
                var selected = _getSelected();
                if (selected == null) return;
                if (selected.TextContent == null) return;

                selected.TextContent = regex.Replace(selected.TextContent, safeReplaceText);
            }
        }
    }
}
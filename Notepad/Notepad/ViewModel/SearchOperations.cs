using System;
using System.Collections.ObjectModel;
using System.Windows;
using Notepad.Model;
using System.Linq;
using System.Text.RegularExpressions;

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
                foreach (var doc in _documents)
                {
                    int index = doc.TextContent?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1;
                    if (index >= 0)
                    {
                        _setSelected(doc);
                        _lastSearchDoc = doc;
                        _lastFoundIndex = index;
                        SearchResultFound?.Invoke(index, searchText.Length);
                        break;
                    }
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
                    SearchResultFound?.Invoke(index, searchText.Length); 
                }
                else
                {
                    MessageBox.Show($"\"{searchText}\" not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public void FindNext(string searchText, bool allTabs)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            if (searchText != _lastSearchText || _getSelected() != _lastSearchDoc)
            {
                _lastFoundIndex = -1;
                _lastSearchText = searchText;
                _lastSearchDoc = _getSelected();
            }

            if (allTabs)
            {
                var docs = _documents.ToList();
                int docStart;

                if (_lastSearchDoc != null)
                {
                    docStart = docs.IndexOf(_lastSearchDoc);
                }
                else
                {
                    docStart = 0;
                }

                for (int i = 0; i < docs.Count; i++)
                {
                    int docIdx = (docStart + i) % docs.Count;
                    var doc = docs[docIdx];
                    int startPos;

                    if (i == 0)
                    {
                        startPos = _lastFoundIndex + 1;
                    }
                    else
                    {
                        startPos = 0;
                    }
                    if (doc.TextContent == null) continue;

                    int idx = doc.TextContent.IndexOf(searchText, startPos, StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        _setSelected(doc);
                        _lastSearchDoc = doc;
                        _lastFoundIndex = idx;
                        SearchResultFound?.Invoke(idx, searchText.Length);
                        return;
                    }
                }
                MessageBox.Show($"\"{searchText}\" not found.", "Find Next", MessageBoxButton.OK, MessageBoxImage.Information);
                _lastFoundIndex = -1;
            }
            else
            {
                var selected = _getSelected();
                if (selected == null || selected.TextContent == null)
                {
                    return;
                }
                int startPos = _lastFoundIndex + 1;
                if (startPos >= selected.TextContent.Length) startPos = 0;

                int idx = selected.TextContent.IndexOf(searchText, startPos, StringComparison.OrdinalIgnoreCase);
                if (idx < 0 && startPos > 0) 
                    idx = selected.TextContent.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase);

                if (idx >= 0)
                {
                    _lastFoundIndex = idx;
                    _lastSearchDoc = selected;
                    SearchResultFound?.Invoke(idx, searchText.Length);
                }
                else
                {
                    MessageBox.Show($"\"{searchText}\" not found.", "Find Next", MessageBoxButton.OK, MessageBoxImage.Information);
                    _lastFoundIndex = -1;
                }
            }
        }

        public void FindPrevious(string searchText, bool allTabs)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            if (searchText != _lastSearchText || _getSelected() != _lastSearchDoc)
            {
                _lastFoundIndex = -1;
                _lastSearchText = searchText;
                _lastSearchDoc = _getSelected();
            }

            if (allTabs)
            {
                var docs = _documents.ToList();
                int docStart;

                if (_lastSearchDoc != null)
                {
                    docStart = docs.IndexOf(_lastSearchDoc);
                }
                else
                {
                    docStart = 0;
                }
                if (docStart < 0) docStart = 0;

                for (int i = 0; i < docs.Count; i++)
                {
                    int docIdx = ((docStart - i) % docs.Count + docs.Count) % docs.Count;
                    var doc = docs[docIdx];

                    if (string.IsNullOrEmpty(doc.TextContent)) continue;

                    int searchUpTo = (i == 0 && _lastFoundIndex != -1)
                                     ? _lastFoundIndex - 1
                                     : doc.TextContent.Length - 1;

                    if (searchUpTo >= 0 && searchUpTo < doc.TextContent.Length)
                    {
                        int idx = doc.TextContent.LastIndexOf(searchText, searchUpTo, StringComparison.OrdinalIgnoreCase);
                        if (idx >= 0)
                        {
                            _setSelected(doc);
                            _lastSearchDoc = doc;
                            _lastFoundIndex = idx;
                            SearchResultFound?.Invoke(idx, searchText.Length);
                            return;
                        }
                    }
                }

                MessageBox.Show($"\"{searchText}\" not found.", "Find Previous", MessageBoxButton.OK, MessageBoxImage.Information);
                _lastFoundIndex = -1;
            }
            else
            {
                var selected = _getSelected();
                if (string.IsNullOrEmpty(selected?.TextContent)) return;

                int searchUpTo;

                if (_lastFoundIndex > 0)
                {
                    searchUpTo = _lastFoundIndex - 1;
                }
                else
                {
                    searchUpTo = selected.TextContent.Length - 1;
                }
                int idx = searchUpTo >= 0 && searchUpTo < selected.TextContent.Length
                    ? selected.TextContent.LastIndexOf(searchText, searchUpTo, StringComparison.OrdinalIgnoreCase)
                    : -1;

                if (idx < 0 && _lastFoundIndex != selected.TextContent.Length - 1)
                    idx = selected.TextContent.LastIndexOf(searchText, StringComparison.OrdinalIgnoreCase);

                if (idx >= 0)
                {
                    _lastFoundIndex = idx;
                    _lastSearchDoc = selected;
                    SearchResultFound?.Invoke(idx, searchText.Length);
                }
                else
                {
                    MessageBox.Show($"\"{searchText}\" not found.", "Find Previous", MessageBoxButton.OK, MessageBoxImage.Information);
                    _lastFoundIndex = -1;
                }
            }
        }

        public event Action<int, int> SearchResultFound;

        public void Replace(string searchText, string replaceText, bool allTabs)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            string pattern = $@"\b{Regex.Escape(searchText)}\b";

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            if (allTabs)
            {
                foreach (var doc in _documents)
                {
                    if (doc.TextContent == null) continue;

                    doc.TextContent = regex.Replace(doc.TextContent, replaceText ?? "");
                }
            }
            else
            {
                var selected = _getSelected();
                if (selected?.TextContent == null) return;

                var match = regex.Match(selected.TextContent);

                if (match.Success)
                {
                    selected.TextContent = regex.Replace(selected.TextContent, replaceText ?? "", 1);
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

            string pattern = $@"\b{Regex.Escape(searchText)}\b";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase); 

            if (allTabs)
            {
                foreach (var doc in _documents)
                {
                    if (doc.TextContent != null)
                    {
                        doc.TextContent = regex.Replace(doc.TextContent, replaceText ?? "");
                    }
                }
            }
            else
            {
                var selected = _getSelected();
                if (selected?.TextContent == null) return;

                selected.TextContent = regex.Replace(selected.TextContent, replaceText ?? "");
            }
        }
    }
}
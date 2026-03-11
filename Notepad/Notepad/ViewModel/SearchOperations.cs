using System;
using System.Collections.ObjectModel;
using System.Windows;
using Notepad.Model;
using System.Linq;

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
                int index = selected.TextContent?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1;
                if (index >= 0)
                {
                    _lastSearchDoc = selected;
                    _lastFoundIndex = index;
                    SearchResultFound?.Invoke(index, searchText.Length); //
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
                int docStart = _lastSearchDoc != null ? docs.IndexOf(_lastSearchDoc) : 0;

                for (int i = 0; i < docs.Count; i++)
                {
                    int docIdx = (docStart + i) % docs.Count;
                    var doc = docs[docIdx];
                    int startPos = (i == 0) ? _lastFoundIndex + 1 : 0;
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
                if (selected?.TextContent == null) return;

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
                int docStart = _lastSearchDoc != null ? docs.IndexOf(_lastSearchDoc) : 0;

                for (int i = 0; i < docs.Count; i++)
                {
                    int docIdx = ((docStart - i) % docs.Count + docs.Count) % docs.Count;
                    var doc = docs[docIdx];
                    int searchUpTo = (i == 0 && _lastFoundIndex > 0) ? _lastFoundIndex - 1 : (doc.TextContent?.Length - 1 ?? 0);
                    if (doc.TextContent == null || searchUpTo < 0) continue;

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
                MessageBox.Show($"\"{searchText}\" not found.", "Find Previous", MessageBoxButton.OK, MessageBoxImage.Information);
                _lastFoundIndex = -1;
            }
            else
            {
                var selected = _getSelected();
                if (selected?.TextContent == null) return;

                int searchUpTo = (_lastFoundIndex > 0) ? _lastFoundIndex - 1 : selected.TextContent.Length - 1;

                int idx = searchUpTo >= 0
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

            string pattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(searchText)}\b";

            if (allTabs)
            {
                foreach (var doc in _documents)
                {
                    if (doc.TextContent == null) continue;
                    doc.TextContent = System.Text.RegularExpressions.Regex.Replace(
                        doc.TextContent, pattern, replaceText ?? "",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
            else
            {
                var selected = _getSelected();
                if (selected?.TextContent == null) return;

                var match = System.Text.RegularExpressions.Regex.Match(
                    selected.TextContent, pattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success)
                    selected.TextContent = selected.TextContent.Remove(match.Index, match.Length)
                                                               .Insert(match.Index, replaceText ?? "");
                else
                    MessageBox.Show($"\"{searchText}\" not found.", "Replace", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void ReplaceAll(string searchText, string replaceText, bool allTabs)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            string pattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(searchText)}\b";

            if (allTabs)
            {
                foreach (var doc in _documents)
                    if (doc.TextContent != null)
                        doc.TextContent = System.Text.RegularExpressions.Regex.Replace(
                            doc.TextContent, pattern, replaceText ?? "",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            else
            {
                var selected = _getSelected();
                if (selected?.TextContent == null) return;
                selected.TextContent = System.Text.RegularExpressions.Regex.Replace(
                    selected.TextContent, pattern, replaceText ?? "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
        }
    }
}
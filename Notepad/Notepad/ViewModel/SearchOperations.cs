using System;
using System.Collections.ObjectModel;
using System.Windows;
using Notepad.Model;

namespace Notepad.ViewModels
{
    public class SearchOperations
    {
        private readonly ObservableCollection<DocumentModel> _documents;
        private readonly Func<DocumentModel> _getSelected;
        private readonly Action<DocumentModel> _setSelected;

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
                    if (index >= 0) { _setSelected(doc); break; }
                }
            }
            else
            {
                var selected = _getSelected();
                if (selected == null) return;
                int index = selected.TextContent?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1;
                if (index < 0)
                    MessageBox.Show($"\"{searchText}\" not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void Replace(string searchText, string replaceText, bool allTabs)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            if (allTabs)
            {
                foreach (var doc in _documents)
                {
                    if (doc.TextContent == null) continue;
                    int index = doc.TextContent.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                        doc.TextContent = doc.TextContent.Remove(index, searchText.Length).Insert(index, replaceText ?? "");
                }
            }
            else
            {
                var selected = _getSelected();
                if (selected?.TextContent == null) return;
                int idx = selected.TextContent.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                    selected.TextContent = selected.TextContent.Remove(idx, searchText.Length).Insert(idx, replaceText ?? "");
                else
                    MessageBox.Show($"\"{searchText}\" not found.", "Replace", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void ReplaceAll(string searchText, string replaceText, bool allTabs)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            if (allTabs)
            {
                foreach (var doc in _documents)
                    if (doc.TextContent != null)
                        doc.TextContent = doc.TextContent.Replace(searchText, replaceText ?? "", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                var selected = _getSelected();
                if (selected?.TextContent == null) return;
                selected.TextContent = selected.TextContent.Replace(searchText, replaceText ?? "", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
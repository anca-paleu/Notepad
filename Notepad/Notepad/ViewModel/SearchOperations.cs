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
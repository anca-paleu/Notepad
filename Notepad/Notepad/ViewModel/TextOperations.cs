using System;
using System.Linq;
using System.Windows;
using Notepad.Model;

namespace Notepad.ViewModels
{
    public class TextOperations
    {
        private readonly Func<DocumentModel> _getSelected;
        private readonly Func<string> _getSelectedText;
        private readonly Func<int> _getSelectionStart;
        private string _internalClipboard;

        public bool HasInternalClipboard => !string.IsNullOrEmpty(_internalClipboard);
        public event Action<int> ScrollToLine;

        public TextOperations(Func<DocumentModel> getSelected, Func<string> getSelectedText, Func<int> getSelectionStart)
        {
            _getSelected = getSelected;
            _getSelectedText = getSelectedText;
            _getSelectionStart = getSelectionStart;
        }

        public void ToUpperCase()
        {
            var doc = _getSelected();
            if (doc?.TextContent == null) return;
            doc.TextContent = doc.TextContent.ToUpper();
        }

        public void ToLowerCase()
        {
            var doc = _getSelected();
            if (doc?.TextContent == null) return;
            doc.TextContent = doc.TextContent.ToLower();
        }

        public void RemoveEmptyLines()
        {
            var doc = _getSelected();
            if (doc?.TextContent == null) return;
            var lines = doc.TextContent.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l));
            doc.TextContent = string.Join("\n", lines);
        }

        public void GoToLine(int lineNumber)
        {
            var doc = _getSelected();
            if (doc?.TextContent == null) return;
            var lines = doc.TextContent.Split('\n');
            if (lineNumber < 1 || lineNumber > lines.Length)
            {
                MessageBox.Show($"Line {lineNumber} does not exist.", "Go To Line", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int charIndex = lines.Take(lineNumber - 1).Sum(l => l.Length + 1);
            ScrollToLine?.Invoke(charIndex);
        }

        public void ToggleReadOnly()
        {
            var doc = _getSelected();
            if (doc == null) return;
            doc.IsReadOnly = !doc.IsReadOnly;
        }

        public void Copy()
        {
            var text = _getSelectedText?.Invoke();
            if (!string.IsNullOrEmpty(text))
                _internalClipboard = text;
        }

        public void Cut()
        {
            var text = _getSelectedText?.Invoke();
            if (string.IsNullOrEmpty(text)) return;

            var doc = _getSelected();
            if (doc?.TextContent == null) return;

            int start = _getSelectionStart?.Invoke() ?? -1;
            if (start < 0 || start + text.Length > doc.TextContent.Length) return;

            _internalClipboard = text;
            doc.TextContent = doc.TextContent.Remove(start, text.Length);
        }

        public void Paste()
        {
            var doc = _getSelected();
            if (doc == null || string.IsNullOrEmpty(_internalClipboard)) return;
            doc.TextContent += _internalClipboard;
        }
    }
}
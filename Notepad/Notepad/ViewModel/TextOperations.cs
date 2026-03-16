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
            if (doc == null) return;
            if (doc.TextContent == null) return;

            string selectedText = null;
            if (_getSelectedText != null)
            {
                selectedText = _getSelectedText();
            }

            if (string.IsNullOrEmpty(selectedText)) return;

            int start;
            if (_getSelectionStart != null)
            {
                start = _getSelectionStart();
            }
            else
            {
                start = -1;
            }

            if (start < 0 || start + selectedText.Length > doc.TextContent.Length) return;

            doc.TextContent = doc.TextContent.Remove(start, selectedText.Length);
            doc.TextContent = doc.TextContent.Insert(start, selectedText.ToUpper());
        }

        public void ToLowerCase()
        {
            var doc = _getSelected();
            if (doc == null) return;
            if (doc.TextContent == null) return;

            string selectedText = null;
            if (_getSelectedText != null)
            {
                selectedText = _getSelectedText();
            }

            if (string.IsNullOrEmpty(selectedText)) return;

            int start;
            if (_getSelectionStart != null)
            {
                start = _getSelectionStart();
            }
            else
            {
                start = -1;
            }

            if (start < 0 || start + selectedText.Length > doc.TextContent.Length) return;

            doc.TextContent = doc.TextContent.Remove(start, selectedText.Length);
            doc.TextContent = doc.TextContent.Insert(start, selectedText.ToLower());
        }

        public void RemoveEmptyLines()
        {
            var doc = _getSelected();
            if (doc == null) return;
            if (doc.TextContent == null) return;

            var lines = doc.TextContent.Split(new[] { "\r\n" }, StringSplitOptions.None)
                                       .Where(l => !string.IsNullOrWhiteSpace(l));

            doc.TextContent = string.Join(Environment.NewLine, lines);
        }

        public void GoToLine(int lineNumber)
        {
            var doc = _getSelected();
            if (doc == null) return;
            if (doc.TextContent == null) return;

            var lines = doc.TextContent.Split(new[] { "\r\n"}, StringSplitOptions.None);

            if (lineNumber < 1 || lineNumber > lines.Length)
            {
                MessageBox.Show($"Line {lineNumber} does not exist.", "Go To Line", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int newLineLength = Environment.NewLine.Length;
            int charIndex = lines.Take(lineNumber - 1).Sum(l => l.Length + newLineLength);

            if (ScrollToLine != null)
            {
                ScrollToLine(charIndex);
            }
        }

        public void ToggleReadOnly()
        {
            var doc = _getSelected();
            if (doc == null) return;

            doc.IsReadOnly = !doc.IsReadOnly;
        }

        public void Copy()
        {
            string text = null;
            if (_getSelectedText != null)
            {
                text = _getSelectedText();
            }

            if (!string.IsNullOrEmpty(text))
            {
                _internalClipboard = text;
            }
        }

        public void Cut()
        {
            string text = null;
            if (_getSelectedText != null)
            {
                text = _getSelectedText();
            }

            if (string.IsNullOrEmpty(text)) return;

            var doc = _getSelected();
            if (doc == null) return;
            if (doc.TextContent == null) return;

            int start = -1;
            if (_getSelectionStart != null)
            {
                start = _getSelectionStart();
            }

            if (start < 0 || start + text.Length > doc.TextContent.Length) return;

            _internalClipboard = text;
            doc.TextContent = doc.TextContent.Remove(start, text.Length);
        }

        public void Paste()
        {
            var doc = _getSelected();
            if (doc == null) return;
            if (string.IsNullOrEmpty(_internalClipboard)) return;

            doc.TextContent += _internalClipboard;
        }
    }
}
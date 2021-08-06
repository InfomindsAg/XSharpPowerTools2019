using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace XSharpPowerTools.Helpers
{
    public static class DocumentHelper
    {
        public static DateTime EditorOpenedTimestamp;

        public static async Task OpenProjectItemAtAsync(string file, int lineNumber)
        {
            var editorWindow = await VS.Documents.IsOpenAsync(file)
                ? await VS.Windows.FindWindowAsync(file)
                : await VS.Documents.OpenViaProjectAsync(file) ?? await VS.Documents.OpenAsync(file);

            if (editorWindow == null)
            {
                await VS.MessageBox.ShowWarningAsync("X# Code Browser", "Failed to open file.");
                return;
            }

            await editorWindow.ShowAsync();

            var textView = await VS.Documents.GetTextViewAsync(file);
            var lineIndex = lineNumber > 0 ? lineNumber - 1 : 0;
            var line = textView.TextSnapshot.Lines.ElementAt(lineIndex);
            textView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(line.Start, line.End));
            textView.Caret.MoveTo(line.End);
            textView.VisualElement.Focus();
        }

        public static async Task InsertUsingAsync(string namespaceRef)
        {
            var textView = await VS.Documents.GetCurrentTextViewAsync();
            var usings = new List<ITextSnapshotLine>();

            foreach (var line in textView.TextSnapshot.Lines)
            {
                var lineText = line.GetText().Trim();
                if (lineText.StartsWith("using"))
                {
                    if (lineText.Split(' ').ElementAtOrDefault(1) == namespaceRef)
                        return;
                    usings.Add(line);
                }
                else
                {
                    break;
                }
            }

            ITextEdit edit;
            try
            {
                edit = textView.TextBuffer.CreateEdit();
            }
            catch (InvalidOperationException)
            {
                return;
            }

            try
            {
                var insertPos = usings.LastOrDefault()?.EndIncludingLineBreak.Position ?? 0;
                var paddingNum = usings.LastOrDefault()?.GetText().TakeWhile(char.IsWhiteSpace).Count();
                var padding = paddingNum != null ? new string(' ', (int)paddingNum) : string.Empty;
                edit.Insert(insertPos, padding + "using " + namespaceRef + Environment.NewLine);
                edit.Apply();
            }
            catch (Exception)
            {
                edit.Cancel();
            }
        }

        public static async Task<string> GetEditorSearchTermAsync()
        {
            var textView = await VS.Documents.GetCurrentTextViewAsync();
            return textView?.Selection == null
                ? string.Empty
                : !textView.Selection.IsEmpty ? textView.Selection.VirtualSelectedSpans[0].GetText() : string.Empty;
        }

        public static async Task<string> GetCurrentFileAsync()
        {
            var currentDocument = await VS.Documents.GetCurrentDocumentAsync();
            return currentDocument?.FilePath;
        }
    }
}

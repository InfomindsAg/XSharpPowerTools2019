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
            if (!System.IO.File.Exists(file))
                return;

            var editorWindow = await VS.Documents.IsOpenAsync(file)
                ? await VS.Windows.FindDocumentWindowAsync(file)
                : (await VS.Documents.OpenViaProjectAsync(file))?.WindowFrame ?? (await VS.Documents.OpenAsync(file))?.WindowFrame;

            if (editorWindow == null)
            {
                await VS.MessageBox.ShowWarningAsync("X# Code Browser", "Failed to open file.");
                return;
            }

            await editorWindow.ShowAsync();

            var textView = (await VS.Documents.GetDocumentViewAsync(file))?.TextView;
            var lineIndex = lineNumber > 0 ? lineNumber - 1 : 0;
            var line = textView.TextSnapshot.Lines.ElementAt(lineIndex);
            textView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(line.Start, line.End));
            textView.Caret.MoveTo(line.End);
            textView.VisualElement.Focus();
        }

        public static async Task InsertUsingAsync(string namespaceRef, XSModel xsModel)
        {
            var documentView = await VS.Documents.GetActiveDocumentViewAsync();
            var fileName = documentView?.FilePath;
            var textView = documentView?.TextView;
            var usings = new List<ITextSnapshotLine>();

            if (textView == null)
                return;

            if (await xsModel.FileContainsUsingAsync(fileName, namespaceRef))
            {
                await VS.MessageBox.ShowAsync("Using with given namespace already found in current document.");
                return;
            }

            foreach (var line in textView.TextSnapshot.Lines)
            {
                var lineText = line.GetText().Trim();
                if (lineText.StartsWith("using"))
                {
                    if (lineText.Split(' ').ElementAtOrDefault(1) == namespaceRef)
                    {
                        await VS.MessageBox.ShowAsync("Using with given namespace already found in current document.");
                        return;
                    }
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
            var textView = (await VS.Documents.GetActiveDocumentViewAsync())?.TextView;
            return textView?.Selection == null
                ? string.Empty
                : !textView.Selection.IsEmpty ? textView.Selection.VirtualSelectedSpans[0].GetText() : string.Empty;
        }

        public static async Task<string> GetCurrentFileAsync()
        {
            var currentDocument = await VS.Documents.GetActiveDocumentViewAsync();
            return currentDocument?.FilePath;
        }
    }
}

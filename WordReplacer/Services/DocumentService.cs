using DocumentFormat.OpenXml.Packaging;
using MatBlazor;
using Microsoft.JSInterop;
using WordReplacer.Models;
using WordReplacer.Utilities;

namespace WordReplacer.Services
{
    /// <summary>
    /// DocumentService Class Implementation
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// New DocumentService Instance.
        /// </summary>
        /// <param name="jsRuntime"></param>
        public DocumentService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <inheritdoc />
        public List<Dictionary<string, string>> GetAllCombinations(Dictionary<DocumentValue, DocumentValue> values)
        {
            List<Node> nodeList = Helper.DictionaryToNode(values);
            var combinationsResult = new List<Dictionary<string, string>>();
            DocumentHelper.GetCombinations(nodeList, 0, combinationsResult, new Dictionary<string, string>());
            return combinationsResult;
        }

        /// <inheritdoc />
        public async Task<MemoryStream> GetMemoryStream(IMatFileUploadEntry? file)
        {
            if (file is null)
            {
                throw new ArgumentException("Cannot get a memory stream from a null/empty file");
            }

            var stream = new MemoryStream();
            await file.WriteToStreamAsync(stream).ConfigureAwait(false);
            return stream;
        }

        /// <inheritdoc />
        public Stream Replace(Dictionary<string, string> values, MemoryStream streamFile)
        {
            if (streamFile is null)
            {
                throw new ArgumentException("Stream file is null");
            }

            var newFile = new MemoryStream();
                
            streamFile.Position = 0;
            streamFile.CopyTo(newFile);

            using var wordDoc = WordprocessingDocument.Open(newFile, true);
            wordDoc.ReplaceWordBodyText(values);
            wordDoc.ReplaceWordHeaderText(values);
            wordDoc.ReplaceWordFooterText(values);
            wordDoc.Close();
                
            return newFile;
        }

        /// <inheritdoc />
        public async Task DownloadFile(string filename, Stream? docReplaced)
        {
            if (docReplaced is null)
            {
                return;
            }

            await _jsRuntime
                  .InvokeVoidAsync("downloadFileFromStream", filename, docReplaced.ConvertToBase64())
                  .ConfigureAwait(false);
        }
    }
}
using DocumentFormat.OpenXml.Packaging;
using MatBlazor;
using Microsoft.JSInterop;
using WordReplacer.Common;
using WordReplacer.Models;

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
        public List<Dictionary<string, string>> GetAllCombinations(List<KeyValuePair<DocumentValue, DocumentValue>> values)
        {
            List<CombinationsNode> nodeList = values.DictionaryToNode();
            var combinationsResult = new List<Dictionary<string, string>>();
            combinationsResult.GetCombinations(nodeList, 0, new Dictionary<string, string>());
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
        public Stream Replace(Dictionary<string, string> values, MemoryStream streamFile, bool isReplaceMultipleWordsAtOnce)
        {
            if (streamFile is null)
            {
                throw new ArgumentException("Stream file is null");
            }

            var newFile = new MemoryStream();
                
            streamFile.Position = 0;
            streamFile.CopyTo(newFile);

            using var wordDoc = WordprocessingDocument.Open(newFile, true);

            // TODO: Check if key has more the one word. if not, use the ReplaceWordBodyText. If it has more than one use ReplaceStringInWordDocument
            if (isReplaceMultipleWordsAtOnce)
            {
                foreach (var words in values)
                {
                    wordDoc.ReplaceStringInWordDocument(words.Key, words.Value);
                }
            }
            else
            {
                // TODO: Make the foreach inside this method so will be able to validate each value to check where should be
                wordDoc.ReplaceWordBodyText(values);
                wordDoc.ReplaceWordHeaderText(values);
                wordDoc.ReplaceWordFooterText(values);
                wordDoc.Close();
            }

            return newFile;
        }

        /// <inheritdoc />
        public async Task DownloadFile(
            string filename, 
            Stream? docReplaced, 
            string mimiType)
        {
            if (docReplaced is null)
            {
                return;
            }

            await _jsRuntime
                  .InvokeVoidAsync("downloadFileFromStream", filename, docReplaced.ConvertToBase64(), mimiType)
                  .ConfigureAwait(false);
        }
    }
}
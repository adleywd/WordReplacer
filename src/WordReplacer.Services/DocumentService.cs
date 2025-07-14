using DocumentFormat.OpenXml.Packaging;
using Microsoft.JSInterop;
using WordReplacer.Common;
using WordReplacer.Dto;
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
            var nodeList = values.Select(inputTxt => 
                                new KeyValuePair<string, List<string>> (inputTxt.Key.Text!, 
                                inputTxt.Value.Text!.Split("\n")
                                    .Where(s => !string.IsNullOrWhiteSpace(s))
                                    .ToList())
                                ).ToList();

            var combinationsResult = new List<Dictionary<string, string>>();
            combinationsResult.GetCombinations(nodeList, 0, new Dictionary<string, string>());
            return combinationsResult;
        }

        /// <inheritdoc />
        public async Task<MemoryStream> GetMemoryStream(FileUploadDto file)
        {
            if (file?.Content == null)
            {
                throw new ArgumentException("Invalid file content");
            }

            return await Task.FromResult(new MemoryStream(file.Content)).ConfigureAwait(false);
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

            foreach (var words in values)
            {

                if (words.Key.HasOnlyOneWord())
                {
                    // This will replace word by word.
                    wordDoc.ReplaceWordBodyText(words);
                    wordDoc.ReplaceWordHeaderText(words);
                    wordDoc.ReplaceWordFooterText(words);
                }
                else
                {
                    // This will use the characters to compare, so it'll be used for multiple words/phrases
                    wordDoc.ReplaceMultipleWordsBodyText(words.Key, words.Value);
                    // TODO: Add replace for header and footer in phrases
                }
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
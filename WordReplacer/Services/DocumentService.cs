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
        public async Task<List<Task>> ReplaceAndDownloadAsync(Document document)
        {
            var tasks = new List<Task>();

            // Check if there is any value as a List to create multiple files
            if (document.DocumentValues.Any(d => d.Value.ShouldReplaceForEachLine))
            {
                List<Node> nodeList = Helper.DictionaryToNode(document.DocumentValues);

                var combinationsResult = new List<Dictionary<string, string>>();

                DocumentHelper.GetCombinations(nodeList, 0, combinationsResult, new Dictionary<string, string>());

                foreach (IDictionary<string, string> result in combinationsResult)
                {
                    tasks.Add(Task.Run(
                        async () =>
                        {
                            using var stream = new MemoryStream();
                            if (document.File is not null)
                            {
                                await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
                                Stream? docReplaced =
                                    DocumentHelper.Replace((Dictionary<string, string>)result, stream);
                                var fileName = string.Join("_", result.Values);
                                await DownloadFileAsync($"{fileName}.docx", docReplaced).ConfigureAwait(false);
                            }
                        }));
                }
            }
            // If not, just replace the single file and download it.
            else
            {
                tasks.Add(Task.Run(
                    async () =>
                    {
                        using var stream = new MemoryStream();
                        if (document.File is not null)
                        {
                            await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
                            Stream? docReplaced = DocumentHelper.Replace(
                                document.DocumentValues.ToDictionary(
                                    d => d.Key.Text!,
                                    d => d.Value.Text!)
                                , stream);
                            await DownloadFileAsync(document.File.Name, docReplaced).ConfigureAwait(false);
                        }
                    }));
            }

            return tasks;
        }

        /// <inheritdoc />
        public async Task DownloadFileAsync(string filename, Stream? docReplaced)
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
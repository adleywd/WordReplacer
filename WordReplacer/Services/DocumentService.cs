using DocumentFormat.OpenXml.Packaging;
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

        /// <summary>
        /// Get all combinations for a List of Nodes that contains Key and List of string Values
        /// </summary>
        /// <param name="branches">List of nodes</param>
        /// <param name="currentNodeIdx">The current node index</param>
        /// <param name="resultList">The result list with the dictionaries</param>
        /// <param name="currentDict">Used internally to populate the result list</param>
        private void GetCombinations(IList<Node> branches, int currentNodeIdx, ICollection<IDictionary<string, string>> resultList, IDictionary<string, string> currentDict)
        {
            Node currentNode = branches[currentNodeIdx];
            var isLastNode = currentNodeIdx == branches.Count - 1;
        
            foreach (var value in currentNode.Values)
            {
                // Since the same dictionary is used in the loops, sometimes the key will be already filled with older value.
                // To avoid the error of duplicated value inside a dictionary, the current key is removed.
                currentDict = currentDict.RemoveFromDictIfExists(currentNode.Key);
            
                var isLastValue = value == currentNode.Values.Last();
            
                // If LAST NODE but NOT LAST VALUE
                if (isLastNode && !isLastValue)
                {
                    currentDict.Add(currentNode.Key, value);
                    resultList.Add(new Dictionary<string, string>(currentDict));
                }

                // If is NOT the LAST NODE but it is the LAST VALUE
                if (!isLastNode && isLastValue)
                {
                    currentDict.Add(currentNode.Key, value);
                    GetCombinations(branches, currentNodeIdx + 1, resultList, currentDict );
                }
                // If is the LAST VALUE and LAST VALUE
                if (isLastNode && isLastValue)
                {
                    currentDict.Add(currentNode.Key, value);
                    resultList.Add(new Dictionary<string, string>(currentDict));
                }
                // if NOT LAST NODE and NOT LAST VALUE
                if (!isLastNode && !isLastValue)
                {
                    currentDict.Add(currentNode.Key, value);
                    GetCombinations(branches, currentNodeIdx + 1, resultList, currentDict );
                }
            }
        }

        public async Task<Stream?> ProcessFilesAsync(Document document)
        {
            // var documentValuesWithRepeatValuesList =
            //     document.DocumentValues.Where(d => d.Value.RepeatReplaceForEachLine);

            List<Node> nodeList = Helper.DictionaryToNode(document.DocumentValues);
            
            var combinationsResult = new List<IDictionary<string, string>>();
            
            GetCombinations(nodeList, 0, combinationsResult, new Dictionary<string, string>());

            foreach (IDictionary<string,string> result in combinationsResult)
            {
                using var stream = new MemoryStream();
                await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
                Stream? docReplaced = Replace((Dictionary<string,string>)result, stream);
                var fileName = string.Join("_",result.Values);
                await DownloadFileAsync($"{fileName}.docx", docReplaced).ConfigureAwait(false);
            }
            
            return new MemoryStream();
            
            //
            // var keyValueListDict = document.DocumentValues.ToDictionary(
            //     k => k.Key.Text,
            //     k =>
            //     {
            //         if (k.Value.RepeatReplaceForEachLine)
            //         {
            //             return k.Value.Text
            //                 .Split("\n")
            //                 .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            //         }
            //
            //         return new List<string>()
            //         {
            //             k.Value.Text ?? string.Empty
            //         };
            //     });
            //
            //
            // if (documentValuesWithRepeatValuesList.ToList().Count > 0)
            // {
            //     Dictionary<string, string> values = new();
            //     Dictionary<string, List<string>> repeatValues = new();
            //
            //     if (document.File is null)
            //     {
            //         return null;
            //     }
            //
            //     try
            //     {
            //         foreach (var repeatingValues in documentValuesWithRepeatValuesList)
            //         {
            //             repeatValues.Add(
            //                 repeatingValues.Key.Text,
            //                 repeatingValues.Value.Text
            //                                .Split("\n")
            //                                .Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
            //         }
            //
            //         foreach (var documentValue in document.DocumentValues)
            //         {
            //             values.Add(documentValue.Key.Text, documentValue.Value.Text);
            //         }
            //
            //
            //         foreach (var listValuesKey in repeatValues.Keys)
            //         {
            //             foreach (KeyValuePair<string, string> value in values)
            //             {
            //                 if (value.Key.Contains(listValuesKey))
            //                 {
            //                     var dict = new Dictionary<string, string>();
            //                     dict = values;
            //                     foreach (var repeatValue in repeatValues[listValuesKey])
            //                     {
            //                         dict[value.Key] = repeatValue;
            //                         using var stream = new MemoryStream();
            //                         await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
            //                         Stream? docReplaced = Replace(dict, stream);
            //                         await DownloadFileAsync($"{repeatValue}.docx", docReplaced).ConfigureAwait(false);
            //                     }
            //                 }
            //                 else
            //                 {
            //                     // using var stream = new MemoryStream();
            //                     // await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
            //                     // Stream? docReplaced = Replace(values, stream);
            //                     // await DownloadFileAsync(document.File.Name, docReplaced).ConfigureAwait(false);
            //                 }
            //             }
            //         }
            //
            //         return new MemoryStream();
            //         // using var stream = new MemoryStream();
            //         // await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
            //         //
            //         // Stream? docReplaced = Replace(document);
            //         //     
            //         // await DownloadFileAsync(document.File.Name, docReplaced).ConfigureAwait(false);
            //         //     
            //         // return docReplaced;
            //
            //         // using var stream = new MemoryStream();
            //         // await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
            //         // document.FileInMemoryStream = stream;
            //         // Stream? docReplaced = Replace(document);
            //         //     
            //         // await DownloadFileAsync(document.File.Name, docReplaced).ConfigureAwait(false);
            //         //     
            //         // return docReplaced;
            //     }
            //
            //     catch (Exception ex)
            //     {
            //         Console.WriteLine(ex.Message);
            //         throw;
            //     }
            // }
            // else
            // {
            //     using var stream = new MemoryStream();
            //     await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
            //     document.FileInMemoryStream = stream;
            //     Stream? docReplaced = Replace(document);
            //     await DownloadFileAsync(document.File.Name, docReplaced).ConfigureAwait(false);
            //     return new MemoryStream();
            // }
        }

        private async Task DownloadFileAsync(string filename, Stream? docReplaced)
        {
            if (docReplaced is null)
            {
                return;
            }

            await _jsRuntime
                  .InvokeVoidAsync("downloadFileFromStream", filename, docReplaced.ConvertToBase64())
                  .ConfigureAwait(false);
        }

        private static Stream? Replace(Document document)
        {
            try
            {
                if (document.FileInMemoryStream is not null)
                {
                    using (var wordDoc = WordprocessingDocument.Open(document.FileInMemoryStream, true))
                    {
                        DoReplaceText(wordDoc, document.DocumentValues);
                        wordDoc.Close();
                    }

                    return document.FileInMemoryStream;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static Stream? Replace(Dictionary<string, string> values, MemoryStream? streamFile)
        {
            try
            {
                if (streamFile is not null)
                {
                    using (var wordDoc = WordprocessingDocument.Open(streamFile, true))
                    {
                        DoReplaceText(wordDoc, values);
                        wordDoc.Close();
                    }

                    return streamFile;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static void DoReplaceText(WordprocessingDocument wordDoc,
            Dictionary<string, string> values)
        {
            if (wordDoc.MainDocumentPart is null)
            {
                return;
            }

            string docText;
            using (var sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
            {
                docText = sr.ReadToEnd();
            }

            foreach (KeyValuePair<string, string> value in values)
            {
                docText = Helper.ReplaceTextWithRegex(value.Key, docText, value.Value);
            }

            Stream wordStream = wordDoc.MainDocumentPart.GetStream(FileMode.Create);
            using (var sw = new StreamWriter(wordStream))
            {
                sw.Write(docText);
            }
        }

        private static void DoReplaceText(WordprocessingDocument wordDoc,
            Dictionary<DocumentValue, DocumentValue> values)
        {
            if (wordDoc.MainDocumentPart is null)
            {
                return;
            }

            string docText;
            using (var sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
            {
                docText = sr.ReadToEnd();
            }

            foreach (KeyValuePair<DocumentValue, DocumentValue> value in values)
            {
                docText = Helper.ReplaceTextWithRegex(value.Key.Text, docText, value.Value.Text);
            }

            Stream wordStream = wordDoc.MainDocumentPart.GetStream(FileMode.Create);
            using (var sw = new StreamWriter(wordStream))
            {
                sw.Write(docText);
            }
        }
    }
}
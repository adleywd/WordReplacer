using DocumentFormat.OpenXml.Packaging;
using Microsoft.JSInterop;
using WordReplacer.Models;
using WordReplacer.Utilities;

namespace WordReplacer.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IJSRuntime _jsRuntime;

        public DocumentService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<Stream?> ProcessFilesAsync(Document document)
        {
            var documentValuesWithRepeatValuesList =
                document.DocumentValues.Where(d => d.Value.RepeatReplaceForEachLine);

            if (documentValuesWithRepeatValuesList.ToList().Count > 0)
            {
                Dictionary<string, string> values = new();
                Dictionary<string, List<string>> repeatValues = new();

                if (document.File is null)
                {
                    return null;
                }

                try
                {
                    foreach (var repeatingValues in documentValuesWithRepeatValuesList)
                    {
                        repeatValues.Add(
                            repeatingValues.Key.Text,
                            repeatingValues.Value.Text
                                           .Split("\n")
                                           .Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                    }

                    foreach (var documentValue in document.DocumentValues)
                    {
                        values.Add(documentValue.Key.Text, documentValue.Value.Text);
                    }


                    foreach (var repeatingValuesKey in repeatValues.Keys)
                    {
                        foreach (KeyValuePair<string, string> value in values)
                        {
                            if (value.Key.Contains(repeatingValuesKey))
                            {
                                var dict = new Dictionary<string, string>();
                                dict = values;
                                foreach (var repeatValue in repeatValues[repeatingValuesKey])
                                {
                                    dict[value.Key] = repeatValue;
                                    using var stream = new MemoryStream();
                                    await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
                                    Stream? docReplaced = Replace(dict, stream);
                                    await DownloadFileAsync($"{repeatValue}.docx", docReplaced).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                // using var stream = new MemoryStream();
                                // await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
                                // Stream? docReplaced = Replace(values, stream);
                                // await DownloadFileAsync(document.File.Name, docReplaced).ConfigureAwait(false);
                            }
                        }
                    }

                    return new MemoryStream();
                    // using var stream = new MemoryStream();
                    // await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
                    //
                    // Stream? docReplaced = Replace(document);
                    //     
                    // await DownloadFileAsync(document.File.Name, docReplaced).ConfigureAwait(false);
                    //     
                    // return docReplaced;

                    // using var stream = new MemoryStream();
                    // await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
                    // document.FileInMemoryStream = stream;
                    // Stream? docReplaced = Replace(document);
                    //     
                    // await DownloadFileAsync(document.File.Name, docReplaced).ConfigureAwait(false);
                    //     
                    // return docReplaced;
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
            else
            {
                using var stream = new MemoryStream();
                await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
                document.FileInMemoryStream = stream;
                Stream? docReplaced = Replace(document);
                await DownloadFileAsync(document.File.Name, docReplaced).ConfigureAwait(false);
                return new MemoryStream();
            }
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
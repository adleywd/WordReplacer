using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.JSInterop;
using WordReplacer.Enums;
using WordReplacer.Models;
using WordReplacer.Utilities;

namespace WordReplacer.Services
{
    public class DocumentService : IDocumentService
    {
        public async Task<Stream?> Replace(Document document)
        {
            try
            {
                using var stream = new MemoryStream();
                if (document.File is not null)
                {
                    await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
                    document.FileInMemoryStream = stream;
                    
                    return PrepareReplace(document);
                }

                return null;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static Stream? PrepareReplace(Document document)
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
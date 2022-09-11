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
                    return Helper.Replace(document);
                }

                return null;
            }
            
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
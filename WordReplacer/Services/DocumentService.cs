using Microsoft.JSInterop;
using WordReplacer.Models;
using WordReplacer.Utilities;

namespace WordReplacer.Services
{
    public class DocumentService : IDocumentService
    {
        public async Task<Stream> Replace(Document document)
        {
            using var stream = new MemoryStream();
            await document.File.WriteToStreamAsync(stream).ConfigureAwait(false);
            document.FileInMemoryStream = stream;
            return Helper.Replace(document);
        }
    }
}
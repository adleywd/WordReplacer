using WordReplacer.Models;

namespace WordReplacer.Services
{
    public interface IDocumentService
    {
        public Task<Stream?> ProcessFilesAsync(Document document);
    }
}
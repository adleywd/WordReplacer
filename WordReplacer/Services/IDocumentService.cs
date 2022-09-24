using WordReplacer.Models;

namespace WordReplacer.Services
{
    /// <summary>
    /// Document Service Interface
    /// </summary>
    public interface IDocumentService
    {
         
        
        /// <summary>
        /// Replace the document with the provided document and download the updated document
        /// </summary>
        /// <param name="Document">The document to have the text replaced.</param>
        public Task<List<Task>>  ReplaceAndDownloadAsync(Document document);
    }
}
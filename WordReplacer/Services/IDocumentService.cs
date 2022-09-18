using WordReplacer.Models;

namespace WordReplacer.Services
{
    /// <summary>
    /// Document Service Interface
    /// </summary>
    public interface IDocumentService
    {
         
        /// <summary>
        /// It takes a document, and returns a list of tasks that will process the document
        /// </summary>
        /// <param name="Document">The document to process.</param>
        public Task<List<Task>>  ProcessFilesAsync(Document document);
    }
}
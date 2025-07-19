using MatBlazor;
using WordReplacer.Dto;
using WordReplacer.Models;

namespace WordReplacer.Services;

/// <summary>
/// Document Service Interface
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Given a dictionary of DocumentValue objects, return a list of dictionaries of string key/value pairs, where each
    /// dictionary represents a possible combination of Text element from the Key and Text element from the Value.
    /// </summary>
    /// <param name="values">A List of Keys and values of DocumentValue objects. The key represents the Old Value and the
    /// value represents the New Value.</param>
    public List<Dictionary<string, string>> GetAllCombinations(List<KeyValuePair<DocumentValue, DocumentValue>> values);

    /// <summary>
    /// Splits text based on delimiter type and returns a list of non-empty values
    /// </summary>
    /// <param name="text">The text to split</param>
    /// <param name="delimiter">The delimiter type</param>
    /// <param name="customDelimiter">Custom delimiter string if DelimiterType.Custom is used</param>
    /// <returns>List of split text values, excluding empty entries</returns>
    public List<string> SplitTextByDelimiter(string? text, DelimiterType delimiter, string? customDelimiter = null);

    /// <summary>
    /// This function returns a `MemoryStream` of the file uploaded by the user
    /// </summary>
    /// <param name="file">The file to get the memory stream for.</param>
    public Task<MemoryStream> GetMemoryStream(FileUploadDto file);

    /// <summary>
    /// It downloads a file using JS Invocation.
    /// </summary>
    /// <param name="filename">The name of the file to download.</param>
    /// <param name="docReplaced">The stream of the document that was replaced.</param>
    /// <param name="mimiType">The mimi type of the file.</param>
    public Task DownloadFile(string filename, Stream? docReplaced, string mimiType);

    /// <summary>
    /// It replaces the values in a word document as MemoryStream for the values in the dictionary.
    /// </summary>
    /// <param name="values">A dictionary of key/value pairs. The key is the old value to be replace, and the value
    /// is the value to replace it with.</param>
    /// <param name="streamFile">The file to be replaced.</param>
    /// <param name="isReplaceMultipleWordsAtOnce">True if should be replace whole phrases. False to replace only words</param>
    public Stream Replace(Dictionary<string, string> values, MemoryStream streamFile, bool isReplaceMultipleWordsAtOnce);
}
using MatBlazor;

namespace WordReplacer.Models;

public class Document
{
    public Dictionary<DocumentValue, DocumentValue> DocumentValues { get; set; } = new();
    public IMatFileUploadEntry? File { get; set; }
}
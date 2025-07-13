using MatBlazor;

namespace WordReplacer.Models;

public class Document
{
    public List<KeyValuePair<DocumentValue, DocumentValue>> DocumentValues { get; set; } = new ();
    public List<IMatFileUploadEntry> Files { get; } = new();
}
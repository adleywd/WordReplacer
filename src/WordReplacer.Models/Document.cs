using MatBlazor;

namespace WordReplacer.Models;

public class Document
{
    public List<KeyValuePair<DocumentValue, DocumentValue>> DocumentValues { get; set; } = new ();
    public IMatFileUploadEntry? File { get; set; }
}
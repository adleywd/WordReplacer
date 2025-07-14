using Microsoft.AspNetCore.Components.Forms;

namespace WordReplacer.Models;

public class Document
{
    public List<KeyValuePair<DocumentValue, DocumentValue>> DocumentValues { get; set; } = new ();
    public List<IBrowserFile> Files { get; } = new();
}
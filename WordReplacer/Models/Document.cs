namespace WordReplacer.Models;

public class Document
{
    public Dictionary<DocumentValue, DocumentValue> DocumentValues { get; set; } = new();
}
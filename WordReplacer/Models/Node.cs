namespace WordReplacer.Models;

public class Node
{
    // public string Key { get; set; }
    // public string? ParentKey { get; set; }
    // public string? ChildKey { get; set; }
    // public List<Node> SubNode { get; set; } = new ();
    // public bool IsFirstNode { get; set; }
    // public bool IsLastNode { get; set; }
    public string Key { get; set; }
    public List<string> Values { get; set; }
}
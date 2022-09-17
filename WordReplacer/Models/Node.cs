namespace WordReplacer.Models;

public class Node
{
    public Node(string key, List<string> value)
    {
        Key = key;
        Values = value;
    }

    public Node(){}
    
    public string Key { get; set; }
    public List<string> Values { get; set; }
}
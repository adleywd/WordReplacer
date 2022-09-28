namespace WordReplacer.WebApp.Models;

public class Node
{
    /// <summary>
    /// The node constructor that receive a key and a list of values
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Node(string key, List<string> value)
    {
        Key = key;
        Values = value;
    }

    public Node(){}

    public string Key { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new ();
}
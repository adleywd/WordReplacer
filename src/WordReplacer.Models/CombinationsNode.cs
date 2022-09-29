namespace WordReplacer.Models;

/// <summary>
/// The Combinations Node
/// </summary>
public class CombinationsNode
{
    /// <summary>
    /// The node constructor that receive a key and a list of values
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public CombinationsNode(string key, List<string> value)
    {
        Key = key;
        Values = value;
    }

    public CombinationsNode(){}

    public string Key { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new ();
}
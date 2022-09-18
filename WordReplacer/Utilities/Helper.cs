using System.Collections;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using WordReplacer.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WordReplacer.Utilities;

public static class Helper
{
    /// <summary>
    /// It replaces all the text that matches the regex pattern with the replacement text.
    /// </summary>
    /// <param name="regexPattern">The regex pattern to use to find the text to replace.</param>
    /// <param name="input">The string to search for matches.</param>
    /// <param name="replacement">The text to replace the matches with.</param>
    public static string ReplaceTextWithRegex(string? regexPattern, string? input, string? replacement)
    {
        // Check a better way to replace it, spaces do not work
        // This way breaks with < >, or : (because alters the xml)
        if (regexPattern is null || input is null || replacement is null)
        {
            return string.Empty;
        }

        var regexText = new Regex(regexPattern);
        return regexText.Replace(input, replacement);
    }

    /// <summary>
    /// If the key exists in the dictionary, remove it.
    /// </summary>
    /// <param name="dict">The dictionary to remove the key from.</param>
    /// <param name="key">The key to remove from the dictionary.</param>
    public static IDictionary<string, string> RemoveFromDictIfExists(this IDictionary<string, string> dict, string key)
    {
        if (dict.ContainsKey(key))
        {
            dict.Remove(key);
        }

        return dict;
    }

   

    /// <summary>
    /// It takes a dictionary of DocumentValues and returns a list of Nodes.
    /// </summary>
    /// <param name="dict">The dictionary to convert to a node.</param>
    public static List<Node> DictionaryToNode(Dictionary<DocumentValue, DocumentValue> dict)
    {
        var result = dict.Select(
                       d => new Node(
                           d.Key.Text,
                           d.Value.Text.Split("\n")
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .ToList())
                       )
                   .ToList();
        return result;
    }
    
        
}
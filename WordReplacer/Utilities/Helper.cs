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

    public static IDictionary<string, string> RemoveFromDictIfExists(this IDictionary<string, string> dict, string key)
    {
        if (dict.ContainsKey(key))
        {
            dict.Remove(key);
        }

        return dict;
    }

    /// <summary>
    /// Transform a Dictionary to
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    public static List<Node> ToNode(Dictionary<string, List<string>> dict)
    {
        return dict.Select(
                       keyValuePair => new Node(
                           keyValuePair.Key,
                           keyValuePair.Value))
                   .ToList();
    }
    
    /// <summary>
    /// TBA
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
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
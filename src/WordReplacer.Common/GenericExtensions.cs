using System.Text.RegularExpressions;
using WordReplacer.Models;

namespace WordReplacer.Common;

public static class Helper
{
    /// <summary>
    /// It replaces all the text that matches the regex pattern with the replacement text.
    /// </summary>
    /// <param name="text">The text to be searched and replaced.</param>
    /// <param name="regexPattern">The regex pattern to use to find the text to replace.</param>
    /// <param name="replacement">The text to replace the matched text with.</param>
    /// <param name="onlyReplacingWholeWord">If true, then the regex pattern will be modified to only match whole words.</param>
    public static string ReplaceTextWithRegex(
        this string text, 
        string? regexPattern, 
        string? replacement,
        bool onlyReplacingWholeWord,
        bool ignoreCaseSensitive)
    {
        // Check a better way to replace it, spaces do not work
        // This way breaks with < >, or : (because alters the xml)
        if (regexPattern is null || replacement is null)
        {
            return string.Empty;
        }

        if (onlyReplacingWholeWord)
        {
            regexPattern = $"\\b{regexPattern}\\b";
        }

        return ignoreCaseSensitive ? 
            Regex.Replace(text, regexPattern, replacement, RegexOptions.IgnoreCase) 
            : Regex.Replace(text, regexPattern, replacement);
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
    public static List<CombinationsNode> DictionaryToNode(this Dictionary<DocumentValue, DocumentValue> dict)
    {
        var result = dict.Select(
                             d => new CombinationsNode(
                                 d.Key.Text,
                                 d.Value.Text.Split("\n")
                                  .Where(s => !string.IsNullOrWhiteSpace(s))
                                  .ToList())
                         )
                         .ToList();
        return result;
    }
}
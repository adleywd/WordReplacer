using System.Text.RegularExpressions;
using WordReplacer.Models;

namespace WordReplacer.Common;

public static class GenericExtensions
{
    /// <summary>
    /// Given a list of nodes, a current node index, a result list, and a current dictionary, add to the result list all
    /// possible combinations of nodes that can be created by traversing the nodes in the list starting at the current node
    /// index, and using the current dictionary to store the values of the nodes that have been traversed
    /// </summary>
    /// <param name="nodes">The list of nodes(each KeyValuePair) to be combined.</param>
    /// <param name="currentNodeIdx">The index of the current node in the list of nodes.</param>
    /// <param name="resultList">The list of dictionaries that will be returned.</param>
    /// <param name="currentDict">This is the dictionary that will be added to the resultList.</param>
    public static void GetCombinations(
        this ICollection<Dictionary<string, string>> resultList,
        List<KeyValuePair<string, List<string>>> nodes,
        int currentNodeIdx,
        IDictionary<string, string> currentDict)
    {
        // CombinationsNode currentNode = nodes[currentNodeIdx];
        var currentNode = nodes[currentNodeIdx];
        var isLastNode = currentNodeIdx == nodes.Count - 1;

        foreach (var value in currentNode.Value)
        {
            // Since the same dictionary is used in the loops, sometimes the key will be already filled with older value.
            // To avoid the error of duplicated value inside a dictionary, the current key is removed.
            currentDict = currentDict.RemoveFromDictIfExists(currentNode.Key);

            var isLastValue = value == currentNode.Value.Last();

            // If LAST NODE but NOT LAST VALUE
            if (isLastNode && !isLastValue)
            {
                //AddOrReplace
                currentDict.Add(currentNode.Key, value);
                resultList.Add(new Dictionary<string, string>(currentDict));
            }

            // If is NOT the LAST NODE but it is the LAST VALUE
            if (!isLastNode && isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                GetCombinations(resultList, nodes, currentNodeIdx + 1, currentDict);
            }

            // If is the LAST NODE and LAST VALUE
            if (isLastNode && isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                resultList.Add(new Dictionary<string, string>(currentDict));
            }

            // if NOT LAST NODE and NOT LAST VALUE
            if (!isLastNode && !isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                GetCombinations(resultList, nodes, currentNodeIdx + 1, currentDict);
            }
        }
    }

    /// <summary>
    /// It replaces all the text that matches the regex pattern with the replacement text.
    /// </summary>
    /// <param name="text">The text to be searched and replaced.</param>
    /// <param name="regexPattern">The regex pattern to use to find the text to replace.</param>
    /// <param name="replacement">The text to replace the matched text with.</param>
    /// <param name="onlyReplacingWholeWord">If true, then the regex pattern will be modified to only match whole words.</param>
    /// <param name="ignoreCaseSensitive">If true, the search and replace will be case insensitive.</param>
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

        return ignoreCaseSensitive
            ? Regex.Replace(text, regexPattern, replacement, RegexOptions.IgnoreCase)
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
    /// It takes a document and replace all the empty strings to \n
    /// </summary>
    /// <param name="document">The document to sanitize.</param>
    public static void SanitizeValues(this List<KeyValuePair<DocumentValue, DocumentValue>> documentValues)
    {
        foreach (KeyValuePair<DocumentValue, DocumentValue> doc in documentValues
                                                                           .Where(d =>
                                                                               string.IsNullOrEmpty(d.Key.Text)
                                                                               || string.IsNullOrEmpty(d.Value.Text)))
        {
            doc.Key.Text = string.IsNullOrEmpty(doc.Key.Text) ? "\n" : doc.Key.Text;
            doc.Value.Text = string.IsNullOrEmpty(doc.Value.Text) ? "\n" : doc.Value.Text;
        }
    }

    /// <summary>
    /// It checks if the string has only one word.
    /// </summary>
    /// <param name="originalValue">The string to check.</param>
    public static bool HasOnlyOneWord(this string originalValue)
    {
        var regex = new Regex(@"^\b[a-zA-Z0-9_’]+\b$", RegexOptions.IgnoreCase);

        return regex.IsMatch(originalValue);
    }

    /// <summary>
    /// Sets the default value if null or empty.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    public static string SetDefaultIfNullOrEmpty(this string? text, string defaultValue)
    {
        return string.IsNullOrEmpty(text) ? defaultValue : text;
    }
}
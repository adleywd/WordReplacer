
using System.Text.RegularExpressions;

namespace WordReplacer.Utilities;

public static class DocumentExtensions
{
    /// <summary>
    /// It replaces all the text that matches the regex pattern with the replacement text.
    /// </summary>
    /// <param name="regexPattern">The regex pattern to use to find the text to replace.</param>
    /// <param name="input">The string to search for matches.</param>
    /// <param name="replacement">The text to replace the matches with.</param>
    public static string Replace(string? regexPattern, string? input, string? replacement)
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
}
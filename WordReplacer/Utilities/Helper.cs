using System.Collections;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using WordReplacer.Models;

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
}
using System.Collections;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using WordReplacer.Models;

namespace WordReplacer.Utilities;

public static class Helper
{

    public static Stream Replace(Document document)
    {
        try
        {
            if (document.FileInMemoryStream is not null)
            {
                using (WordprocessingDocument wordDoc =
                       WordprocessingDocument.Open(document.FileInMemoryStream, true))
                {
                    DoReplaceText(wordDoc, document.DocumentValues);
                    wordDoc.Close();
                }

                return document.FileInMemoryStream;

            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public static void DoReplaceText(WordprocessingDocument wordDoc, Dictionary<DocumentValue, DocumentValue> values)
    {
        string docText;
        using (var sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
        {
            docText = sr.ReadToEnd();
        }

        // Move to specif class, not a "helper"
        // Check a better way to replace it, spaces do not work
        // This way breaks with < >, or : (because alters the xml)
        foreach (var value in values)
        {
            var regexText = new Regex(value.Key.Text);
            docText = regexText.Replace(docText, value.Value.Text);
        }

        var wordStream = wordDoc.MainDocumentPart.GetStream(FileMode.Create);
        using (var sw = new StreamWriter(wordStream))
        {
            sw.Write(docText);
        }

    }

}
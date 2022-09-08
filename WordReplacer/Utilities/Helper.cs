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
                    DoReplace(wordDoc);
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

    public static void DoReplace(WordprocessingDocument wordDoc)
    {
        string docText;
        using (var sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
        {
            docText = sr.ReadToEnd();
        }

        var regexText = new Regex("Student’s Name");
        docText = regexText.Replace(docText, "Batatola");

        var wordStream = wordDoc.MainDocumentPart.GetStream(FileMode.Create);
        using (var sw = new StreamWriter(wordStream))
        {
            sw.Write(docText);
        }

    }

}
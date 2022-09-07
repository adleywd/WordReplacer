using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;

namespace WordReplacer.Utilities;

public static class Helper
{
    public static void Replace()
    {
        var folder = "C:\\testfiles\\";
        var saveLocation = @"C:\\testfiles\\output";
        var filename = Path.Join(folder, "testDoc3.docx");
        var outputfile = Path.Join(saveLocation, "testeOOXML.docx");
        
        using var originalDoc = WordprocessingDocument.Open(filename, false);
        
        // Clone the opened WordprocessingDocument.
        using var newDoc = (WordprocessingDocument)originalDoc.Clone(outputfile, true);

        string docText = null;
        using (var sr = new StreamReader(newDoc.MainDocumentPart.GetStream()))
        {
            docText = sr.ReadToEnd();
        }

        var regexText = new Regex("Student’s Name");
        docText = regexText.Replace(docText, "TestePerson1");

        using (var sw = new StreamWriter(newDoc.MainDocumentPart.GetStream(FileMode.Create)))
        {
            sw.Write(docText);
        }
    }
}
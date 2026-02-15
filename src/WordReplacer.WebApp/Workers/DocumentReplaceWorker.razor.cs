using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using WordReplacer.Common;

namespace WordReplacer.WebApp.Workers;

[SupportedOSPlatform("browser")]
public partial class DocumentReplaceWorker
{
    [JSExport]
    internal static byte[] Replace(byte[] fileBytes, string replacementsJson)
    {
        var replacements = JsonSerializer.Deserialize<Dictionary<string, string>>(replacementsJson)!;

        using var inputStream = new MemoryStream(fileBytes);
        var outputStream = new MemoryStream();
        inputStream.CopyTo(outputStream);

        using var wordDoc = WordprocessingDocument.Open(outputStream, true);
        foreach (var words in replacements)
        {
            if (words.Key.HasOnlyOneWord())
            {
                wordDoc.ReplaceWordBodyText(words);
                wordDoc.ReplaceWordHeaderText(words);
                wordDoc.ReplaceWordFooterText(words);
            }
            else
            {
                wordDoc.ReplaceMultipleWordsBodyText(words.Key, words.Value);
            }
        }
        wordDoc.Dispose();

        return outputStream.ToArray();
    }
}

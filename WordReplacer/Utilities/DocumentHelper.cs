using DocumentFormat.OpenXml.Packaging;
using WordReplacer.Models;

namespace WordReplacer.Utilities;

/// A class that contains methods that help us to replace the text in a Word document.
public static class DocumentHelper
{
    /// <summary>
    /// Given a list of nodes, a current node index, a result list, and a current dictionary, add to the result list all
    /// possible combinations of nodes that can be created by traversing the nodes in the list starting at the current node
    /// index, and using the current dictionary to store the values of the nodes that have been traversed
    /// </summary>
    /// <param name="nodes">The list of nodes to be combined.</param>
    /// <param name="currentNodeIdx">The index of the current node in the list of nodes.</param>
    /// <param name="resultList">The list of dictionaries that will be returned.</param>
    /// <param name="currentDict">This is the dictionary that will be added to the resultList.</param>
    /// <summary>
    public static void GetCombinations(IList<Node> nodes, int currentNodeIdx,
        ICollection<IDictionary<string, string>> resultList, IDictionary<string, string> currentDict)
    {
        Node currentNode = nodes[currentNodeIdx];
        var isLastNode = currentNodeIdx == nodes.Count - 1;

        foreach (var value in currentNode.Values)
        {
            // Since the same dictionary is used in the loops, sometimes the key will be already filled with older value.
            // To avoid the error of duplicated value inside a dictionary, the current key is removed.
            currentDict = currentDict.RemoveFromDictIfExists(currentNode.Key);

            var isLastValue = value == currentNode.Values.Last();

            // If LAST NODE but NOT LAST VALUE
            if (isLastNode && !isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                resultList.Add(new Dictionary<string, string>(currentDict));
            }

            // If is NOT the LAST NODE but it is the LAST VALUE
            if (!isLastNode && isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                GetCombinations(nodes, currentNodeIdx + 1, resultList, currentDict);
            }

            // If is the LAST VALUE and LAST VALUE
            if (isLastNode && isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                resultList.Add(new Dictionary<string, string>(currentDict));
            }

            // if NOT LAST NODE and NOT LAST VALUE
            if (!isLastNode && !isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                GetCombinations(nodes, currentNodeIdx + 1, resultList, currentDict);
            }
        }
    }

    /// <summary>
    /// It replaces the document text.
    /// </summary>
    /// <param name="Document">The document to replace the text in.</param>
    public static Stream? Replace(Document document)
    {
        try
        {
            if (document.FileInMemoryStream is not null)
            {
                using (var wordDoc = WordprocessingDocument.Open(document.FileInMemoryStream, true))
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

    /// <summary>
    /// It replaces the values in the dictionary.
    /// </summary>
    /// <param name="values">A dictionary of key/value pairs. The key is the name of the variable to replace, and the value
    /// is the value to replace it with.</param>
    /// <param name="streamFile">The file to be replaced.</param>
    public static Stream? Replace(Dictionary<string, string> values, MemoryStream? streamFile)
    {
        try
        {
            if (streamFile is not null)
            {
                using (var wordDoc = WordprocessingDocument.Open(streamFile, true))
                {
                    DoReplaceText(wordDoc, values);
                    wordDoc.Close();
                }

                return streamFile;
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

    
    /// <summary>
    /// It replaces the text in the word document with the values in the dictionary.
    /// </summary>
    /// <param name="WordprocessingDocument">This is the document that we're going to be working with.</param>
    /// <param name="values">A dictionary of key/value pairs. The key is the text to be replaced, and the value is the
    /// replacement text.</param>
    private static void DoReplaceText(WordprocessingDocument wordDoc,
        Dictionary<string, string> values)
    {
        if (wordDoc.MainDocumentPart is null)
        {
            return;
        }

        string docText;
        using (var sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
        {
            docText = sr.ReadToEnd();
        }

        foreach (KeyValuePair<string, string> value in values)
        {
            docText = Helper.ReplaceTextWithRegex(value.Key, docText, value.Value);
        }

        Stream wordStream = wordDoc.MainDocumentPart.GetStream(FileMode.Create);
        using (var sw = new StreamWriter(wordStream))
        {
            sw.Write(docText);
        }
    }

    
    /// <summary>
    /// It takes a Word document and a dictionary of values to replace, and replaces the values in the document
    /// </summary>
    /// <param name="WordprocessingDocument">The Word document you want to replace text in.</param>
    /// <param name="values">A dictionary of key/value pairs. The key is the text to be replaced, and the value is the text
    /// to replace it with.</param>
    private static void DoReplaceText(WordprocessingDocument wordDoc,
        Dictionary<DocumentValue, DocumentValue> values)
    {
        if (wordDoc.MainDocumentPart is null)
        {
            return;
        }

        string docText;
        using (var sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
        {
            docText = sr.ReadToEnd();
        }

        foreach (KeyValuePair<DocumentValue, DocumentValue> value in values)
        {
            docText = Helper.ReplaceTextWithRegex(value.Key.Text, docText, value.Value.Text);
        }

        Stream wordStream = wordDoc.MainDocumentPart.GetStream(FileMode.Create);
        using (var sw = new StreamWriter(wordStream))
        {
            sw.Write(docText);
        }
    }
}
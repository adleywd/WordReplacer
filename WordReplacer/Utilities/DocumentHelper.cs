﻿using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using WordReplacer.Models;
using Document = WordReplacer.Models.Document;

namespace WordReplacer.Utilities;

/// A class that contains methods that help us to replace the text in a Word document.
public static class DocumentHelper
{
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
                    // ReplaceWordBodyText(wordDoc, values);
                    // ReplaceWordHeaderText(wordDoc, values);
                    // ReplaceWordFooterText(wordDoc, values);
                    ReplaceTextWithRegex(wordDoc, values);
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
    private static void ReplaceTextWithRegex(WordprocessingDocument wordDoc,
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
    /// Replaces the body text from Word file.
    /// </summary>
    /// <param name="doc">The document.</param>
    /// <param name="replaceWords">The replace words.</param>
    private static void ReplaceWordBodyText(WordprocessingDocument doc, Dictionary<string, string> replaceWords)
    {
        var body = doc.MainDocumentPart?.Document.Body;

        if (body is null)
        {
            return;
        }

        foreach (var text in body.Descendants<Text>())
        {
            foreach (var words in replaceWords)
            {
                if (text.Text.Contains(words.Key))
                {
                    text.Text = text.Text.ReplaceTextWithRegex(words.Key, words.Value, true, false);
                }
            }
        }
    }

    private static void ReplaceWordHeaderText(WordprocessingDocument doc, Dictionary<string, string> replaceWords)
    {
        IEnumerable<HeaderPart>? headers = doc.MainDocumentPart?.HeaderParts;
        if (headers is null)
        {
            return;
        }

        foreach (var headerPart in headers)
        {
            if (headerPart.RootElement is not null)
            {
                foreach (var text in headerPart.RootElement.Descendants<Text>())
                {
                    foreach (var words in replaceWords)
                    {
                        if (text.Text.Contains(words.Key))
                        {
                            text.Text = text.Text.ReplaceTextWithRegex(words.Key, words.Value, true, false);
                        }
                    }
                }
            }
        }
    }

    private static void ReplaceWordFooterText(WordprocessingDocument doc, Dictionary<string, string> replaceWords)
    {
        IEnumerable<FooterPart>? footer = doc.MainDocumentPart?.FooterParts;

        if (footer is null)
        {
            return;
        }

        foreach (var headerPart in footer)
        {
            if (headerPart.RootElement is not null)
            {
                foreach (var text in headerPart.RootElement.Descendants<Text>())
                {
                    foreach (var words in replaceWords)
                    {
                        if (text.Text.Contains(words.Key))
                        {
                            text.Text = text.Text.ReplaceTextWithRegex(words.Key, words.Value, true, false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Given a list of nodes, a current node index, a result list, and a current dictionary, add to the result list all
    /// possible combinations of nodes that can be created by traversing the nodes in the list starting at the current node
    /// index, and using the current dictionary to store the values of the nodes that have been traversed
    /// </summary>
    /// <param name="nodes">The list of nodes to be combined.</param>
    /// <param name="currentNodeIdx">The index of the current node in the list of nodes.</param>
    /// <param name="resultList">The list of dictionaries that will be returned.</param>
    /// <param name="currentDict">This is the dictionary that will be added to the resultList.</param>
    public static void GetCombinations(
        IList<Node> nodes,
        int currentNodeIdx,
        ICollection<Dictionary<string, string>> resultList,
        IDictionary<string, string> currentDict)
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
                //AddOrReplace
                currentDict.Add(currentNode.Key, value);
                resultList.Add(new Dictionary<string, string>(currentDict));
            }

            // If is NOT the LAST NODE but it is the LAST VALUE
            if (!isLastNode && isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                GetCombinations(nodes, currentNodeIdx + 1, resultList, currentDict);
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
                GetCombinations(nodes, currentNodeIdx + 1, resultList, currentDict);
            }
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
}
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using WordReplacer.Models;

namespace WordReplacer.Common;

/// A class that contains methods that help us to replace the text in a Word document.
public static class DocumentHelper
{
    /// <summary>
    /// Replaces the body text from Word file.
    /// </summary>
    /// <param name="doc">The document.</param>
    /// <param name="replaceWords">The replace words.</param>
    public static void ReplaceWordBodyText(this WordprocessingDocument doc, Dictionary<string, string> replaceWords)
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

    public static void ReplaceWordHeaderText(this WordprocessingDocument doc, Dictionary<string, string> replaceWords)
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

    public static void ReplaceWordFooterText(this WordprocessingDocument doc, Dictionary<string, string> replaceWords)
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
        this ICollection<Dictionary<string, string>> resultList,
        IList<CombinationsNode> nodes,
        int currentNodeIdx,
        IDictionary<string, string> currentDict)
    {
        CombinationsNode currentNode = nodes[currentNodeIdx];
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
    /// It replaces all instances of a string in a Word document with another string.
    /// </summary>
    /// <param name="wordProcessingDocument">The WordProcessingDocument object that you want to replace text in.</param>
    /// <param name="originalValue">The string you want to replace.</param>
    /// <param name="newerValue">The string you want to replace the replaceWhat string with.</param>
    /// <param name="matchExactly">True for the words needs to match exactly to be replaced.</param>
    public static void ReplaceStringInWordDocument(
        this WordprocessingDocument wordProcessingDocument, 
        string originalValue, 
        string newerValue, 
        bool matchExactly = false)
    {
        List<WordMatchedPhrase> matchedPhrases = FindWordMatchedPhrases(wordProcessingDocument, originalValue, matchExactly);

        var document = wordProcessingDocument.MainDocumentPart!.Document;
        var currentDocTextIndex = 0;
        var isInPhrase = false;
        var isInEndOfPhrase = false;
        foreach (Text text in document.Descendants<Text>()) // <<< Here
        {
            var textChars = text.Text.ToCharArray();
            List<WordMatchedPhrase> curParPhrases = matchedPhrases.FindAll(a => (a.FirstCharParOccurrence.Equals(currentDocTextIndex) || a.LastCharParOccurrence.Equals(currentDocTextIndex)));
            StringBuilder outStringBuilder = new StringBuilder();

            for (var c = 0; c < textChars.Length; c++)
            {
                if (isInEndOfPhrase)
                {
                    isInPhrase = false;
                    isInEndOfPhrase = false;
                }

                foreach (var parPhrase in curParPhrases)
                {
                    if (c == parPhrase.CharStartInFirstPar && currentDocTextIndex == parPhrase.FirstCharParOccurrence)
                    {
                        outStringBuilder.Append(newerValue);
                        isInPhrase = true;
                    }
                    if (c == parPhrase.CharEndInLastPar && currentDocTextIndex == parPhrase.LastCharParOccurrence)
                    {
                        isInEndOfPhrase = true;
                    }

                }
                if (isInPhrase == false && isInEndOfPhrase == false)
                {
                    outStringBuilder.Append(textChars[c]);
                }
            }
            text.Text = outStringBuilder.ToString();
            currentDocTextIndex++;
        }
    }

    private static List<WordMatchedPhrase> FindWordMatchedPhrases(WordprocessingDocument wordProcessingDocument, string originalValue, bool matchExactly)
    {
        // TODO: CHANGE EMPTY CHARACTER TO A FUNCTION isValidForExactlyMatch That will check for characters in ASCII (see notepad++)
        // TODO: Validate for nextChar for incoming iterations, ex: next will be null Exception, so still need checking
        const char emptyCharacter = (char)32;
        char previousChar = emptyCharacter;
        char nextChar = emptyCharacter;
        char[] replaceWhatChars = originalValue.ToCharArray();
        int overlapsRequired = replaceWhatChars.Length;
        var currentOriginalCharIndex = 0;
        var firstCharParOccurrence = 0;
        //int lastCharParOccurrence = 0;
        var startChar = 0;
        //int endChar = 0;
        var wordMatchedPhrases = new List<WordMatchedPhrase>();
        var document = wordProcessingDocument.MainDocumentPart!.Document;
        var currentDocTextIndex = 0;
        foreach (Text text in document.Descendants<Text>())
        {
            var textChars = text.Text.ToCharArray();
            // If previous char is empty, stay empty (with space). Otherwise it will keep the previous char
            previousChar = previousChar == emptyCharacter ? emptyCharacter : previousChar;

            for (var c = 0; c < textChars.Length; c++)
            {
                char compareToChar = replaceWhatChars[currentOriginalCharIndex];

                if (c-1 >= 0)
                {
                    previousChar = textChars[c - 1];
                }

                // If next char is possible to get, get it
                if (c + 1 < textChars.Length)
                {
                    nextChar = textChars[c + 1];
                }
                // If not possible, assume as empty
                else if (c + 1 > textChars.Length)
                {
                    nextChar = emptyCharacter;
                    //bool needsValidationInTheNextRun = true;
                }

                if (textChars[c] == compareToChar)
                {
                    // Check for match Exactly the word
                    if (matchExactly)
                    {
                        if (currentOriginalCharIndex == 0 && previousChar != emptyCharacter)
                        {
                            continue;
                        }

                        if (currentOriginalCharIndex == overlapsRequired && nextChar != emptyCharacter)
                        {
                            currentDocTextIndex = 0;
                            continue;
                        }
                    }

                    currentOriginalCharIndex++;

                    if (currentOriginalCharIndex == 1 ) 
                    {
                        startChar = c;
                        firstCharParOccurrence = currentDocTextIndex;
                    }
                    if (currentOriginalCharIndex == overlapsRequired)
                    {
                        var endChar = c;

                        var lastCharParOccurrence = currentDocTextIndex;

                        var matchedPhrase = new WordMatchedPhrase
                        {
                            FirstCharParOccurrence = firstCharParOccurrence,
                            LastCharParOccurrence = lastCharParOccurrence,
                            CharEndInLastPar = endChar,
                            CharStartInFirstPar = startChar
                        };

                        wordMatchedPhrases.Add(matchedPhrase);

                        currentOriginalCharIndex = 0;
                    }
                }
                else
                {
                    currentOriginalCharIndex = 0;
                }

                // If is the last iteration so the next Text iteration will have the previous char from previous Text iteration
                if (c + 1 == textChars.Length)
                {
                    previousChar = textChars[c];
                }
            }
            currentDocTextIndex++;
        }

        return wordMatchedPhrases;
    }


    private class WordMatchedPhrase
    {
        public int CharStartInFirstPar { get; set; }
        public int CharEndInLastPar { get; set; }
        public int FirstCharParOccurrence { get; set; }
        public int LastCharParOccurrence { get; set; }
    }
}
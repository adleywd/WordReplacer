using System.Text;
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
    /// It replaces all instances of a string in a Word document with another string.
    /// </summary>
    /// <param name="wordProcessingDocument">The WordProcessingDocument object that you want to replace text in.</param>
    /// <param name="originalValue">The string you want to replace.</param>
    /// <param name="newerValue">The string you want to replace the replaceWhat string with.</param>
    public static void ReplaceStringInWordDocument(
        this WordprocessingDocument wordProcessingDocument,
        string originalValue,
        string newerValue)
    {
        List<WordMatchedPhrase> matchedPhrases = FindWordMatchedPhrases(wordProcessingDocument, originalValue);

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

    /// <param name="matchExactly">Current NOT WORKING. True for the words needs to match exactly to be replaced.</param>
    private static List<WordMatchedPhrase> FindWordMatchedPhrases(WordprocessingDocument wordProcessingDocument, string originalValue, bool matchExactly = false)
    {
        // TODO: Match exactly not work properly because sometimes the previous char still the last one matched.
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
        List<Text> texts = document.Descendants<Text>().ToList();
        for (var i = 0; i < texts.Count; i++)
        {
            var textChars = texts[i].Text.ToCharArray();

            previousChar = previousChar == emptyCharacter ? emptyCharacter : previousChar;

            for (var c = 0; c < textChars.Length; c++)
            {
                char compareToChar = replaceWhatChars[currentOriginalCharIndex];

                if (c - 1 >= 0)
                {
                    previousChar = textChars[c - 1];
                }

                // If next char is possible to get, get it
                if (c + 1 < textChars.Length)
                {
                    nextChar = textChars[c + 1];
                }
                // If not possible, assume as empty
                else if (c + 1 >= textChars.Length)
                {
                    if (i == texts.Count - 1)
                    {
                        // If the last run of all texts
                        nextChar = emptyCharacter;
                    }
                    else
                    {
                        // Find the text in the next text obj.
                        nextChar = texts[i + 1].Text.ToCharArray().FirstOrDefault(emptyCharacter);
                    }
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

                        // If last character is not empty, it will not replace
                        if (currentOriginalCharIndex == overlapsRequired - 1 && nextChar != emptyCharacter)
                        {
                            currentDocTextIndex = 0;
                            continue;
                        }
                    }

                    currentOriginalCharIndex++;

                    if (currentOriginalCharIndex == 1)
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
        public int CharStartInFirstPar { get; init; }
        public int CharEndInLastPar { get; init; }
        public int FirstCharParOccurrence { get; init; }
        public int LastCharParOccurrence { get; init; }
    }
}
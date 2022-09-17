using System.Security.Cryptography.X509Certificates;
using DocumentFormat.OpenXml.Spreadsheet;
using WordReplacer.Models;

namespace WordReplacer.Tests.Services;

public class DocumentServiceTest
{
    [Fact]
    private void Test2()
    {
        List<Node> nodes = new()
        {
            new Node() { Key = "Aluno", Values = new List<string>() { "A1", "A2", "A3" } },
            new Node() { Key = "Escola", Values = new List<string>() { "E1", "E2", "E3"} },
            new Node() { Key = "Data", Values = new List<string>() { "D1", "D2" } }
        };

        var result = new List<Dictionary<string, string>>();

        GetCombinations(nodes, 0, result, new Dictionary<string, string>());
    }

    public void GetCombinations(List<Node> branches, int currentNodeIdx, List<Dictionary<string, string>> result,
        Dictionary<string, string> currentDict)
    {
        // var currentDict = new Dictionary<string, string>(branchDictResult);
        Node currentNode = branches[currentNodeIdx];
        var isLastNode = currentNodeIdx == branches.Count - 1;

        foreach (var value in currentNode.Values)
        {
            // Since the same dictionary is used in the loops, sometimes the key will be already filled with older value.
            // To avoid the error of duplicated value inside a dictionary, the current key is removed.
            RemoveDictKey(currentNode.Key, currentDict);

            var isLastValue = value == currentNode.Values.Last();

            // If LAST NODE but NOT LAST VALUE
            if (isLastNode && !isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                result.Add(new Dictionary<string, string>(currentDict));
            }

            // If is NOT the LAST NODE but it is the LAST VALUE
            if (!isLastNode && isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                GetCombinations(branches, currentNodeIdx + 1, result, currentDict);
            }

            // If is the LAST VALUE and LAST VALUE
            if (isLastNode && isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                result.Add(new Dictionary<string, string>(currentDict));
            }

            // if NOT LAST NODE and NOT LAST VALUE
            if (!isLastNode && !isLastValue)
            {
                currentDict.Add(currentNode.Key, value);
                GetCombinations(branches, currentNodeIdx + 1, result, currentDict);
            }
        }
    }

    private static void RemoveDictKey(string key, IDictionary<string, string> dict)
    {
        if (dict.ContainsKey(key))
        {
            dict.Remove(key);
        }
    }
}   
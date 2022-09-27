using System.Text.Json;
using WordReplacer.Models;
using WordReplacer.Utilities;

namespace WordReplacer.Tests.Services;

public class DocumentServiceTest
{
    [Fact]
    private void GetCombinations_WithValidNodeList_ShouldReturnListWithAllPossibleCombinationsInDict()
    {
        // Arrange
        var expectedListResult = new List<IDictionary<string, string>>
        {
            new Dictionary<string, string>
            {
                { "Student", "A1" }, { "School", "E1" }, { "Date", "D1" }
            },
            new Dictionary<string, string>
            {
                { "Student", "A1" }, { "School", "E1" }, { "Date", "D2" }
            },
            new Dictionary<string, string>
            {
                { "Student", "A2" }, { "School", "E1" }, { "Date", "D1" }
            },
            new Dictionary<string, string>
            {
                { "Student", "A2" }, { "School", "E1" }, { "Date", "D2" }
            }
        };

        List<Node> nodes = new()
        {
            new Node() { Key = "Student", Values = new List<string>() { "A1", "A2" } },
            new Node() { Key = "School", Values = new List<string>() { "E1" } },
            new Node() { Key = "Date", Values = new List<string>() { "D1", "D2" } }
        };

        var result = new List<Dictionary<string, string>>();

        // Act
        DocumentHelper.GetCombinations(nodes, 0, result, new Dictionary<string, string>());

        // Assert
        var serializedResult = JsonSerializer.Serialize(result);
        var serializedExpectedResult = JsonSerializer.Serialize(expectedListResult);
        serializedResult.ShouldBe(serializedExpectedResult);
        // CompareListDictionaries(result, expectedListResult).ShouldBeTrue();
    }
    
}
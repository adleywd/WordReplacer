using System.Text.Json;
using WordReplacer.Common;

namespace WordReplacer.Tests.Services;

public class DocumentServiceTest
{
    [Fact]
    private void GetCombinations_WithValidNodeList_ShouldReturnListWithAllPossibleCombinationsInDict()
    {
        // Arrange
        var expectedListResult = new List<Dictionary<string, string>>
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

        List<KeyValuePair<string, List<string>>> nodes = new()
        {
            new KeyValuePair<string, List<string>>("Student", new List<string>() { "A1", "A2" } ),
            new KeyValuePair<string, List<string>>("School", new List<string>() { "E1" } ),
            new KeyValuePair<string, List<string>>("Date", new List<string>() { "D1", "D2" } )
        };

        var result = new List<Dictionary<string, string>>();

        // Act
        result.GetCombinations(nodes, 0, new Dictionary<string, string>());

        // Assert
        (SerializeDictOrdered(result) == SerializeDictOrdered(expectedListResult)).ShouldBeTrue();
    }

    private static string SerializeDictOrdered(IList<Dictionary<string, string>> list)
    {
        var result = new List<KeyValuePair<string, string>>();
        
        foreach (var item in list)
        {
            foreach (var keyValue in item)
            {
                result.Add(new KeyValuePair<string, string>(keyValue.Key, keyValue.Value));
            }
        }

        var orderedResult = result.OrderBy(kv => kv.Key).ThenBy(kv => kv.Value);
        return JsonSerializer.Serialize(orderedResult);
    }

}
using System.Text.Json;

namespace WordReplacer.Common.Tests
{
    public class GenericExtensionsTests
    {
        public class DocumentServiceTests
        {
            private class CombinationsTestData : TheoryData<List<KeyValuePair<string, List<string>>>, List<Dictionary<string, string>>>
            {
                public CombinationsTestData()
                {
                    Add(new List<KeyValuePair<string, List<string>>>()
                        {
                            new ("Student", new List<string>() { "A1", "A2" }),
                            new ("School", new List<string>() { "E1" }),
                            new ("Date", new List<string>() { "D1", "D2" })
                        },
                        new List<Dictionary<string, string>>()
                        {
                            new ()
                            {
                                { "Student", "A1" }, { "School", "E1" }, { "Date", "D1" }
                            },
                            new ()
                            {
                                { "Student", "A1" }, { "School", "E1" }, { "Date", "D2" }
                            },
                            new ()
                            {
                                { "Student", "A2" }, { "School", "E1" }, { "Date", "D1" }
                            },
                            new ()
                            {
                                { "Student", "A2" }, { "School", "E1" }, { "Date", "D2" }
                            }
                        });

                    Add(new List<KeyValuePair<string, List<string>>>()
                        {
                            new ("Student", new List<string>() { "A1", "A2" }),
                            new ("School", new List<string>() { "E1" }),
                            new ("Date", new List<string>() { "D1" })
                        },
                        new List<Dictionary<string, string>>()
                        {
                            new ()
                            {
                                { "Student", "A1" }, { "School", "E1" }, { "Date", "D1" }
                            },
                            new ()
                            {
                                { "Student", "A2" }, { "School", "E1" }, { "Date", "D1" }
                            },
                        });

                    Add(new List<KeyValuePair<string, List<string>>>()
                        {
                            new ("Student", new List<string>() { "A1", "A2" }),
                            new ("School", new List<string>() { "E1" , "E2" }),
                            new ("Date", new List<string>() { "D1", "D2" })
                        },
                        new List<Dictionary<string, string>>()
                        {
                            new ()
                            {
                                { "Student", "A1" }, { "School", "E1" }, { "Date", "D1" }
                            },
                            new ()
                            {
                                { "Student", "A1" }, { "School", "E1" }, { "Date", "D2" }
                            },
                            new ()
                            {
                                { "Student", "A1" }, { "School", "E2" }, { "Date", "D1" }
                            },
                            new ()
                            {
                                { "Student", "A1" }, { "School", "E2" }, { "Date", "D2" }
                            },
                            new ()
                            {
                                { "Student", "A2" }, { "School", "E1" }, { "Date", "D1" }
                            },
                            new ()
                            {
                                { "Student", "A2" }, { "School", "E1" }, { "Date", "D2" }
                            },
                            new ()
                            {
                                { "Student", "A2" }, { "School", "E2" }, { "Date", "D1" }
                            },
                            new ()
                            {
                                { "Student", "A2" }, { "School", "E2" }, { "Date", "D2" }
                            }
                        });
                }
            }

            [Theory]
            [ClassData(typeof(CombinationsTestData))]
            private void GetCombinations_WithValidNodeList_ShouldReturnListWithAllPossibleCombinationsInDict(List<KeyValuePair<string, List<string>>> inputs, List<Dictionary<string, string>> expectedListResult)
            {
                // Arrange
                var result = new List<Dictionary<string, string>>();

                // Act
                result.GetCombinations(inputs, 0, new Dictionary<string, string>());

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

                return JsonSerializer.Serialize(result.OrderBy(kv => kv.Key).ThenBy(kv => kv.Value));
            }
        }
    }
}
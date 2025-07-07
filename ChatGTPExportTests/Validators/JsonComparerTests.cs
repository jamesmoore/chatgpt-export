using ChatGPTExport.Validators;

namespace ChatGTPExportTests.Validators
{
    public class JsonComparerTests
    {
        [Fact]
        public void JsonComparer_Match()
        {
            var json = """
            {
              "number": 42,
              "text": "Hello, world",
              "nested": 
              { 
                "flag": true
              }
            }
            """;

            var diff = JsonComparer.CompareJson(json, json);

            Assert.Empty(diff);
        }

        [Fact]
        public void JsonComparer_Value_Change()
        {
            var json = """
            {
              "number": 42,
              "text": "Hello, world",
              "nested": 
              { 
                "flag": true
              }
            }
            """;

            var json2 = """
            {
              "number": 42,
              "text": "Hello, world",
              "nested": 
              { 
                "flag": false
              }
            }
            """;

            var diff = JsonComparer.CompareJson(json, json2);

            Assert.NotEmpty(diff);
        }

        [Fact]
        public void JsonComparer_Value_Added()
        {
            var json = """
            {
              "number": 42,
              "text": "Hello, world",
              "nested": 
              { 
                "flag": true
              }
            }
            """;

            var json2 = """
            {
              "number": 42,
              "text": "Hello, world",
              "nested": 
              { 
                "flag": true,
                "flag2": "abc123"
              }
            }
            """;

            var diff = JsonComparer.CompareJson(json, json2);

            Assert.NotEmpty(diff);
        }

        [Fact]
        public void JsonComparer_Nested_Type_Added()
        {
            var json = """
            {
              "number": 42,
              "text": "Hello, world",
              "nested": 
              { 
                "flag": true
              }
            }
            """;

            var json2 = """
            {
              "number": 42,
              "text": "Hello, world",
              "nested": 
              { 
                "flag": true,
                "nested2": {
                    "value2": "abc123"
                }
              }
            }
            """;

            var diff = JsonComparer.CompareJson(json, json2);

            Assert.NotEmpty(diff);
        }


        [Fact]
        public void JsonComparer_Array_Match()
        {
            var json = """
            [
                "a",
                1,
                "b",
                2
            ]
            """;

            var diff = JsonComparer.CompareJson(json, json);

            Assert.Empty(diff);
        }

        [Fact]
        public void JsonComparer_Array_Diff()
        {
            var json = """
            [
                "a",
                1,
                "b",
                2
            ]
            """;

            var json2 = """
            [
                "a",
                1,
                2
            ]
            """;

            var diff = JsonComparer.CompareJson(json, json2);
            Assert.NotEmpty(diff);
        }
    }
}

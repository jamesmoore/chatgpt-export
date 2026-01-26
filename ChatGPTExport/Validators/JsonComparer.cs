using System.IO;
using System.Text.Json.Nodes;

namespace ChatGPTExport.Validators
{
    public static class JsonComparer
    {
        public static List<string> CompareJson(Stream json1, string json2)
        {
            var differences = new List<string>();

            var node1 = JsonNode.Parse(json1);
            var node2 = JsonNode.Parse(json2);

            CompareNodes(node1, node2, "", differences);

            return differences;
        }

        private static void CompareNodes(JsonNode? node1, JsonNode? node2, string path, List<string> diffs)
        {
            var node1missing = IsNullOrMissing(node1);
            var node2missing = IsNullOrMissing(node2);
            if (node1missing && node2missing)
                return;

            if (node1missing || node2missing)
            {
                diffs.Add($"{path}: One is null/missing, the other is not");
                return;
            }

            if (node1 is JsonObject obj1 && node2 is JsonObject obj2)
            {
                var allKeys = new HashSet<string>(obj1.Select(kv => kv.Key));
                allKeys.UnionWith(obj2.Select(kv => kv.Key));

                foreach (var key in allKeys)
                {
                    obj1.TryGetPropertyValue(key, out var val1);
                    obj2.TryGetPropertyValue(key, out var val2);

                    CompareNodes(val1, val2, $"{path}.{key}".TrimStart('.'), diffs);
                }
            }
            else if (node1 is JsonArray arr1 && node2 is JsonArray arr2)
            {
                if (arr1.Count != arr2.Count)
                {
                    diffs.Add($"{path}: Array length differs ({arr1.Count} vs {arr2.Count})");
                    return;
                }

                for (int i = 0; i < arr1.Count; i++)
                {
                    CompareNodes(arr1[i], arr2[i], $"{path}[{i}]", diffs);
                }
            }
            else
            {
                if (!JsonEquals(node1, node2))
                {
                    diffs.Add($"{path}: Value differs ({node1} vs {node2})");
                }
            }
        }

        private static bool JsonEquals(JsonNode? node1, JsonNode? node2)
        {
            if (node1 is null || node2 is null) return node1?.ToJsonString() == node2?.ToJsonString();
            
            if(node1.GetValueKind() == System.Text.Json.JsonValueKind.Number &&
                node2.GetValueKind() == System.Text.Json.JsonValueKind.Number)
            {
                return node1.GetValue<double>() == node2.GetValue<double>();
            }
            
            var matches = node1.ToJsonString() == node2.ToJsonString();
            if (matches == false)
            {
                return false;
            }
            return matches;
        }

        private static bool IsNullOrMissing(JsonNode? node)
        {
            // If node is null (missing), or a JsonValue explicitly set to null
            return node is null || (node is JsonValue v && v.ToJsonString() == "null");
        }
    }
}

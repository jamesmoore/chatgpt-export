using System.Globalization;
using System.Text.Json;

namespace ChatGPTExport
{
    public static class ExtensionMethods
    {
        public static DateTimeOffset ToDateTimeOffset(this decimal d)
        {
            // Convert to total milliseconds
            // If the value is < 10^10, assume it's in seconds, so scale it
            var millis = d < 1_000_000_000_0m ? d * 1000 : d;

            // Truncate to long — DateTimeOffset doesn't support sub-millisecond precision
            return DateTimeOffset.FromUnixTimeMilliseconds((long)millis);
        }

        public static bool IsValidJson(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            if (!(input.StartsWith("{") && input.EndsWith("}")) &&
                !(input.StartsWith("[") && input.EndsWith("]")))
            {
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(input);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
            catch
            {
                return false; // optionally handle or log other exceptions
            }
        }

        public static IList<int> GetRenderedElementIndexes(this string input)
        {

            var realList = new List<int>();
            var textElements = StringInfo.GetTextElementEnumerator(input);
            int count = 0;
            while (textElements.MoveNext())
            {
                string element = textElements.GetTextElement();
                realList.Add(count);
                count += element.GetRealElementWidth();
            }

            return realList;
        }

        public static int GetRealElementWidth(this string element)
        {
            const char VariationSelector16 = '\uFE0F';

            // FLAG EMOJI SPECIAL CASE
            if (IsFlagEmoji(element))
            {
                return 3;
            }

            if (element.Contains(VariationSelector16)) //  eg "⚠️" or 🗂️ or 🧛‍♂️
            {
                if (element.Length > 2)
                {
                    return 2;
                }
                else
                {
                    return element.Length - 1;
                }
            }
            else if (element.Length == 2 && char.IsSurrogatePair(element, 0))
            {
                return 2; // external system treats this emoji as 2 units
            }
            else if (element.Length > 1)
            {
                int unitCount = 0;
                for (int i = 0; i < element.Length; i++)
                {
                    if (char.IsHighSurrogate(element[i]) &&
                        i + 1 < element.Length &&
                        char.IsLowSurrogate(element[i + 1]))
                    {
                        unitCount++;
                        i++; // skip low surrogate
                    }
                    else
                    {
                        unitCount++;
                    }
                }

                return unitCount;
            }
            else
            {
                return element.Length;
            }
        }


        static bool IsFlagEmoji(string element)
        {
            if (element.Length != 4)
                return false;

            int first = char.ConvertToUtf32(element, 0);
            int second = char.ConvertToUtf32(element, 2);

            return first >= 0x1F1E6 && first <= 0x1F1FF &&
                   second >= 0x1F1E6 && second <= 0x1F1FF;
        }
    }
}

using System.Text.RegularExpressions;

namespace AggregationTestProject.Utilities
{
    public class ZPLFormatter
    {
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

        public ZPLFormatter SetZPLVariables(string key, string value)
        {
            keyValuePairs[key] = value;
            return this;
        }

        public string FormatZPL(string data)
        {
            foreach (var kv in keyValuePairs)
            {
                string pattern = $"#V{kv.Key}#V";
                data = Regex.Replace(data, pattern, kv.Value);
            }

            return data;
        }
    }
}

using System.Text;

namespace AggregationTestProject.Services.Utilities
{
    public static class StringService
    {
        public static byte[] GetBytesFromString(string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        public static string GetStringFromBytes(byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }
    }
}

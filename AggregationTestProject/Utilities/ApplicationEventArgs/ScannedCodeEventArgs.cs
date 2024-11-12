namespace AggregationTestProject.Utilities.ApplicationEventArgs
{
    public class ScannedCodeEventArgs
    {
        public string ReadableData { get; set; }

        public ScannedCodeEventArgs(string readableData)
        {
            this.ReadableData = readableData;
        }
    }
}

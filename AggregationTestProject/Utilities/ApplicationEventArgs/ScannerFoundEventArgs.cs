namespace AggregationTestProject.Utilities.ApplicationEventArgs
{
    public class ScannerFoundEventArgs : EventArgs
    {
        public string Brand { get; set; }
        public string SerialNumber { get; set; }
        public bool IsStation { get; set; }
        public bool IsOk { get; set; }

        public ScannerFoundEventArgs(string brand, string serialNumber, bool isStation, bool isOk)
        {
            Brand = brand;
            SerialNumber = serialNumber;
            IsStation = isStation;
            IsOk = isOk;
        }
    }
}

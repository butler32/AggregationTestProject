namespace AggregationTestProject.Utilities.ApplicationEventArgs
{
    public class CameraFoundEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string Ip {  get; set; }
        public bool IsOk { get; set; }

        public CameraFoundEventArgs(string name, string ip, bool isOk)
        {
            Name = name;
            Ip = ip;
            IsOk = isOk;
        }
    }
}

namespace AggregationTestProject.Models.Settings
{
    public class ServerOptions
    {
        public string Name { get; set; }
        public string Host {  get; set; }
        public string Ip {  get; set; }
        public int Timeout { get; set; }
        public int Threshold { get; set; }
        public int PingInterval { get; set; }
        public int YellowThreshold { get; set; }
        public int RedThreshold { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

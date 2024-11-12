namespace AggregationTestProject.Models.Settings
{
    public class PrinterOptions
    {
        public PrinterDetail BoxPrinter {  get; set; }
        public PrinterDetail PalletPrinter { get; set; }
        public ApplicatorDetail BoxApplicator { get; set; }
        public ApplicatorDetail PalletApplicator { get; set; }
    }
}

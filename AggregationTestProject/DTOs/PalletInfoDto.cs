namespace AggregationTestProject.DTOs
{
    public class PalletInfoDto
    {
        public int Id { get; set; }
        public int Counter { get; set; }
        public int BoxesInPallet { get; set; }
        public string Code { get; set; }
        public string Date {  get; set; }
        public string Time {  get; set; }
    }

    public class PalletInfoResultDto
    {
        public PalletInfoDto Pallet { get; set; }
        public ErrorDto Error { get; set; }
    }
}

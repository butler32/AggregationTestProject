namespace AggregationTestProject.DTOs
{
    public class LotDto
    {
        public int Id { get; set; }
        public string DateAt { get; set; }
        public DateTime DateAtDateTime
        {
            get => DateTime.ParseExact(DateAt, "dd.MM.yy", null);
        }
        public string Number { get; set; }
        public PackageDto Package { get; set; }
        public ProductDto Product { get; set; }
    }
}

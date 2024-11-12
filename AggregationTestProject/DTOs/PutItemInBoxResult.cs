namespace AggregationTestProject.DTOs
{
    public class PutItemInBoxResult
    {
        public int BoxId { get; set; }
        public int ItemId { get; set; }
        public int BoxFormat {  get; set; }
        public int ItemsInsideBox { get; set; }
    }
}

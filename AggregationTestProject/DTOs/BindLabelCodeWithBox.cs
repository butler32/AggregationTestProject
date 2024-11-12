namespace AggregationTestProject.DTOs
{
    public class BindLabelCodeWithBox
    {
        public int Id { get; set; }
        public int Counter { get; set; }
        public string Code { get; set; }
    }

    public class BindLabelCodeWithBoxResult
    {
        public BindLabelCodeWithBox Box { get; set; }
    }
}

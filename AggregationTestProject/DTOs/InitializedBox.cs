namespace AggregationTestProject.DTOs
{
    public class InitializedBox
    {
        public int Id { get; set; }
    }

    public class InitializedBoxResult
    {
        public InitializedBox Box { get; set; }
        public ErrorDto Error { get; set; }
    }
}

namespace AggregationTestProject.DTOs
{
    public class InitializedPallet
    {
        public int Id { get; set; }
    }

    public class InitializedPalletResult
    {
        public InitializedPallet Pallet { get; set; }
        public ErrorDto Error { get; set; }
    }
}

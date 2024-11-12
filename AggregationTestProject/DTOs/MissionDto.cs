namespace AggregationTestProject.DTOs
{
    public class MissionDto
    {
        public int Id { get; set; }
        public string DateAt { get; set; }
        public DateTime DateAtDateTime
        {
            get => DateTime.ParseExact(DateAt, "dd.MM.yy", null);
        }
        public int CodeTypeId { get; set; }
        public int NeedToMark { get; set; }
        public bool IsOneAgglevel { get; set; }
        public int ParentBoxCounter { get; set; }
        public int BoxCounter { get; set; }
        public LotDto Lot { get; set; }
    }
}

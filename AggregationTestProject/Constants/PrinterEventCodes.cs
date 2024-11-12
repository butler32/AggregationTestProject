namespace AggregationTestProject.Constants
{
    public enum PrinterEventCodes : ushort
    {
        SetAll = 0b11111110_11111110,
        StartGeneration = (1 << 7 + 8),
        EndGeneration = (1 << 6 + 8),
        StartPrinting = (1 << 5 + 8),
        EndPrint = (1 << 4 + 8),
        StartCutting = (1 << 3 + 8),
        EndCut = (1 << 2 + 8),
        StartFeeding = (1 << 1 + 8),

        EndLabelFeed = (1 << 7),
        StartPrintOrder = (1 << 6),
        EndPrintOrder = (1 << 5),
        Error = (1 << 4),
        PrintStopped = (1 << 2),
        PrintResumed = (1 << 1),
        None = 0
    }
}

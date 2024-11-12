namespace AggregationTestProject.Constants
{
    public enum ZebraBeepCode
    {
        OneShortHigh = 0x00,
        TwoShortHigh = 0x01,
        ThreeShortHigh = 0x02,
        FourShortHigh = 0x03,
        FiveShortHigh = 0x04,

        OneShortLow = 0x05,
        TwoShortLow = 0x06,
        ThreeShortLow = 0x07,
        FourShortLow = 0x08,
        FiveShortLow = 0x09,

        OneLongHigh = 0x0a,
        TwoLongHigh = 0x0b,
        ThreeLongHigh = 0x0c,
        FourLongHigh = 0x0d,
        FiveLongHigh = 0x0e,

        OneLongLow = 0x0f,
        TwoLongLow = 0x10,
        ThreeLongLow = 0x11,
        FourLongLow = 0x12,
        FiveLongLow = 0x13,

        FastHighLowHighLow = 0x14,
        SlowHighLowHighLow = 0x15,
        HighLow = 0x16,
        LowHigh = 0x17,
        HighLowHigh = 0x18,
        LowHighLow = 0x19
    }
}

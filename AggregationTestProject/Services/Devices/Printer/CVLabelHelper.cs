using AggregationTestProject.DTOs;
using AggregationTestProject.Services.Utilities;
using System.Globalization;

namespace AggregationTestProject.Services.Devices.Printer
{
    public class CVLabelHelper
    {
        private const char StartDelimiter = '\u0001';
        private const char EndDelimiter = '\u0017';

        public static byte[] GetLabelData(MissionDto missionDto)
        {
            var labelData = string.Empty;

            labelData += $"{StartDelimiter}FBE---rtestLabel{EndDelimiter}";
            labelData += Environment.NewLine;
            labelData += $"{StartDelimiter}AM[1]3324;5110;0;52;0;200;0;0;9;6;7{EndDelimiter}";
            labelData += Environment.NewLine;
            labelData += $"{StartDelimiter}BM[1]{PrepareCode(missionDto)}{EndDelimiter}";
            labelData += Environment.NewLine;
            labelData += $"{StartDelimiter}AM[2]4382;3627;0;4;0;5;400;400;0;10{EndDelimiter}";
            labelData += Environment.NewLine;
            labelData += $"{StartDelimiter}BM[2]GTIN: {missionDto.Lot.Product.Gtin}{EndDelimiter}";
            labelData += Environment.NewLine;
            labelData += $"{StartDelimiter}AM[3]5124;5056;0;4;0;5;400;400;0;10{EndDelimiter}";
            labelData += Environment.NewLine;
            labelData += $"{StartDelimiter}BM[2]No: {missionDto.BoxCounter}{EndDelimiter}";
            labelData += Environment.NewLine;
            labelData += $"{StartDelimiter}FBAA--r1{EndDelimiter}";
            labelData += Environment.NewLine;
            labelData += $"{StartDelimiter}FBBA--r00001{EndDelimiter}";
            labelData += Environment.NewLine;
            labelData += $"{StartDelimiter}FBC---r{EndDelimiter}";

            return StringService.GetBytesFromString(labelData);
        }

        private static string PrepareCode(MissionDto missionDto)
        {
            var productationDate = DateTime.ParseExact(missionDto.Lot.DateAt, "dd.MM.yy", CultureInfo.InvariantCulture).ToString("yyMMdd");
            var expiryDate = DateTime.ParseExact(missionDto.DateAt, "dd.MM.yy", CultureInfo.InvariantCulture).ToString("yyMMdd");

            return $"02{missionDto.Lot.Product.Gtin}" +
                $"11{productationDate}" +
                $"17{expiryDate}" +
                $"10{missionDto.Lot.Number}>1d" +
                $"37{missionDto.Lot.Package.BoxFormat}>1d" +
                $"2001" +
                $"21{missionDto.BoxCounter:D4}";
        }
    }
}

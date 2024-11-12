using AggregationTestProject.Constants;
using AggregationTestProject.Models.Settings;
using AggregationTestProject.Shared;
using AggregationTestProject.Utilities;
using log4net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using static AggregationTestProject.Services.Utilities.ZPLCalculator;

namespace AggregationTestProject.Services.Devices.Printer
{
    public class ZebraPrinter
    {
        private readonly ILog _log;
        private readonly Setting _settings;
        private readonly SharedState _sharedState;

        private string _boxLabelPath = @"C:\Users\Admin\source\repos\AggregationTestProject\AggregationTestProject\Label\boxLabel.txt";
        private string _palletLabelPath = @"C:\Users\Admin\source\repos\AggregationTestProject\AggregationTestProject\Label\palletLabel.txt";

        public ZebraPrinter(ILog log, Setting setting, SharedState sharedState)
        {
            _log = log;
            _settings = setting;
            _sharedState = sharedState;
        }

        public string PreparePalletLabel(
            DPI dpi, string code, 
            int labelWidthMm, int labelHeightMm, 
            int modulesCount, int modulesWidthMm,
            int headerWidthMm, int fontSizeMm, int gtinTextWidthMm)
        {
            var labelWidthDots = MillimetersToDots(labelWidthMm, dpi);
            var labelHeightDots = MillimetersToDots(labelHeightMm, dpi);
            var moduleWidthDots = MillimetersToDots(modulesWidthMm, dpi);
            var fontSizeDots = MillimetersToDots(fontSizeMm, dpi);
            var headerWidthDots = MillimetersToDots(headerWidthMm, dpi);

            var textYDelta = moduleWidthDots * 3;
            var fullTextHeigthDot = textYDelta + 3 * fontSizeDots;

            var dmWidthDot = modulesCount * moduleWidthDots;

            var topLeftDMPoint = CalculateTopLeftCorner(
                dmWidthDot,
                dmWidthDot + fullTextHeigthDot,
                labelWidthDots,
            labelHeightDots);

            var gtinTextWidthDot = MillimetersToDots(gtinTextWidthMm, dpi);

            var topLeftTextPoint = CalculateTopLeftCorner(
                gtinTextWidthDot,
                dmWidthDot + fullTextHeigthDot,
                labelWidthDots,
                labelHeightDots);

            var textY = topLeftDMPoint.Y + dmWidthDot + textYDelta;

            var commonFormatter = new ZPLFormatter()
                .SetZPLVariables("CODE", code)
                .SetZPLVariables("DMX", topLeftDMPoint.X.ToString())
                .SetZPLVariables("DMY", topLeftDMPoint.Y.ToString())
                .SetZPLVariables("DMModuleWidth", moduleWidthDots.ToString())
                .SetZPLVariables("DMModuleCount", modulesCount.ToString())
                .SetZPLVariables("HeaderX", (topLeftTextPoint.X).ToString())
                .SetZPLVariables("GTINY", (textY).ToString())
                .SetZPLVariables("ValueX", (topLeftTextPoint.X + headerWidthDots).ToString())
                .SetZPLVariables("FontSize", fontSizeDots.ToString());

            var fileToPrint = File.ReadAllText(_palletLabelPath);

            return commonFormatter.FormatZPL(fileToPrint);
        }

        public async Task PrintPalletLabel(string code)
        {
            var result = Encoding.ASCII.GetBytes(code);

            await SendTextToPrint(result);
        }

        public string PrepareBoxLabel(
            DPI dpi,
            string gtin, string productionDate, string expiryDate, string batch,
            string boxFormat, string agLevel,
            int labelWidthMm, int labelHeightMm, int modulesCount, int modulesWidthMm,
            int headerWidthMm, int fontSizeMm, int gtinTextWidthMm)
        {
            var labelWidthDots = MillimetersToDots(labelWidthMm, dpi);
            var labelHeightDots = MillimetersToDots(labelHeightMm, dpi);
            var moduleWidthDots = MillimetersToDots(modulesWidthMm, dpi);
            var fontSizeDots = MillimetersToDots(fontSizeMm, dpi);
            var headerWidthDots = MillimetersToDots(headerWidthMm, dpi);

            var textYDelta = moduleWidthDots * 3;
            var fullTextHeigthDot = textYDelta + 3 * fontSizeDots;

            var dmWidthDot = modulesCount * moduleWidthDots;

            var topLeftDMPoint = CalculateTopLeftCorner(
                dmWidthDot,
                dmWidthDot + fullTextHeigthDot,
                labelWidthDots,
            labelHeightDots);

            var gtinTextWidthDot = MillimetersToDots(gtinTextWidthMm, dpi);

            var topLeftTextPoint = CalculateTopLeftCorner(
                gtinTextWidthDot,
                dmWidthDot + fullTextHeigthDot,
                labelWidthDots,
                labelHeightDots);

            var textY = topLeftDMPoint.Y + dmWidthDot + textYDelta;

            var commonFormatter = new ZPLFormatter()
                .SetZPLVariables("GTIN", gtin)
                .SetZPLVariables("ProductionDate", productionDate)
                .SetZPLVariables("ExpiryDate", expiryDate)
                .SetZPLVariables("Batch", batch)
                .SetZPLVariables("BoxFormat", boxFormat)
                .SetZPLVariables("AgLevel", agLevel)
                .SetZPLVariables("DMX", topLeftDMPoint.X.ToString())
                .SetZPLVariables("DMY", topLeftDMPoint.Y.ToString())
                .SetZPLVariables("DMModuleWidth", moduleWidthDots.ToString())
                .SetZPLVariables("DMModuleCount", modulesCount.ToString())
                .SetZPLVariables("HeaderX", (topLeftTextPoint.X).ToString())
                .SetZPLVariables("GTINY", (textY).ToString())
                .SetZPLVariables("BatchY", (textY + fontSizeDots).ToString())
                .SetZPLVariables("BoxCounterY", (textY + 2 * fontSizeDots).ToString())
                .SetZPLVariables("ValueX", (topLeftTextPoint.X + headerWidthDots).ToString())
                .SetZPLVariables("FontSize", fontSizeDots.ToString());

            var fileToPrint = File.ReadAllText(_boxLabelPath);

            return commonFormatter.FormatZPL(fileToPrint);
        }

        public async Task PrintBoxLabel(string commonLabel, string serialNumber, int printCount = 1, bool isReversePrint = false)
        {
            var specificFormatter = new ZPLFormatter();

            var resultBuilder = new StringBuilder();

            var startValueInt = int.Parse(serialNumber);

            for (int i = 0; i < printCount; i++)
            {
                var boxCounterInt = isReversePrint ? startValueInt + printCount - 1 - i : startValueInt + i;

                var boxCounter = boxCounterInt.ToString($"D{serialNumber.Length}");

                specificFormatter.SetZPLVariables("BoxCounter", boxCounter);
                var formattedZPL = specificFormatter.FormatZPL(commonLabel);

                resultBuilder.Append(formattedZPL);
            }

            var stringResult = resultBuilder.ToString();

            var result = Encoding.ASCII.GetBytes(stringResult);

            await SendTextToPrint(result);
        }

        public async Task SendTextToPrint(byte[] data)
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    var startConnect = tcpClient.ConnectAsync(_settings.PrinterOptions.BoxPrinter.Ip, _settings.PrinterOptions.BoxPrinter.Port);

                    if (await Task.WhenAny(startConnect, Task.Delay(2000)) != startConnect)
                    {
                        throw new Exception("Connection error");
                    }

                    Console.WriteLine("Connection established");

                    using (NetworkStream netStream = tcpClient.GetStream())
                    {
                        int maxChunkSize = 8192;
                        int totalLength = data.Length;
                        int bytesSent = 0;

                        while (bytesSent < totalLength)
                        {
                            int bytesToSend = Math.Min(maxChunkSize, totalLength - bytesSent);
                            await netStream.WriteAsync(data, bytesSent, bytesToSend);
                            bytesSent += bytesToSend;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Zebra printer: error", ex);
            }
        }
    }
}

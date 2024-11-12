using AggregationTestProject.Constants;
using AggregationTestProject.Services.Utilities;
using log4net;
using System.Text.RegularExpressions;

namespace AggregationTestProject.Services.Devices.Printer
{
    public class CarlValentineApplicator : IApplicatorService
    {
        private readonly ILog _log;

        private int _port;

        public bool IsConnected { get; private set; }

        public string Name { get; }

        public string Ip { get; private set; }

        public event EventHandler<string> ErrorMessageReceived;
        public event EventHandler HeartbeatReceived;
        public event EventHandler<bool> ConnectionStateChanged;

        private Thread _receiveDataThread;
        private Thread _sendHeartbeatThread;

        private TcpClientWrapper _tcpClient;

        public CarlValentineApplicator(ILog log)
        {
            _log = log;
        }

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            Ip = ip;
            _port = port;

            try
            {
                _tcpClient = new(_log);

                await _tcpClient.ConnectAsync(Ip, _port);

                ConnectionStateChanged?.Invoke(this, true);

                await SendDataAsync(GetInitialCommand);

                await SendDataAsync(GenerateAutostatusCommand(setAllToTrue: true));

                _sendHeartbeatThread = new Thread(new ThreadStart(SendHeartbeatLoopAsync));
                _receiveDataThread = new Thread(new ThreadStart(ReceiveDataLoopAsync));

                _sendHeartbeatThread.Start();
                _receiveDataThread.Start();
            }
            catch (Exception ex)
            {
                _tcpClient.Dispose();

                _log.Error(ex);

                return false;

                throw;
            }

            return true;
        }

        public async Task SendDataAsync(byte[] data)
        {
            try
            {
                await _tcpClient.SendDataAsync(data);
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw;
            }
        }

        private static List<byte[]> GetCommandArrays(byte[] byteArray)
        {
            List<byte[]> commandArrays = new List<byte[]>();
            List<byte> commandContent = new List<byte>();

            for (int i = 0; i < byteArray.Length; i++)
            {
                if (byteArray[i] == 1) // Start of command
                {
                    commandContent.Clear(); // Clear previous command content
                    i++; // Move to next byte
                    while (i < byteArray.Length && byteArray[i] != 23) // Until end of command
                    {
                        commandContent.Add(byteArray[i]);
                        i++;
                    }
                    commandArrays.Add(commandContent.ToArray()); // Add command content to the list
                }
            }

            return commandArrays;
        }

        private async void ReceiveDataLoopAsync()
        {
            _log.Debug($"{Name}: Запуск процесса получения ответа.");
            while (_tcpClient.IsConnected)
            {
                _log.Debug($"{Name}: Начало получения ответа.");
                var commandBytes = await _tcpClient.ReceiveDataAsync();
                _log.Debug($"{Name}: Конец получения ответа.");

                if (commandBytes != null)
                {
                    var commands = GetCommandArrays(commandBytes);
                    _log.Debug($"{Name}: Получение массива комманд.");

                    foreach (var command in commands)
                    {
                        HeartbeatReceived?.Invoke(this, null);

                        var commandStr = StringService.GetStringFromBytes(command);
                        _log.Debug($"{Name}: Получена команда - {commandStr}.");

                        if (command[0] == (byte)'A')
                        {
                            if (command[command.Length - 1] == (byte)PrinterMessageCodes.ErrorMessage)
                            {
                                byte[] newData = new byte[command.Length - 1];
                                Array.Copy(command, 1, newData, 0, command.Length - 1);
                                var message = StringService.GetStringFromBytes(newData);

                                // извлечь текст из завычек
                                string result = GetTextMessage(message);

                                ErrorMessageReceived?.Invoke(this, result);
                            }
                            else if (command[command.Length - 1] == (byte)PrinterMessageCodes.Heartbeat)
                            {
                                HeartbeatReceived?.Invoke(this, null);
                            }
                        }
                    }

                }
                else
                {
                    _log.Debug($"{Name}: Получение данных: получен пустой ответ.");
                }

            }
            _log.Debug($"{Name}: Завершение процесса получения сообщений.");

        }

        private static string GetTextMessage(string message)
        {
            string result;
            string pattern = "\"([^\"]*)\"";

            Match match = Regex.Match(message, pattern);

            if (match.Success)
            {
                result = match.Groups[1].Value;
            }
            else
            {
                result = message;
            }

            return result;
        }

        private async void SendHeartbeatLoopAsync()
        {
            _log.Debug($"Applicator: Запуск процесса периодического опроса.");

            try
            {
                while (true)
                {
                    await Task.Delay(2000);
                    await SendAndReceiveHeartbeatAsync();
                }
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(this, false);
            }

            _log.Debug($"Applicator: Завершение процесса периодического опроса.");
        }

        private async Task SendAndReceiveHeartbeatAsync()
        {
            var responseReceived = new TaskCompletionSource<object>();

            void DataReceivedHandler(object? sender, EventArgs e)
            {
                if (!responseReceived.Task.IsCompleted)
                {
                    responseReceived.SetResult(null);
                }
            }

            HeartbeatReceived += DataReceivedHandler;

            try
            {
                _log.Debug("Heartbeat send");

                await SendDataAsync(GetHeartbeatCommand);

                if (await Task.WhenAny(responseReceived.Task, Task.Delay(2000)) != responseReceived.Task)
                {
                    throw new Exception("Heartbeat timeout");
                }
            }
            finally
            {
                HeartbeatReceived -= DataReceivedHandler;
            }
        }

        private static byte[] GetInitialCommand
        {
            get
            {
                return new byte[] { 0x01 }
                    .Concat(StringService.GetBytesFromString("RCGA--wXXXXXXXX"))
                    .Concat(new byte[] { 0x17 })
                    .ToArray();
            }
        }

        private static byte[] GetHeartbeatCommand
        {
            get
            {
                return new byte[] { 0x01 }
                    .Concat(StringService.GetBytesFromString("RCGA--w"))
                    .Concat(new byte[] { (byte)PrinterMessageCodes.Heartbeat, 0x17 })
                    .ToArray();
            }
        }

        private static byte[] GenerateAutostatusCommand(
            bool startGeneration = false,
            bool startCutting = false,
            bool endGeneration = false,
            bool endCut = false,
            bool startPrinting = false,
            bool startFeeding = false,
            bool endPrint = false,
            bool endLabelFeed = false,
            bool startPrintOrder = false,
            bool printStopped = false,
            bool endPrintOrder = false,
            bool printResumed = false,
            bool error = false,
            bool setAllToTrue = false
        )
        {
            // First byte
            ushort commandContent = 0;
            if (setAllToTrue || startGeneration) commandContent |= (ushort)PrinterEventCodes.StartGeneration;
            if (setAllToTrue || startCutting) commandContent |= (ushort)PrinterEventCodes.StartCutting;
            if (setAllToTrue || endGeneration) commandContent |= (ushort)PrinterEventCodes.EndGeneration;
            if (setAllToTrue || endCut) commandContent |= (ushort)PrinterEventCodes.EndCut;
            if (setAllToTrue || startPrinting) commandContent |= (ushort)PrinterEventCodes.StartPrinting;
            if (setAllToTrue || startFeeding) commandContent |= (ushort)PrinterEventCodes.StartFeeding;
            if (setAllToTrue || endPrint) commandContent |= (ushort)PrinterEventCodes.EndPrint;

            if (setAllToTrue || endLabelFeed) commandContent |= (ushort)PrinterEventCodes.EndLabelFeed;
            if (setAllToTrue || startPrintOrder) commandContent |= (ushort)PrinterEventCodes.StartPrintOrder;
            if (setAllToTrue || printStopped) commandContent |= (ushort)PrinterEventCodes.PrintStopped;
            if (setAllToTrue || endPrintOrder) commandContent |= (ushort)PrinterEventCodes.EndPrintOrder;
            if (setAllToTrue || printResumed) commandContent |= (ushort)PrinterEventCodes.PrintResumed;
            if (setAllToTrue || error) commandContent |= (ushort)PrinterEventCodes.Error;

            byte byte1 = (byte)((commandContent >> 8) & 0xFF);
            byte byte2 = (byte)(commandContent & 0xFF);


            var commandBytes = new byte[] { 0x01, (byte)'G', byte1, byte2, 0x17, (byte)PrinterMessageCodes.Autostatus };

            return commandBytes;
        }
    }
}

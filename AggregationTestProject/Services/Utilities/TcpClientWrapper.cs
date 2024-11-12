using log4net;
using System.IO;
using System.Net.Sockets;

namespace AggregationTestProject.Services.Utilities
{
    public class TcpClientWrapper : IDisposable
    {
        private readonly ILog _log;

        private string _ip;
        private int _port;

        private Socket _socket;

        public bool IsConnected
        {
            get
            {
                return _socket is not null && _socket.Connected;
            }
        }

        public TcpClientWrapper(ILog log)
        {
            _log = log;
        }

        public async Task ConnectAsync(string ip, int port)
        {
            _ip = ip;
            _port = port;

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _socket.NoDelay = true;

                var connectTask = _socket.ConnectAsync(_ip, _port);

                if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                {
                    throw new Exception("Connection timeout");
                }

                if (IsConnected)
                {
                    _log.Info("Tcp connection established");
                }
                else
                {
                    throw new Exception("Cannot established tcp connection");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw;
            }
        }

        public async Task SendDataAsync(byte[] data)
        {
            try
            {
                NetworkStream stream = new NetworkStream(_socket, true);
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw;
            }
        }

        public async Task<byte[]> ReceiveDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (IsConnected)
                {
                    byte[] buffer = new byte[1024];

                    MemoryStream memoryStream = new MemoryStream();
                    NetworkStream networkStream = new NetworkStream(_socket, false);
                    do
                    {
                        int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                        if (bytesRead > 0)
                        {
                            memoryStream.Write(buffer, 0, bytesRead);
                        }
                    } while (networkStream.DataAvailable);

                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            return null;
        }

        public async Task ForceCloseAsync()
        {
            try
            {
                _socket.Close();
                await ConnectAsync(_ip, _port);
                this.Dispose();
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw;
            }
        }

        public void Dispose()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
        }
    }
}

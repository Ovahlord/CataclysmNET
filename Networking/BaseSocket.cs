using System.Net.Sockets;

namespace Networking
{
    /// <summary>
    /// The base socket class handles the lower level networking operations. It creates a session that handles the high level operations, such as packet handling
    /// </summary>
    public abstract class BaseSocket
    {
        public BaseSocket(TcpClient client)
        {
            _client = client;
            Session = CreateSession();
        }

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly TcpClient _client;
        public BaseSession Session { get; private set; }

        public void Start()
        {
            Console.WriteLine($"[{GetType().Name}] Started socket for client {_client.Client.RemoteEndPoint}");
            Task.Run(ReadDataFromStream, _cancellationTokenSource.Token);
        }

        public void Close()
        {
            Console.WriteLine($"[{GetType().Name}] Closed socket for client {_client.Client.RemoteEndPoint}");
            _cancellationTokenSource.Cancel();
            _client.Close();
        }

        public async Task ReadDataFromStream()
        {
            try
            {
                byte[] readBuffer = new byte[1024];

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    int bytesReceived = await _client.GetStream().ReadAsync(readBuffer, _cancellationTokenSource.Token);
                    // Receiving 0 bytes means that the client has been closed or lost its connection
                    if (bytesReceived == 0)
                    {
                        Close();
                        return;
                    }

                    Console.WriteLine($"[{GetType().Name}] Received {bytesReceived} bytes from tcp stream");
                    DataReceived(readBuffer, bytesReceived);
                }

                return;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Close();
            }
        }

        public async Task WriteDataToStreamAsync(ReadOnlyMemory<byte> data)
        {
            try
            {
                await _client.GetStream().WriteAsync(data, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public abstract BaseSession CreateSession();
        public abstract void DataReceived(byte[] data, int dataLength);
    }
}

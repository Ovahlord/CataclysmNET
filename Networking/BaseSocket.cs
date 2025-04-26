using Packets;
using System.Net.Sockets;

namespace Networking
{
    /// <summary>
    /// The base socket class handles the lower level networking operations. It creates a session that handles the high level operations, such as packet handling
    /// </summary>
    public abstract class BaseSocket(TcpClient client)
    {
        protected readonly CancellationTokenSource _cancellationTokenSource = new();
        public BaseSession? Session { get; private set; }

        public void Start()
        {
            Console.WriteLine($"[{GetType().Name}] Started socket for client {client.Client.RemoteEndPoint}");
            Task.Run(ReadDataFromStream, _cancellationTokenSource.Token);
        }

        public void Close()
        {
            Console.WriteLine($"[{GetType().Name}] Closed socket for client {client.Client.RemoteEndPoint}");
            _cancellationTokenSource.Cancel();
            client.Close();
        }

        public async Task ReadDataFromStream()
        {
            try
            {
                byte[] readBuffer = new byte[1024];

                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    int bytesReceived = await client.GetStream().ReadAsync(readBuffer, _cancellationTokenSource.Token);
                    // Receiving 0 bytes means that the client has been closed or lost its connection
                    if (bytesReceived == 0)
                    {
                        Close();
                        return;
                    }

                    // Create a session when we receive our first data
                    if (Session == null)
                        Session = CreateSession();

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
                await client.GetStream().WriteAsync(data, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public virtual async Task SendPacketAsync(ServerPacket packet)
        {
            await WriteDataToStreamAsync(packet.Write().GetRawPacket());
        }

        public abstract BaseSession CreateSession();
        public abstract void DataReceived(byte[] data, int dataLength);
    }
}

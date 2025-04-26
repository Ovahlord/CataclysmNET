using Packets;
using System.Net.Sockets;

namespace Core.Networking
{
    /// <summary>
    /// The base socket class handles the lower level networking operations. It creates a session that handles the high level operations, such as packet handling
    /// </summary>
    public abstract class BaseSocket(TcpClient client)
    {
        protected readonly CancellationTokenSource _cancellationTokenSource = new();
        public BaseSession? Session { get; private set; }

        /// <summary>
        /// Initializes the stream reading task so it can received packets from the client
        /// </summary>
        public void Start()
        {
            Console.WriteLine($"[{GetType().Name}] Started socket for client {client.Client.RemoteEndPoint}");
            Task.Run(ReadDataFromStream, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Cancels the stream reading task so it can no longer received packets from the client and closes it afterwards
        /// </summary>
        public void Close()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            Console.WriteLine($"[{GetType().Name}] Closed socket for client {client.Client.RemoteEndPoint}");
            _cancellationTokenSource.Cancel();
            client.Close();
            Session?.Close();
        }

        /// <summary>
        /// Reads buffered data from the Tcp stream which has been sent by the client
        /// </summary>
        private async Task ReadDataFromStream()
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

        /// <summary>
        /// Writes buffered data to the Tcp stream which will be received by the client
        /// </summary>
        protected async Task WriteDataToStreamAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            try
            {
                await client.GetStream().WriteAsync(data, cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public virtual async Task SendPacketAsync(ServerPacket packet, CancellationToken cancellationToken = default)
        {
            await WriteDataToStreamAsync(packet.Write().GetRawPacket(), cancellationToken);
        }

        public abstract BaseSession CreateSession();
        public abstract void DataReceived(byte[] data, int dataLength);
    }
}

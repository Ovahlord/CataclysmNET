using Core.Packets;
using System.Collections.Concurrent;
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
        private readonly SemaphoreSlim _writeSemaphore = new(0);
        private readonly ConcurrentQueue<ServerPacket> _serverPacketQueue = new();

        /// <summary>
        /// Initializes the stream reading task so it can received packets from the client
        /// </summary>
        public void Start()
        {
            Console.WriteLine($"[{GetType().Name}] Started socket on endpoint {client.Client.LocalEndPoint} for client {client.Client.RemoteEndPoint}");
            _ = ReadDataFromStreamAsync();
            _ = WriteDataToStreamAsync();
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
        private async Task ReadDataFromStreamAsync()
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

        private async Task WriteDataToStreamAsync()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    // Wait for the packet queue to actually have something to send.
                    await _writeSemaphore.WaitAsync(_cancellationTokenSource.Token);
                    while (_serverPacketQueue.TryDequeue(out ServerPacket? packet))
                    {
                        byte[] payload = packet.Write().GetRawPacket();
                        byte[]? header = BuildPacketHeader(payload, packet.Cmd);

                        if (header != null)
                            await client.GetStream().WriteAsync(header, _cancellationTokenSource.Token);

                        await client.GetStream().WriteAsync(payload, _cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Close();
            }
        }

        public void EnqueuePacket(ServerPacket packet)
        {
            _serverPacketQueue.Enqueue(packet);
            _writeSemaphore.Release();
        }

        public abstract BaseSession CreateSession();
        public abstract void DataReceived(byte[] data, int dataLength);
        protected virtual byte[]? BuildPacketHeader(byte[] payload, int cmd) { return null; }
    }
}

using Core.Packets;
using Core.Packets.Opcodes;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Core.Networking
{
    /// <summary>
    /// The base socket class handles the lower level networking operations. It creates a session that handles the high level operations, such as packet handling
    /// </summary>
    public abstract class BaseSocket(TcpClient client)
    {
        public BaseSession? Session { get; private set; }
        private readonly SemaphoreSlim _writeSemaphore = new(0);
        private readonly ConcurrentQueue<ServerPacket> _serverPacketQueue = new();
        private bool _isClosing = false;
        private bool _isClosed = false;
        private bool _isCommsSuspended = false;

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
        /// Closes the socket and requests that the Tcp stream will be closed as well.
        /// </summary>
        public void Close()
        {
            if (_isClosed)
                return;

            _isClosed = true;

            Console.WriteLine($"[{GetType().Name}] Closed socket for client {client.Client.RemoteEndPoint}");
            client.Close();
            Session?.Close();
        }

        /// <summary>
        /// Closes the socket after all remaining server packets have been sent out.
        /// </summary>
        public void DelayedClose()
        {
            if (_isClosing)
                return;

            _isClosing = true;

            if (_serverPacketQueue.IsEmpty)
                Close();
        }

        /// <summary>
        /// Suspends the server-to-client communication, which means it will no longer receive any packets from the server
        /// </summary>
        public void SuspendComms()
        {
            _isCommsSuspended = true;
        }

        /// <summary>
        /// Reads buffered data from the Tcp stream which has been sent by the client
        /// </summary>
        private async Task ReadDataFromStreamAsync()
        {
            try
            {
                byte[] readBuffer = new byte[1024];

                // Only accept incoming packets while we are not closing
                while (!_isClosed && !_isClosing)
                {
                    int bytesReceived = await client.GetStream().ReadAsync(readBuffer);
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
                // Allow packets to get written to the socket before closing down
                while (!_isClosed)
                {
                    // Wait for the packet queue to actually have something to send.
                    await _writeSemaphore.WaitAsync();

                    // The socket may have closed why we have waited for the next packet in queue
                    if (_isClosed)
                        return;

                    while (_serverPacketQueue.TryDequeue(out ServerPacket? packet))
                    {
                        byte[] payload = packet.Write().GetRawPacket();
                        byte[]? header = BuildPacketHeader(payload, packet.Cmd);

                        if (header != null)
                            await client.GetStream().WriteAsync(header);

                        if (packet.Cmd != 0)
                            Console.WriteLine($"[{GetType().Name}] Sending {(ServerOpcode)packet.Cmd}");

                        await client.GetStream().WriteAsync(payload);
                    }

                    // If all remaining packets have been sent out, we may finally close the socket
                    if (_isClosing)
                        Close();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Close();
            }
        }

        /// <summary>
        /// Adds the ServerPacket to the socket's internal packet queue to send it to the client as soon as possible.
        /// </summary>
        public void EnqueuePacket(ServerPacket packet)
        {
            // The the communication has been suspended
            if (_isCommsSuspended)
                return;

            _serverPacketQueue.Enqueue(packet);
            _writeSemaphore.Release();
        }

        public abstract BaseSession CreateSession();
        public abstract void DataReceived(byte[] data, int dataLength);
        protected virtual byte[]? BuildPacketHeader(byte[] payload, int cmd) { return null; }
    }
}

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
        private enum SocketState
        {
            Open        = 0,
            Closed      = 1,
            Closing     = 2,
            Suspending  = 3,
            Suspended   = 4
        }

        public BaseSession? Session { get; private set; }
        private readonly SemaphoreSlim _writeSemaphore = new(0);
        private readonly CancellationTokenSource _writeCancellationTokenSource = new();
        private readonly CancellationTokenSource _readCancellationTokenSource = new();

        private readonly ConcurrentQueue<ServerPacket> _serverPacketQueue = new();
        private SocketState _state = SocketState.Closed;
        private Task? _readTask;
        private Task? _writeTask;

        /// <summary>
        /// Initializes the stream reading task so it can received packets from the client
        /// </summary>
        public void Open()
        {
            if (_state == SocketState.Open)
                return;

            Console.WriteLine($"[{GetType().Name}] Started socket on endpoint {client.Client.LocalEndPoint} for client {client.Client.RemoteEndPoint}");

            _state = SocketState.Open;
            _readTask = ReadDataFromStreamAsync();
            _writeTask = WriteDataToStreamAsync();
        }

        /// <summary>
        /// Closes the socket and requests that the Tcp stream will be closed as well.
        /// </summary>
        public void Close()
        {
            if (_state == SocketState.Closed)
                return;

            _readCancellationTokenSource.Cancel();
            _writeCancellationTokenSource.Cancel();

            if (_readTask == null || _writeTask == null)
                throw new Exception("One of the stream tasks in BaseSocket has been null, which most not happen at this point!");

            Task.WhenAll(_readTask, _writeTask).Wait();

            Console.WriteLine($"[{GetType().Name}] Closed socket for client {client.Client.RemoteEndPoint}");
            client.Close();
            Session?.Close();
        }

        /// <summary>
        /// Closes the socket after all remaining server packets have been sent out.
        /// </summary>
        public void DelayedClose()
        {
            if (_state == SocketState.Closed || _state == SocketState.Closing)
                return;

            _state = SocketState.Closing;

            if (_serverPacketQueue.IsEmpty)
                Close();
        }

        /// <summary>
        /// Suspends the server-to-client communication after processing remaining packets, which means it will no longer receive any packets from the server
        /// </summary>
        public void DelayedSuspendComms()
        {
            if (_state == SocketState.Suspending)
                return;

            Console.WriteLine($"[{GetType().Name}] DelayedSuspendComms invoked");

            _state = SocketState.Suspending;
        }

        /// <summary>
        /// Suspends the server-to-client communication, which means it will no longer receive any packets from the server
        /// </summary>
        private void SuspendComms()
        {
            if (_state != SocketState.Suspending || _state == SocketState.Suspended)
                return;

            Console.WriteLine($"[{GetType().Name}] SuspendComms invoked");

            // We send the packet before setting the boolean so we can squeeze that one last packed through
            Session?.SendSuspendComms();
            _state = SocketState.Suspended;
        }

        /// <summary>
        /// Reads buffered data from the Tcp stream which has been sent by the client
        /// </summary>
        private async Task ReadDataFromStreamAsync()
        {
            try
            {
                byte[] readBuffer = new byte[4096];

                // Only accept incoming packets while we are not closing
                while (_state != SocketState.Closing && !_readCancellationTokenSource.IsCancellationRequested)
                {
                    int bytesReceived = await client.GetStream().ReadAsync(readBuffer, _readCancellationTokenSource.Token);
                    // Receiving 0 bytes means that the client has been closed or lost its connection
                    if (bytesReceived == 0)
                    {
                        Close();
                        return;
                    }

                    // Create a session when we receive our first data
                    if (Session == null)
                        Session = CreateSession();

                    // Extract the packets from the streamed data and handle them
                    Task[]? packetHandlerTasks = HandlePackets(readBuffer, bytesReceived);
                    if (packetHandlerTasks != null)
                        await Task.WhenAll(packetHandlerTasks);

                    // When we are about to suspend the communication of the client, we have to make sure all packets are processed
                    if (_state == SocketState.Suspending)
                        SuspendComms();
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
                while (!_writeCancellationTokenSource.IsCancellationRequested)
                {
                    // Wait for the packet queue to actually have something to send.
                    await _writeSemaphore.WaitAsync(_writeCancellationTokenSource.Token);

                    while (!_serverPacketQueue.IsEmpty)
                    {
                        if (_serverPacketQueue.TryDequeue(out ServerPacket? packet))
                        {
                            //if (packet.Cmd != 0)
                            Console.WriteLine($"[{GetType().Name}] Sending {(ServerOpcode)packet.Cmd}");

                            byte[] payload = packet.Write().GetRawPacket();
                            byte[]? header = BuildPacketHeader(payload, packet.Cmd);

                            if (header != null)
                            {
                                byte[] sendBuffer = new byte[header.Length + payload.Length];
                                Buffer.BlockCopy(header, 0, sendBuffer, 0, header.Length);
                                Buffer.BlockCopy(payload, 0, sendBuffer, header.Length, payload.Length);
                                await client.GetStream().WriteAsync(sendBuffer, _writeCancellationTokenSource.Token);
                            }
                            else
                                await client.GetStream().WriteAsync(payload, _writeCancellationTokenSource.Token);
                        }
                    }

                    // If all remaining packets have been sent out, we may finally close the socket
                    if (_state == SocketState.Closing)
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
            if (_state == SocketState.Closed || _state == SocketState.Suspended)
                return;

            _serverPacketQueue.Enqueue(packet);
            _writeSemaphore.Release();
        }

        protected abstract BaseSession CreateSession();
        protected virtual Task[]? HandlePackets(byte[] data, int dataLength) { return null; }
        protected virtual byte[]? BuildPacketHeader(byte[] payload, int cmd) { return null; }
    }
}

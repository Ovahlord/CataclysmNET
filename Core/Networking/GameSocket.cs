using Packets;
using System.Net.Sockets;
using System;
using System.Buffers;
using Packets.GamePackets;
using System.Buffers.Binary;
using System.Reflection.Emit;
using Core.Cryptography;

namespace Core.Networking
{

    /// <summary>
    /// A special derived base socket which serves as base class for Realms and Worlds.
    /// This class performs additional packet de/encryption which is required for game connections.
    /// </summary>
    public abstract class GameSocket(TcpClient client) : BaseSocket(client)
    {
        private static readonly string _serverConnectionInitialize = "WORLD OF WARCRAFT CONNECTION - SERVER TO CLIENT";
        private static readonly string _clientConnectionInitialize = "WORLD OF WARCRAFT CONNECTION - CLIENT TO SERVER";

        private PacketCrypt? _packetCrypt = null;
        private bool _connectionInitialized = false;

        byte[]? header = null;
        int headerBytesReceived = 0;

        byte[]? payload = null;
        int payloadBytesReceived = 0;

        int cmd = 0;
        private int HeaderSize { get { return _connectionInitialized ? 6 : 2; } }

        public override void DataReceived(byte[] data, int dataLength)
        {
            int processedBytes = 0;

            while (!_cancellationTokenSource.IsCancellationRequested && processedBytes < dataLength)
            {
                if (header == null)
                {
                    header = new byte[HeaderSize];
                    headerBytesReceived = 0;
                }

                // Extract the header from the streamed buffer
                if (header.Length != headerBytesReceived)
                {
                    int remainingHeaderBytes = header.Length - headerBytesReceived;
                    int bytesToRead = Math.Min(remainingHeaderBytes, dataLength - processedBytes);
                    Buffer.BlockCopy(data, processedBytes, header, headerBytesReceived, bytesToRead);
                    headerBytesReceived += bytesToRead;
                    processedBytes += bytesToRead;
                }

                // Payload has been not been fully read yet but we ran out of bytes to read. Wait for the next stream input
                if (header.Length != headerBytesReceived)
                    return;

                // We will now extract the packet size and opcode from the header and initialize the payload buffer
                if (payload == null)
                {
                    payloadBytesReceived = 0;
                    ReadHeader(out int payloadSize, out cmd);
                    payload = new byte[payloadSize];
                }

                Console.WriteLine($"[{GetType().Name}] extracted payload size ({payload.Length}) and opcode ({cmd}) from header");

                if (payload.Length != payloadBytesReceived)
                {
                    int remainingPayloadBytes = payload.Length - payloadBytesReceived;
                    int bytesToRead = Math.Min(remainingPayloadBytes, dataLength - processedBytes);
                    Buffer.BlockCopy(data, processedBytes, payload, payloadBytesReceived, bytesToRead);
                    payloadBytesReceived += bytesToRead;
                    processedBytes += bytesToRead;
                }

                // Payload has been not been fully read yet but we ran out of bytes to read. Wait for the next stream input
                if (payload.Length != payloadBytesReceived)
                    return;

                // We expect the first packet to initialize the connection.
                if (!_connectionInitialized)
                {
                    if (!ReadConnectionInitialize(payload))
                    {
                        Close();
                        return;
                    }

                    header = null;
                    payload = null;
                    continue;
                }


                Session?.HandlePacket(cmd, payload);
                header = null;
                payload = null;
            }
        }

        public override async Task SendPacketAsync(ServerPacket packet)
        {
            byte[] payload = packet.Write().GetRawPacket();
            byte[] header = ServerPacket.BuildHeader(payload.Length + (_connectionInitialized ? 2 : 0), packet.Cmd);

            // Encrypt the header when the session key has been validated
            _packetCrypt?.EncryptSend(header);

            byte[] packetData = new byte[payload.Length + header.Length];
            Buffer.BlockCopy(header, 0, packetData, 0, header.Length);
            Buffer.BlockCopy(payload, 0, packetData, header.Length, payload.Length);

            await WriteDataToStreamAsync(packetData);
        }

        public void SendConnectionInitialize()
        {
            ServerConnectionInitialize packet = new()
            {
                ConnectionInitialize = _serverConnectionInitialize
            };

            Task.Run(() => SendPacketAsync(packet), _cancellationTokenSource.Token);
        }

        public void InitializePacketCrypt(byte[] sessionKey)
        {
            _packetCrypt = new(sessionKey);
        }

        private bool ReadConnectionInitialize(ClientConnectionInitialize connectionInitialize)
        {
            if (connectionInitialize.ConnectionInitialize != _clientConnectionInitialize)
                return false;

            _connectionInitialized = true;

            // Connection has been established. Time to challenge the client to proof its identity
            if (Session is GameSession session)
                session.SendAuthChallenge();

            return true;
        }

        private void ReadHeader(out int payloadSize, out int cmd)
        {
            // Invalid Header
            if (header == null || header.Length < HeaderSize)
            {
                payloadSize = 0;
                cmd = -1;
                return;
            }

            // Decrypt the header when the session key has been validated
            _packetCrypt?.DecryptRecv(header);

            Array.Reverse(header, 0, 2);
            ReadOnlySpan<byte> sizeSpan = new(header, 0, 2);
            payloadSize = BinaryPrimitives.ReadUInt16LittleEndian(sizeSpan);

            // Validate allowed payload size range
            if (payloadSize < 4 || payloadSize > 10240)
            {
                cmd = -1; // -1 marks the packet as invalid.
                return;
            }

            // If the connection has not been initialized, we don't have to extract an opcode yet.
            if (!_connectionInitialized)
            {
                cmd = 0;
                return;
            }

            ReadOnlySpan<byte> cmdSpan = new(header, 2, 4);
            cmd = (int)BinaryPrimitives.ReadUInt32LittleEndian(cmdSpan);

            // Validate Cmd range
            if (cmd < 0 || cmd > 0x7FFF)
            {
                cmd = -1; // -1 marks the packet as invalid.
                return;
            }

            payloadSize -= sizeof(uint); // Subtracting the Cmd size
        }
    }
}

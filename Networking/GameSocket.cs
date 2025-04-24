using Packets;
using System.Net.Sockets;
using System;
using System.Buffers;

namespace Networking
{
    /// <summary>
    /// A special derived base socket which serves as base class for Realms and Worlds.
    /// This class performs additional packet de/encryption which is required for game connections.
    /// </summary>
    public abstract class GameSocket(TcpClient client) : BaseSocket(client)
    {
        private PacketCrypt? _packetCrypt = null;
        private bool _connectionInitialized = false;

        public override void DataReceived(byte[] data, int dataLength)
        {
            throw new NotImplementedException();
        }

        public override async Task SendPacketAsync(ServerPacket packet)
        {
            byte[] payload = packet.Write().GetRawPacket();
            byte[] header = ServerPacket.BuildHeader(payload.Length, packet.Cmd);

            if (_packetCrypt != null && _packetCrypt.Initialized)
                _packetCrypt.EncryptSend(header);

            byte[] packetData = new byte[payload.Length + header.Length];
            Buffer.BlockCopy(header, 0, packetData, 0, header.Length);
            Buffer.BlockCopy(payload, 0, packetData, header.Length, payload.Length);

            await WriteDataToStreamAsync(packetData);
        }

        public void SendConnectionInitialize()
        {

        }
    }
}

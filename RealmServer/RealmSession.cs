using Networking;
using Packets;
using System;

namespace RealmServer
{
    public sealed class RealmSession(BaseSocket socket) : BaseSession(socket)
    {
        public override void HandlePacket(int opcode, byte[] payload)
        {
            throw new NotImplementedException();
        }

        public override void SendPacket(ServerPacket packet)
        {
            try
            {
                ReadOnlyMemory<byte> packetBuffer = packet.Write().GetRawPacket();
                ReadOnlyMemory<byte> headerBuffer = ServerPacket.BuildHeader(packetBuffer.Length, packet.Cmd);
            }
            catch (OperationCanceledException) { }
            catch (Exception) { throw; }
        }
    }
}

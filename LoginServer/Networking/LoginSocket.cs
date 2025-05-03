using System.Net.Sockets;
using Core.Networking;
using Core.Packets;
using Core.Packets.Opcodes;

namespace LoginServer.Networking
{
    public sealed class LoginSocket(TcpClient client) : BaseSocket(client)
    {
        protected override BaseSession CreateSession()
        {
            return new LoginSession(this);
        }

        protected override Task[]? HandlePackets(byte[] data, int dataLength)
        {
            // We don't accept empty or too big packets.
            if (dataLength == 0 || dataLength > 100)
                return null;

            // Login packets are not split over multiple stream messages and instead arrive as one full packet.
            LoginOpcode opcode = (LoginOpcode)data[0];
            ReadOnlySpan<byte> span = new(data, 1, dataLength - 1);


            Task? task = Session?.HandlePacket(data[0], span.ToArray());
            if (task == null)
                return null;

            return [task];
        }
    }
}

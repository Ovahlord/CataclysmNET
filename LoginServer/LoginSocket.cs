using Networking;
using Packets.Opcodes;
using System.Net.Sockets;

namespace LoginServer
{
    public sealed class LoginSocket(TcpClient client) : BaseSocket(client)
    {
        public override BaseSession CreateSession()
        {
            return new LoginSession(this);
        }

        public override void DataReceived(byte[] data, int dataLength)
        {
            // We don't accept empty or too big packets.
            if (dataLength == 0 || dataLength > 100)
                return;

            // Login packets are not split over multiple stream messages and instead arrive as one full packet.
            LoginOpcode opcode = (LoginOpcode)data[0];
            ReadOnlySpan<byte> span = new(data, 1, dataLength - 1);
            Session?.HandlePacket((int)opcode, span.ToArray());
        }
    }
}

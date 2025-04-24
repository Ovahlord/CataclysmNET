using Networking;
using Packets;
using System.Net.Sockets;
using System.Threading;

namespace RealmServer
{
    public sealed class RealmSocket(TcpClient client) : GameSocket(client)
    {
        public override BaseSession CreateSession()
        {
            return new RealmSession(this);
        }
    }
}

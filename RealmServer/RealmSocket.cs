using Networking;
using System.Net.Sockets;

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

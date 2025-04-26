using System.Net.Sockets;
using Core.Networking;

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

using System.Net.Sockets;
using Core.Networking;
using Game.Networking;

namespace RealmInstance
{
    public sealed class RealmSocket(TcpClient client) : GameSocket(client)
    {
        public override BaseSession CreateSession()
        {
            return new RealmSession(this);
        }
    }
}

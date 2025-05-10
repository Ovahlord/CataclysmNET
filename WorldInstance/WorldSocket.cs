using System.Net.Sockets;
using Core.Networking;
using Game.Networking;

namespace WorldInstance
{
    public sealed class WorldSocket(TcpClient client) : GameSocket(client)
    {
        protected override BaseSession CreateSession()
        {
            return new WorldSession(this);
        }
    }
}

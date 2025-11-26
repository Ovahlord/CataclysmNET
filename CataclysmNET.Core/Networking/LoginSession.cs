using System.Net.Sockets;

namespace CataclysmNET.Core.Networking
{
    public class LoginSession : BaseSession
    {
        public LoginSession(TcpClient tcpClient) : base(tcpClient)
        {
        }

        public override async Task<bool> OpenAsync(CancellationToken cancellationToken)
        {
            return false;
        }

        public override async Task CloseAsync(CancellationToken cancellationToken)
        {
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace CataclysmNET.Core.Networking
{
    public class ConnectionListener<TSession>(IPEndPoint endPoint) where TSession : BaseSession
    {
        private const int FirstReceiveTimeoutLimit = 5000;
        private readonly TcpListener _tcpListener = new(endPoint);
        private CancellationTokenSource? _cancellationTokenSource;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _tcpListener.Start();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
                // We grant the client a short amount of time to send its first message before rejecting it
                tcpClient.ReceiveTimeout = FirstReceiveTimeoutLimit;

                // We try to create and open a session for the tcp client
                if (TryCreateSession(tcpClient, out TSession? session))
                {
                    if (!await session.OpenAsync(_cancellationTokenSource.Token))
                        session.Dispose();
                    else
                    {

                    }
                }
                else // if the creation failed for whatever reason, we dispose it right away to reclaim resources
                    tcpClient.Dispose();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || _cancellationTokenSource == null)
                return;

            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            _tcpListener.Stop();

            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        }

        private static bool TryCreateSession(TcpClient tcpClient, [NotNullWhen(true)] out TSession? session)
        {
            session = null;

            if (typeof(TSession) == typeof(LoginSession))
            {
                if (new LoginSession(tcpClient) is TSession instance)
                    session = instance;
            }

            return session != null;
        }
    }
}

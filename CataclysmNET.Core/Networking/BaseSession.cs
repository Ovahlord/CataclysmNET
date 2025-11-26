using System.Net.Sockets;

namespace CataclysmNET.Core.Networking
{
    public abstract class BaseSession : IDisposable
    {
        protected readonly TcpClient _tcpClient;

        protected BaseSession(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        ~BaseSession()
        {
            Dispose(false);
        }

        public abstract Task<bool> OpenAsync(CancellationToken cancellationToken);
        public abstract Task CloseAsync(CancellationToken cancellationToken);

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _tcpClient.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

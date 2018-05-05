
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using static Magma.Transport.Tcp.Transport.EstablishedSocket;

namespace Magma.Transport.Tcp.Transport
{
    public sealed class ListeningSocket : Socket
    {
        private object _acceptLock = new object();
        private Queue<AcceptedSocket> _backlog;
        private CancellationToken _cancellationToken;
        private Task _listeningTask;

        internal ListeningSocket(IPEndPoint endPoint, int backlog, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _backlog = new Queue<AcceptedSocket>(backlog);
        }

        public ValueTask ListenAsync()
        {
            _listeningTask = Task.Run(() => RunListenLoopAsync());
            return default;
        }

        public ValueTask<EstablishedSocket> AcceptAsync()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            ValueTask<EstablishedSocket> acceptTask;
            lock (_acceptLock)
            {
                if (_backlog.Count > 0)
                {
                    acceptTask = new ValueTask<EstablishedSocket>(_backlog.Dequeue());
                }
                else
                {
                    acceptTask = GetNextAvailableAsync();
                }
            }

            return acceptTask;
        }

        ValueTask<EstablishedSocket> GetNextAvailableAsync() => default;

        Task RunListenLoopAsync()
        {
            return Task.CompletedTask;
        }
    }
}


using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Magma.Transport.Tcp.Internal;
using Magma.Transport.Tcp.SocketStates;
using static Magma.Transport.Tcp.Transport.SocketConnection;

namespace Magma.Transport.Tcp.Transport
{
    public sealed class ListeningSocket : Socket
    {
        private object _acceptLock = new object();
        private readonly int _maxBacklog;
        private Queue<AcceptedSocket> _backlog;
        private CancellationToken _cancellationToken;
        private ResponderListening _listener;
        private Task _listeningTask;

        private readonly PipeScheduler _scheduler;
        private readonly ISocketsTrace _trace;
        public MemoryPool<byte> MemoryPool { get; }

        internal ListeningSocket(IPEndPoint endPoint, int backlog, MemoryPool<byte> memoryPool, PipeScheduler scheduler, ISocketsTrace trace, CancellationToken cancellationToken)
        {
            _maxBacklog = backlog;
            _cancellationToken = cancellationToken;
            _backlog = new Queue<AcceptedSocket>(backlog);
            MemoryPool = memoryPool;
            _scheduler = scheduler;
            _trace = trace;
        }

        public async ValueTask ListenAsync()
        {
            var state = new ResponderClosed();
            _listener = await state.ListenAsync();
            _listeningTask = Task.Run(() => RunListenLoopAsync());
        }

        public ValueTask<SocketConnection> AcceptAsync()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            ValueTask<SocketConnection> acceptTask;
            lock (_acceptLock)
            {
#if NETCOREAPP2_1
                if (_backlog.TryDequeue(out var socket))
                {
                    acceptTask = new ValueTask<SocketConnection>(socket);
                }
#else
                if (_backlog.Count > 0)
                {
                    acceptTask = new ValueTask<SocketConnection>(_backlog.Dequeue());
                }
#endif
                else
                {
                    acceptTask = GetNextAvailableAsync();
                }
            }

            return acceptTask;
        }

        ValueTask<SocketConnection> GetNextAvailableAsync() => default;

        async Task RunListenLoopAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var incoming = await _listener.WaitSynAsync();
#if NETCOREAPP2_1
                ThreadPool.QueueUserWorkItem(state => _ = state.socket.ConnectAsync(state.incoming), (socket: this, incoming), preferLocal: true);
#else
                ThreadPool.QueueUserWorkItem(state => ((ListeningSocket)state).ConnectAsync(incoming), this);
#endif
            }
        }

        private async ValueTask ConnectAsync(ResponderSynReceived incoming)
        {
            // TODO: Waiting accepts
            var shouldAccept = false;
            lock (_acceptLock)
            {
                if (_backlog.Count < _maxBacklog)
                {
                    shouldAccept = true;
                }
            }

            if (shouldAccept)
            {
                var connection = await incoming.SendSynAckAsync();
                lock (_acceptLock)
                {
                    if (_backlog.Count == _maxBacklog)
                    {
                        shouldAccept = false;
                    }
                    else
                    {
                        _backlog.Enqueue(new AcceptedSocket(connection, MemoryPool, _scheduler, _trace));
                    }
                }
            }
            
            if (!shouldAccept)
            {
                await incoming.SendRstAsync();
            }
        }
    }
}

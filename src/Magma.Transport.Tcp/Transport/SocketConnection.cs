using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using Magma.Transport.Tcp.Internal;
using Magma.Transport.Tcp.SocketStates;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.Extensions.Logging;

namespace Magma.Transport.Tcp.Transport
{
    public abstract class SocketConnection : TransportConnection
    {
        private static readonly int MinAllocBufferSize = 2048; // KestrelMemoryPool.MinimumSegmentSize / 2;

        private readonly PipeScheduler _scheduler;
        private readonly ISocketsTrace _trace;
        protected SocketReceiver Receiver { get; set; }
        protected SocketSender Sender { get; set; }

        private volatile bool _aborted;

        private protected SocketConnection(MemoryPool<byte> memoryPool, PipeScheduler scheduler, ISocketsTrace trace)
        {
            Debug.Assert(memoryPool != null);
            Debug.Assert(trace != null);

            MemoryPool = memoryPool;
            _scheduler = scheduler;
            _trace = trace;
        }

        public override MemoryPool<byte> MemoryPool { get; }
        public override PipeScheduler InputWriterScheduler => _scheduler;
        public override PipeScheduler OutputReaderScheduler => _scheduler;

        public async Task StartAsync()
        {
            Exception sendError = null;
            try
            {
                // Spawn send and receive logic
                var receiveTask = DoReceive();
                var sendTask = DoSend();

                // If the sending task completes then close the receive
                // We don't need to do this in the other direction because the kestrel
                // will trigger the output closing once the input is complete.
                if (await Task.WhenAny(receiveTask, sendTask) == sendTask)
                {
                    //// Tell the reader it's being aborted
                    //_socket.Dispose();
                }

                // Now wait for both to complete
                await receiveTask;
                sendError = await sendTask;

                //// Dispose the socket(should noop if already called)
                //_socket.Dispose();
                Receiver.Dispose();
                Sender.Dispose();
            }
            catch (Exception ex)
            {
                _trace.LogError(0, ex, $"Unexpected exception in {nameof(SocketConnection)}.{nameof(StartAsync)}.");
            }
            finally
            {
                // Complete the output after disposing the socket
                Output.Complete(sendError);
            }
        }

        private async Task DoReceive()
        {
            Exception error = null;

            try
            {
                await ProcessReceives();
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                error = new ConnectionResetException(ex.Message, ex);
                _trace.ConnectionReset(ConnectionId);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted ||
                                             ex.SocketErrorCode == SocketError.ConnectionAborted ||
                                             ex.SocketErrorCode == SocketError.Interrupted ||
                                             ex.SocketErrorCode == SocketError.InvalidArgument)
            {
                if (!_aborted)
                {
                    // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
                    error = new ConnectionAbortedException();
                    _trace.ConnectionError(ConnectionId, error);
                }
            }
            catch (ObjectDisposedException)
            {
                if (!_aborted)
                {
                    error = new ConnectionAbortedException();
                    _trace.ConnectionError(ConnectionId, error);
                }
            }
            catch (IOException ex)
            {
                error = ex;
                _trace.ConnectionError(ConnectionId, error);
            }
            catch (Exception ex)
            {
                error = new IOException(ex.Message, ex);
                _trace.ConnectionError(ConnectionId, error);
            }
            finally
            {
                if (_aborted)
                {
                    error = error ?? new ConnectionAbortedException();
                }

                Input.Complete(error);
            }
        }

        private async Task ProcessReceives()
        {
            while (true)
            {
                // Ensure we have some reasonable amount of buffer space
                var buffer = Input.GetMemory(MinAllocBufferSize);

                var bytesReceived = await Receiver.ReceiveAsync(buffer);

                if (bytesReceived == 0)
                {
                    // FIN
                    _trace.ConnectionReadFin(ConnectionId);
                    break;
                }

                Input.Advance(bytesReceived);

                var flushTask = Input.FlushAsync();

                if (!flushTask.IsCompleted)
                {
                    _trace.ConnectionPause(ConnectionId);

                    await flushTask;

                    _trace.ConnectionResume(ConnectionId);
                }

                var result = flushTask.GetAwaiter().GetResult();
                if (result.IsCompleted)
                {
                    // Pipe consumer is shut down, do we stop writing
                    break;
                }
            }
        }

        private async Task<Exception> DoSend()
        {
            Exception error = null;

            try
            {
                await ProcessSends();
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                error = null;
            }
            catch (ObjectDisposedException)
            {
                error = null;
            }
            catch (IOException ex)
            {
                error = ex;
            }
            catch (Exception ex)
            {
                error = new IOException(ex.Message, ex);
            }
            finally
            {
                // Make sure to close the connection only after the _aborted flag is set.
                // Without this, the RequestsCanBeAbortedMidRead test will sometimes fail when
                // a BadHttpRequestException is thrown instead of a TaskCanceledException.
                _aborted = true;
                _trace.ConnectionWriteFin(ConnectionId);
                //_socket.Shutdown(SocketShutdown.Both);
            }

            return error;
        }

        private async Task ProcessSends()
        {
            while (true)
            {
                // Wait for data to write from the pipe producer
                var result = await Output.ReadAsync();
                var buffer = result.Buffer;

                if (result.IsCanceled)
                {
                    break;
                }

                var end = buffer.End;
                var isCompleted = result.IsCompleted;
                if (!buffer.IsEmpty)
                {
                    await Sender.SendAsync(buffer);
                }

                Output.AdvanceTo(end);

                if (isCompleted)
                {
                    break;
                }
            }
        }


        // Accepted and Connect Sockets have different shutdown sequences; so two derived classes
        internal protected sealed class ConnectedSocket : SocketConnection
        {
            private InitatorEstablished _connection;

            internal ConnectedSocket(InitatorEstablished connection, MemoryPool<byte> memoryPool, PipeScheduler scheduler, ISocketsTrace trace)
            : base(memoryPool, scheduler, trace)
            {
                _connection = connection;

                var localEndPoint = connection.LocalEndPoint;
                var remoteEndPoint = connection.RemoteEndPoint;

                LocalAddress = localEndPoint.Address;
                LocalPort = localEndPoint.Port;

                RemoteAddress = remoteEndPoint.Address;
                RemotePort = remoteEndPoint.Port;

                //// On *nix platforms, Sockets already dispatches to the ThreadPool.
                //var awaiterScheduler = IsWindows ? _scheduler : PipeScheduler.Inline;

                Receiver = new SocketReceiver(this, scheduler);
                Sender = new SocketSender(this, scheduler);
            }
        }

        // Accepted and Connect Sockets have different shutdown sequences; so two derived classes
        internal protected sealed class AcceptedSocket : SocketConnection
        {
            private ResponderEstablished _connection;

            internal AcceptedSocket(ResponderEstablished connection, MemoryPool<byte> memoryPool, PipeScheduler scheduler, ISocketsTrace trace)
            : base(memoryPool, scheduler, trace)
            {
                _connection = connection;

                var localEndPoint = connection.LocalEndPoint;
                var remoteEndPoint = connection.RemoteEndPoint;

                LocalAddress = localEndPoint.Address;
                LocalPort = localEndPoint.Port;

                RemoteAddress = remoteEndPoint.Address;
                RemotePort = remoteEndPoint.Port;

                //// On *nix platforms, Sockets already dispatches to the ThreadPool.
                //var awaiterScheduler = IsWindows ? _scheduler : PipeScheduler.Inline;

                Receiver = new SocketReceiver(this, scheduler);
                Sender = new SocketSender(this, scheduler);
            }
        }
    }
}

using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Magma.Transport.Tcp.Internal;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.Extensions.Logging;

namespace Magma.Transport.Tcp
{
    using Magma.Transport.Tcp.Transport;

    internal sealed class SocketTransport : ITransport
    {
        private MemoryPool<byte> _memoryPool;
        private IEndPointInformation _endPointInformation;
        private IConnectionDispatcher _dispatcher;
        private IApplicationLifetime _appLifetime;
        private readonly ISocketsTrace _trace;
        private ListeningSocket _listenSocket;
        private Task _listenTask;
        private Exception _listenException;
        private volatile bool _unbinding;

        private int _numSchedulers;

        public SocketTransport(IEndPointInformation endPointInformation, IConnectionDispatcher dispatcher, IApplicationLifetime applicationLifetime, int ioQueueCount, SocketsTrace trace, MemoryPool<byte> memoryPool)
        {
            Debug.Assert(endPointInformation != null);
            Debug.Assert(endPointInformation.Type == ListenType.IPEndPoint);
            Debug.Assert(dispatcher != null);
            Debug.Assert(applicationLifetime != null);
            Debug.Assert(trace != null);

            _endPointInformation = endPointInformation;
            _dispatcher = dispatcher;
            _appLifetime = applicationLifetime;
            _trace = trace;
            _memoryPool = memoryPool;

            //if (ioQueueCount > 0)
            //{
            //    _numSchedulers = ioQueueCount;
            //    _schedulers = new IOQueue[_numSchedulers];

            //    for (var i = 0; i < _numSchedulers; i++)
            //    {
            //        _schedulers[i] = new IOQueue();
            //    }
            //}
            //else
            //{
            //    _numSchedulers = ThreadPoolSchedulerArray.Length;
            //    _schedulers = ThreadPoolSchedulerArray;
            //}
        }

        public async Task BindAsync()
        {
            if (_listenSocket != null)
            {
                throw new InvalidOperationException(SocketsStrings.TransportAlreadyBound);
            }

            var endPoint = _endPointInformation.IPEndPoint;

            var listenSocket = await Socket.ListenAsync(endPoint, backlog: 512, _appLifetime.ApplicationStopping);

            //EnableRebinding(listenSocket);

            //// Kestrel expects IPv6Any to bind to both IPv6 and IPv4
            //if (endPoint.Address == IPAddress.IPv6Any)
            //{
            //    listenSocket.DualMode = true;
            //}

            //try
            //{
            //    listenSocket.Bind(endPoint);
            //}
            //catch (SocketException e) when (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
            //{
            //    throw new AddressInUseException(e.Message, e);
            //}

            //// If requested port was "0", replace with assigned dynamic port.
            //if (_endPointInformation.IPEndPoint.Port == 0)
            //{
            //    _endPointInformation.IPEndPoint = (IPEndPoint)listenSocket.LocalEndPoint;
            //}

            _listenSocket = listenSocket;

            _listenTask = Task.Run(() => RunAcceptLoopAsync());
        }

        public async Task UnbindAsync()
        {
            if (_listenSocket != null)
            {
                _unbinding = true;
                _listenSocket.Dispose();

                Debug.Assert(_listenTask != null);
                await _listenTask.ConfigureAwait(false);

                _unbinding = false;
                _listenSocket = null;
                _listenTask = null;

                if (_listenException != null)
                {
                    var exInfo = ExceptionDispatchInfo.Capture(_listenException);
                    _listenException = null;
                    exInfo.Throw();
                }
            }
        }

        public Task StopAsync()
        {
            _memoryPool.Dispose();
            return Task.CompletedTask;
        }

        private async Task RunAcceptLoopAsync()
        {
            try
            {
                while (true)
                {
                    for (var schedulerIndex = 0; schedulerIndex < _numSchedulers; schedulerIndex++)
                    {
                        //try
                        //{
                        //    var acceptSocket = await _listenSocket.AcceptAsync();
                        //    //acceptSocket.NoDelay = _endPointInformation.NoDelay;

                        //    var connection = new SocketConnection(acceptSocket, _memoryPool, _schedulers[schedulerIndex], _trace);

                        //    _dispatcher.OnConnection(connection);

                        //    _ = connection.StartAsync();
                        //}
                        //catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
                        //{
                        //    // REVIEW: Should there be a separate log message for a connection reset this early?
                        //    _trace.ConnectionReset(connectionId: "(null)");
                        //}
                        //catch (SocketException ex) when (!_unbinding)
                        //{
                        //    _trace.ConnectionError(connectionId: "(null)", ex);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                if (_unbinding)
                {
                    // Means we must be unbinding. Eat the exception.
                }
                else
                {
                    _trace.LogCritical(ex, $"Unexpected exeption in {nameof(SocketTransport)}.{nameof(RunAcceptLoopAsync)}.");
                    _listenException = ex;

                    // Request shutdown so we can rethrow this exception
                    // in Stop which should be observable.
                    _appLifetime.StopApplication();
                }
            }
        }

        //[DllImport("libc", SetLastError = true)]
        //private static extern int setsockopt(int socket, int level, int option_name, IntPtr option_value, uint option_len);

        //private const int SOL_SOCKET_OSX = 0xffff;
        //private const int SO_REUSEADDR_OSX = 0x0004;
        //private const int SOL_SOCKET_LINUX = 0x0001;
        //private const int SO_REUSEADDR_LINUX = 0x0002;

        //// Without setting SO_REUSEADDR on macOS and Linux, binding to a recently used endpoint can fail.
        //// https://github.com/dotnet/corefx/issues/24562
        //private unsafe void EnableRebinding(Socket listenSocket)
        //{
        //    var optionValue = 1;
        //    var setsockoptStatus = 0;

        //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        //    {
        //        setsockoptStatus = setsockopt(listenSocket.Handle.ToInt32(), SOL_SOCKET_LINUX, SO_REUSEADDR_LINUX,
        //                                      (IntPtr)(&optionValue), sizeof(int));
        //    }
        //    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        //    {
        //        setsockoptStatus = setsockopt(listenSocket.Handle.ToInt32(), SOL_SOCKET_OSX, SO_REUSEADDR_OSX,
        //                                      (IntPtr)(&optionValue), sizeof(int));
        //    }

        //    if (setsockoptStatus != 0)
        //    {
        //        _trace.LogInformation("Setting SO_REUSEADDR failed with errno '{errno}'.", Marshal.GetLastWin32Error());
        //    }
        //}
    }
}

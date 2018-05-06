using System;
using System.IO.Pipelines;
using Magma.Transport.Tcp.Transport;

namespace Magma.Transport.Tcp.Internal
{
    public class SocketReceiver : IDisposable
    {
        private readonly SocketConnection _socket;
        //private readonly SocketAsyncEventArgs _eventArgs = new SocketAsyncEventArgs();
        private readonly SocketAwaitable _awaitable;

        public SocketReceiver(SocketConnection socket, PipeScheduler scheduler)
        {
            _socket = socket;
            _awaitable = new SocketAwaitable(scheduler);
            //_eventArgs.UserToken = _awaitable;
            //_eventArgs.Completed += (_, e) => ((SocketAwaitable)e.UserToken).Complete(e.BytesTransferred, e.SocketError);
        }

        public SocketAwaitable ReceiveAsync(Memory<byte> buffer)
        {
//#if NETCOREAPP2_1
//            _eventArgs.SetBuffer(buffer);
//#else
//            var segment = buffer.GetArray();

//            _eventArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
//#endif
//            if (!_socket.ReceiveAsync(_eventArgs))
//            {
//                _awaitable.Complete(_eventArgs.BytesTransferred, _eventArgs.SocketError);
//            }

            return _awaitable;
        }

        public void Dispose()
        {
            //_eventArgs.Dispose();
        }
    }
}

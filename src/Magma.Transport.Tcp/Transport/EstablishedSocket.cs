using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Transport.Tcp.Transport
{
    public abstract class EstablishedSocket : Socket
    {
        // Send
        // Receive


        internal protected sealed class ConnectedSocket : EstablishedSocket
        {
        }

        internal protected sealed class AcceptedSocket : EstablishedSocket
        {

        }
    }
}

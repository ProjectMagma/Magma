using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Magma.NetMap.TcpHost
{
    public class TestConnectionDispatcher : IConnectionDispatcher
    {
        public void OnConnection(TransportConnection connection)
        {
            var ignore = ReadAndRespond(connection);
        }

        private async Task ReadAndRespond(TransportConnection connection)
        {
            //while (true)
            //{
            //    var result = await connection.Output.ReadAsync();
            //    Console.WriteLine("Got actual useful data and it is ---------------------------------");
            //    Console.WriteLine(Encoding.UTF8.GetString(result.Buffer.First.ToArray()));
            //    connection.Output.AdvanceTo(result.Buffer.End);

            //}
            //Need to read until we find a End of request and then write back the correct response
        }
    }
}


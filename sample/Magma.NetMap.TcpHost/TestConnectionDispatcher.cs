using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Magma.NetMap.TcpHost
{
    public class TestConnectionDispatcher : IConnectionDispatcher
    {
        public void OnConnection(TransportConnection connection)
        {
            // REVIEW: Unfortunately, we still need to use the service context to create the pipes since the settings
            // for the scheduler and limits are specified here
            var inputOptions = GetInputPipeOptions(connection.MemoryPool, connection.InputWriterScheduler);
            var outputOptions = GetOutputPipeOptions(connection.MemoryPool, connection.OutputReaderScheduler);

            var pair = DuplexPipe.CreateConnectionPair(inputOptions, outputOptions);

            // Set the transport and connection id
            connection.ConnectionId = Guid.NewGuid().ToString();
            connection.Transport = pair.Transport;

            // This *must* be set before returning from OnConnection
            connection.Application = pair.Application;
            var ignore = ReadAndRespond(connection);
        }

        internal static PipeOptions GetOutputPipeOptions(MemoryPool<byte> memoryPool, PipeScheduler readerScheduler) => new PipeOptions
        (
            pool: memoryPool,
            readerScheduler: readerScheduler,
            writerScheduler: PipeScheduler.ThreadPool,
            pauseWriterThreshold: (50 * 1024),
            resumeWriterThreshold: (50 * 1024),
            useSynchronizationContext: false,
            minimumSegmentSize: KestrelMemoryPool.MinimumSegmentSize
        );

        // Internal for testing
        internal static PipeOptions GetInputPipeOptions(MemoryPool<byte> memoryPool, PipeScheduler writerScheduler) => new PipeOptions
        (
            pool: memoryPool,
            readerScheduler: PipeScheduler.ThreadPool,
            writerScheduler: writerScheduler,
            pauseWriterThreshold: 0,
            resumeWriterThreshold: 0,
            useSynchronizationContext: false,
            minimumSegmentSize: KestrelMemoryPool.MinimumSegmentSize
        );

        private async Task ReadAndRespond(TransportConnection connection)
        {
            while (true)
            {
                var result = await connection.Application.Input.ReadAsync();
                Console.WriteLine("Got actual useful data and it is ---------------------------------");
                Console.WriteLine(Encoding.UTF8.GetString(result.Buffer.First.ToArray()));
                connection.Application.Input.AdvanceTo(result.Buffer.End);

            }
            //Need to read until we find a End of request and then write back the correct response
        }
    }
}


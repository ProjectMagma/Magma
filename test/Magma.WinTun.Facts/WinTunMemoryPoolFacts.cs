using System;
using System.Buffers;
using System.Threading.Tasks;
using Magma.WinTun.Internal;
using Xunit;

namespace Magma.WinTun.Facts
{
    public class WinTunMemoryPoolFacts
    {
        [Fact]
        public void PoolCreatesBuffersOfCorrectSize()
        {
            var poolSize = 10;
            var bufferSize = 2000;
            var pool = new WinTunMemoryPool(poolSize, bufferSize);

            var success = pool.TryGetMemory(out var memory);
            
            Assert.True(success);
            Assert.NotNull(memory);
            Assert.Equal(bufferSize, memory.Memory.Length);
            
            memory.Return();
        }

        [Fact]
        public void PoolSupportsMultipleBuffers()
        {
            var poolSize = 5;
            var bufferSize = 1500;
            var pool = new WinTunMemoryPool(poolSize, bufferSize);

            var buffers = new WinTunMemoryPool.WinTunOwnedMemory[poolSize];
            
            for (var i = 0; i < poolSize; i++)
            {
                var success = pool.TryGetMemory(out buffers[i]);
                Assert.True(success);
                Assert.Equal(bufferSize, buffers[i].Memory.Length);
            }
            
            var exhausted = pool.TryGetMemory(out _);
            Assert.False(exhausted);
            
            for (var i = 0; i < poolSize; i++)
            {
                buffers[i].Return();
            }
        }

        [Fact]
        public void ReturnedBufferCanBeReused()
        {
            var pool = new WinTunMemoryPool(2, 1000);

            var success1 = pool.TryGetMemory(out var memory1);
            Assert.True(success1);
            
            memory1.Return();
            
            var success2 = pool.TryGetMemory(out var memory2);
            Assert.True(success2);
            
            memory2.Return();
        }

        [Fact]
        public void TryGetMemoryReturnsFalseWhenExhausted()
        {
            var pool = new WinTunMemoryPool(1, 1000);

            var success1 = pool.TryGetMemory(out var memory1);
            Assert.True(success1);
            
            var success2 = pool.TryGetMemory(out var memory2);
            Assert.False(success2);
            Assert.Null(memory2);
            
            memory1.Return();
        }

        [Fact]
        public async Task GetMemoryAsyncWaitsWhenPoolIsEmpty()
        {
            var pool = new WinTunMemoryPool(1, 1000);

            var memory1 = await pool.GetMemoryAsync();
            
            var getTask = pool.GetMemoryAsync();
            await Task.Delay(50);
            Assert.False(getTask.IsCompleted);
            
            memory1.Return();
            
            var memory2 = await getTask;
            Assert.NotNull(memory2);
            
            memory2.Return();
        }

        [Fact]
        public void MemoryOwnerGetSpanReturnsCorrectSpan()
        {
            var pool = new WinTunMemoryPool(1, 1500);
            var success = pool.TryGetMemory(out var memory);
            
            Assert.True(success);
            var span = memory.GetSpan();
            Assert.Equal(1500, span.Length);
            
            memory.Return();
        }
    }
}

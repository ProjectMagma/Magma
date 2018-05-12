using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Magma.Network.Header;

namespace Magma.Internet.Ip
{
    public class IPv4Defragmenter
    {
        private readonly IMemoryOwner<byte>[] _fragments;
        private int _currentExpectedSync;
        private int _fragmentsHeld;

        public IPv4Defragmenter(int maxFragments) => _fragments = new IMemoryOwner<byte>[maxFragments];

        // Do we want the memory owner to go down to here? If so and we are going to drop the fragments
        // We need to dispose of the memory owners.
        public bool TryDefrag(int expectedSync, IPv4 header, IMemoryOwner<byte> buffer)
        {
            if(_currentExpectedSync == -1)
            {
                _currentExpectedSync = expectedSync;
                _fragmentsHeld = 0;
            }
            else if(expectedSync != _currentExpectedSync )
            {
                throw new InvalidOperationException("Tried to wait on a different sync than we currently were waiting on, need to figure out how to handle");
            }

            // Need to figure out the mod of where we want to save the fragment
            // Then we will put into the array, if we have space, then we check to see if we completed the packet
            // if we haven't then we check if we have got to our max fragments. If that is the case we need to figure out what to do
            // Do we just drop or send an ICMP to say its a problem?

            throw new NotImplementedException();
        }
    }
}

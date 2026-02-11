using System;
using System.Runtime.CompilerServices;

namespace Magma.DPDK.Interop;

/// <summary>
/// Extension methods for working with DPDK mbufs and providing zero-copy access via Span&lt;T&gt;.
/// </summary>
internal static unsafe class MbufExtensions
{
    /// <summary>
    /// Get a Span&lt;byte&gt; view of the packet data in an mbuf (zero-copy).
    /// </summary>
    /// <param name="mbuf">Pointer to the mbuf</param>
    /// <returns>Span representing the packet data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> GetPacketData(this rte_mbuf* mbuf)
    {
        if (mbuf == null)
            return Span<byte>.Empty;

        var dataPtr = (byte*)mbuf->buf_addr + mbuf->data_off;
        return new Span<byte>(dataPtr, mbuf->data_len);
    }

    /// <summary>
    /// Get a pointer to the start of packet data in an mbuf.
    /// </summary>
    /// <param name="mbuf">Pointer to the mbuf</param>
    /// <returns>Pointer to packet data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte* GetDataPointer(this rte_mbuf* mbuf)
    {
        if (mbuf == null)
            return null;

        return (byte*)mbuf->buf_addr + mbuf->data_off;
    }

    /// <summary>
    /// Set the packet length for an mbuf.
    /// </summary>
    /// <param name="mbuf">Pointer to the mbuf</param>
    /// <param name="length">Packet length in bytes</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetPacketLength(this rte_mbuf* mbuf, ushort length)
    {
        mbuf->data_len = length;
        mbuf->pkt_len = length;
    }

    /// <summary>
    /// Copy data into an mbuf's packet buffer.
    /// </summary>
    /// <param name="mbuf">Pointer to the mbuf</param>
    /// <param name="source">Source data to copy</param>
    /// <returns>True if successful, false if buffer too small</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CopyPacketData(this rte_mbuf* mbuf, ReadOnlySpan<byte> source)
    {
        if (mbuf == null || source.Length > mbuf->buf_len - mbuf->data_off)
            return false;

        var dest = mbuf->GetPacketData();
        source.CopyTo(dest);
        mbuf->SetPacketLength((ushort)source.Length);
        return true;
    }

    /// <summary>
    /// Get the available buffer space in an mbuf for packet data.
    /// </summary>
    /// <param name="mbuf">Pointer to the mbuf</param>
    /// <returns>Available space in bytes</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetAvailableSpace(this rte_mbuf* mbuf)
    {
        if (mbuf == null)
            return 0;

        return mbuf->buf_len - mbuf->data_off;
    }
}

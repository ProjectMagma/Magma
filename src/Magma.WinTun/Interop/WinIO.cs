using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Magma.WinTun.Interop
{
    internal class WinIO
    {
        public const int FILE_ATTRIBUTE_SYSTEM = 0x4;
        public const int FILE_FLAG_OVERLAPPED = 0x40000000;
        private const uint FILE_DEVICE_UNKNOWN = 0x00000022;
        private const uint FILE_ANY_ACCESS = 0;
        private const uint METHOD_BUFFERED = 0;

        [DllImport("kernel32")]
        internal static extern SafeFileHandle CreateFile(string filename, FileAccess fileAccess, FileShare fileShare, int securityAttribs, FileMode fileMode, int flags, IntPtr extraData);

        [DllImport("kernel32")]
        private unsafe static extern bool DeviceIoControl(SafeFileHandle deviceHandle, uint dwIoControlCode, void* lpInBuffer, uint nInBufferSize, void* lpOutBuffer, uint nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        internal unsafe static void SetMediaStatus(SafeFileHandle deviceHandle)
        {
            var pStatus = 1;
            var result = DeviceIoControl(deviceHandle, TAP_CONTROL_CODE(6, METHOD_BUFFERED), &pStatus, sizeof(int), &pStatus, 4, out var len, IntPtr.Zero);
            if (!result) throw new InvalidOperationException("Failed to set media status");
        }

        internal unsafe static void SetTapIOCtl(SafeFileHandle deviceHandle)
        {
            var inputValues = stackalloc int[3];
            inputValues[0] = 0x0100030A;
            inputValues[1] = 0x0000030A;
            inputValues[2] = unchecked((int)0x00FFFFFF);
            var result = DeviceIoControl(deviceHandle, TAP_CONTROL_CODE(10, METHOD_BUFFERED), inputValues, 12, inputValues, 12, out var len, IntPtr.Zero);
            if (!result) throw new InvalidOperationException("Failed to set tun setup");
        }

        private static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
        {
            return ((DeviceType << 16) | (Access << 14) | (Function << 2) | Method);
        }

        static uint TAP_CONTROL_CODE(uint request, uint method)
        {
            return CTL_CODE(FILE_DEVICE_UNKNOWN, request, method, FILE_ANY_ACCESS);
        }
    }
}

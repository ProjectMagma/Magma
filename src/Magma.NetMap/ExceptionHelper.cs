using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap
{
    internal static class ExceptionHelper
    {
        public static void Throw<T>() where T : Exception, new() => throw new T();
        public static void ThrowInvalidOperation(string message) => throw new InvalidOperationException(message);
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Genies.Naf
{
#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class NativeCallbacksManager : IDisposable
#else
    public sealed class NativeCallbacksManager : IDisposable
#endif
    {
        private readonly List<NativeCallback> _callbacks;

        private IntPtr Create(Delegate callback)
        {
            var nativeCallback = new NativeCallback(callback);
            _callbacks.Add(nativeCallback);
            return nativeCallback.Pointer;
        }

        public void Dispose()
        {
            foreach (NativeCallback callback in _callbacks)
            {
                callback.Dispose();
            }

            _callbacks.Clear();
        }

        ~NativeCallbacksManager()
        {
            Dispose();
        }
    }
}

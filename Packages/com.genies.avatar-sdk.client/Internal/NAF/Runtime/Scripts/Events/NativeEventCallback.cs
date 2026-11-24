using System;
using GnWrappers;

namespace Genies.Naf
{
#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class NativeEventCallback : IDisposable
#else
    public sealed class NativeEventCallback : IDisposable
#endif
    {
        public NativeCallback NativeCallback { get; private set; }
        public EventCallback  EventCallback  { get; private set; }

        public NativeEventCallback(NativeCallback nativeCallback, EventCallback eventCallback)
        {
            NativeCallback = nativeCallback;
            EventCallback  = eventCallback;
        }

        public void Dispose()
        {
            NativeCallback?.Dispose();
            EventCallback?.Dispose();
            NativeCallback = null;
            EventCallback = null;
        }

        ~NativeEventCallback()
        {
            Dispose();
        }
    }
}

using System;
using GnWrappers;

namespace Genies.Naf
{
#if GENIES_SDK && !GENIES_INTERNAL
    internal static class EventExtensions
#else
    public static class EventExtensions
#endif
    {
        public static NativeEventCallback Subscribe(this Event ev, Delegate callback)
        {
            NativeCallback      nativeCallback = new (callback);
            EventCallback       eventCallback  = ev.CreateCallback(nativeCallback.Pointer);
            NativeEventCallback natEvCallback  = new (nativeCallback, eventCallback);

            ev.Subscribe(eventCallback);

            return natEvCallback;
        }

        public static NativeEventCallback SubscribeAndDispose(this Event ev, Delegate callback)
        {
            NativeCallback      nativeCallback = new (callback);
            EventCallback       eventCallback  = ev.CreateCallback(nativeCallback.Pointer);
            NativeEventCallback natEvCallback  = new (nativeCallback, eventCallback);

            ev.Subscribe(eventCallback);
            ev.Dispose();

            return natEvCallback;
        }

        public static NativeEventCallback Subscribe(this Event ev, in NativeCallback nativeCallback)
        {
            EventCallback       eventCallback = ev.CreateCallback(nativeCallback.Pointer);
            NativeEventCallback natEvCallback = new (nativeCallback, eventCallback);

            ev.Subscribe(eventCallback);

            return natEvCallback;
        }

        public static NativeEventCallback SubscribeAndDispose(this Event ev, in NativeCallback nativeCallback)
        {
            EventCallback       eventCallback = ev.CreateCallback(nativeCallback.Pointer);
            NativeEventCallback natEvCallback = new (nativeCallback, eventCallback);

            ev.Subscribe(eventCallback);
            ev.Dispose();

            return natEvCallback;
        }

        public static void UnsubscribeAndDispose(this Event ev, EventCallback callback)
        {
            ev.Unsubscribe(callback);
            ev.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using GnWrappers;

namespace Genies.Naf
{
#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class EventSubscriptionsManager : IDisposable
#else
    public sealed class EventSubscriptionsManager : IDisposable
#endif
    {
        private readonly List<NativeEventCallback> _callbacks;

        public void SubscribeTo(Event ev, Delegate callback)
        {
            _callbacks.Add(ev.Subscribe(callback));
        }

        public void SubscribeToAndDispose(Event ev, Delegate callback)
        {
            _callbacks.Add(ev.Subscribe(callback));
            ev.Dispose();
        }

        public void SubscribeTo(Event ev, NativeCallback callback)
        {
            _callbacks.Add(ev.Subscribe(callback));
        }

        public void SubscribeToAndDispose(Event ev, NativeCallback callback)
        {
            _callbacks.Add(ev.Subscribe(callback));
            ev.Dispose();
        }

        public void Dispose()
        {
            foreach (NativeEventCallback callback in _callbacks)
            {
                callback.Dispose();
            }

            _callbacks.Clear();
        }

        ~EventSubscriptionsManager()
        {
            Dispose();
        }
    }
}

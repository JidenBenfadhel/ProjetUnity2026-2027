using System;
using GnWrappers;

namespace Genies.Naf
{
#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class NativeEvent : IDisposable
#else
    public sealed class NativeEvent : IDisposable
#endif
    {
        public Event Instance { get; private set; }

        public event Action Value;

        private NativeEventCallback _callback;

        public NativeEvent(Event instance = null, bool invokeInMainThread = false)
        {
            Instance = instance;
            if (Instance is null)
            {
                return;
            }

            Action callback = () => Value?.Invoke();
            if (invokeInMainThread)
            {
                callback = callback.AlwaysInMainThread();
            }

            _callback = Instance.Subscribe(callback);
        }

        public void Dispose()
        {
            Instance?.Dispose();
            _callback?.Dispose();
            Instance = null;
            _callback = null;
        }
    }

#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class NativeEvent<T1> : IDisposable
#else
    public sealed class NativeEvent<T1> : IDisposable
#endif
    {
        public Event Instance { get; private set; }

        public event Action<T1> Value;

        private NativeEventCallback _callback;

        public NativeEvent(Event instance = null, bool invokeInMainThread = false)
        {
            Instance = instance;
            if (Instance is null)
            {
                return;
            }

            Action<T1> callback = (arg1) => Value?.Invoke(arg1);
            if (invokeInMainThread)
            {
                callback = callback.AlwaysInMainThread();
            }

            _callback = Instance.Subscribe(callback);
        }

        public void Dispose()
        {
            Instance?.Dispose();
            _callback?.Dispose();
            Instance = null;
            _callback = null;
        }
    }

#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class NativeEvent<T1, T2> : IDisposable
#else
    public sealed class NativeEvent<T1, T2> : IDisposable
#endif
    {
        public Event Instance { get; private set; }

        public event Action<T1, T2> Value;

        private NativeEventCallback _callback;

        public NativeEvent(Event instance = null, bool invokeInMainThread = false)
        {
            Instance = instance;
            if (Instance is null)
            {
                return;
            }

            Action<T1, T2> callback = (arg1, arg2) => Value?.Invoke(arg1, arg2);
            if (invokeInMainThread)
            {
                callback = callback.AlwaysInMainThread();
            }

            _callback = Instance.Subscribe(callback);
        }

        public void Dispose()
        {
            Instance?.Dispose();
            _callback?.Dispose();
            Instance = null;
            _callback = null;
        }
    }

#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class NativeEvent<T1, T2, T3> : IDisposable
#else
    public sealed class NativeEvent<T1, T2, T3> : IDisposable
#endif
    {
        public Event Instance { get; private set; }

        public event Action<T1, T2, T3> Value;

        private NativeEventCallback _callback;

        public NativeEvent(Event instance = null, bool invokeInMainThread = false)
        {
            Instance = instance;
            if (Instance is null)
            {
                return;
            }

            Action<T1, T2, T3> callback = (arg1, arg2, arg3) => Value?.Invoke(arg1, arg2, arg3);
            if (invokeInMainThread)
            {
                callback = callback.AlwaysInMainThread();
            }

            _callback = Instance.Subscribe(callback);
        }

        public void Dispose()
        {
            Instance?.Dispose();
            _callback?.Dispose();
            Instance = null;
            _callback = null;
        }
    }

#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class NativeEvent<T1, T2, T3, T4> : IDisposable
#else
    public sealed class NativeEvent<T1, T2, T3, T4> : IDisposable
#endif
    {
        public Event Instance { get; private set; }

        public event Action<T1, T2, T3, T4> Value;

        private NativeEventCallback _callback;

        public NativeEvent(Event instance = null, bool invokeInMainThread = false)
        {
            Instance = instance;
            if (Instance is null)
            {
                return;
            }

            Action<T1, T2, T3, T4> callback = (arg1, arg2, arg3, arg4) => Value?.Invoke(arg1, arg2, arg3, arg4);
            if (invokeInMainThread)
            {
                callback = callback.AlwaysInMainThread();
            }

            _callback = Instance.Subscribe(callback);
        }

        public void Dispose()
        {
            Instance?.Dispose();
            _callback?.Dispose();
            Instance = null;
            _callback = null;
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace Genies.Naf
{
    /**
     * Wraps any managed delegate as a native callback that can be used with the NAF foreign callbacks system. Once you
     * have created a native callback instance, the delegate won't be garbage collected until you dispose it.
     *
     * Please note that the given IntPtr is not actually pointing to the delegate, but it is meant to be used with the
     * foreign callbacks system, paired with the static <see cref="GetFrom{T}"/> method to retrieve the managed delegate
     * back.
     */
#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class NativeCallback : IDisposable
#else
    public sealed class NativeCallback : IDisposable
#endif
    {
        public IntPtr Pointer => GCHandle.ToIntPtr(_handle);

        private GCHandle _handle;

        public NativeCallback()
        {
            _handle = default;
        }

        public NativeCallback(Delegate callback)
        {
            _handle = callback is null ? default : GCHandle.Alloc(callback);
        }

        public T GetAs<T>() where T : Delegate
        {
            if (_handle.Target is T value)
            {
                return value;
            }

            throw new InvalidCastException($"This native callback does not point to a delegate of type {typeof(T).Name}. Current type is {_handle.Target.GetType().Name}");
        }

        public void Dispose()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }

            _handle = default;
        }

        public static T GetFrom<T>(IntPtr pointer) where T : Delegate
        {
            var handle = GCHandle.FromIntPtr(pointer);
            if (handle.Target is T value)
            {
                return value;
            }

            throw new InvalidCastException($"The given native callback pointer does not point to a delegate of type {typeof(T).Name}. Current type is {handle.Target.GetType().Name}");
        }

        ~NativeCallback()
        {
            Dispose();
        }
    }
}

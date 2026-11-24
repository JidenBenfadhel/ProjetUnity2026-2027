using System;
using System.Runtime.InteropServices;
using UnityEngine;
using GnWrappers;

namespace Genies.Naf
{
    /**
     * Implements the foreign callback handlers for NAF and automatically registers them on the plugin. If new callback
     * types are needed, you have to instantiate them on the Swig wrapper code first, then recompile the plugin and add
     * them here as well.
     *
     * NOTE: you could be tempted to use lambdas in the readonly Action fields directly. While this would work when
     * using Mono (i.e.: in the Editor), it will completely break in IL2CPP builds.
     */
    internal static class ForeignCallbackHandlers
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SetNativeHandler()
        {
            GnUtilsWrapper.ForeignCallbackHandler_Void        = Marshal.GetFunctionPointerForDelegate(_handler_Void);
            GnUtilsWrapper.ForeignCallbackHandler_Void_Bool   = Marshal.GetFunctionPointerForDelegate(_handler_Void_Bool);
            GnUtilsWrapper.ForeignCallbackHandler_Void_Int    = Marshal.GetFunctionPointerForDelegate(_handler_Void_Int);
            GnUtilsWrapper.ForeignCallbackHandler_Void_Float  = Marshal.GetFunctionPointerForDelegate(_handler_Void_Float);
            GnUtilsWrapper.ForeignCallbackHandler_Void_String = Marshal.GetFunctionPointerForDelegate(_handler_Void_String);
        }

        private static readonly Action<IntPtr>           _handler_Void        = Handler_Void;
        private static readonly Action<IntPtr, bool>     _handler_Void_Bool   = Handler_Void_Bool;
        private static readonly Action<IntPtr, int>      _handler_Void_Int    = Handler_Void_Int;
        private static readonly Action<IntPtr, float>    _handler_Void_Float  = Handler_Void_Float;
        private static readonly Action<IntPtr, string>   _handler_Void_String = Handler_Void_String;

        [AOT.MonoPInvokeCallback(typeof(Action<IntPtr>))]
        private static void Handler_Void(IntPtr callback)
            => NativeCallback.GetFrom<Action>(callback).Invoke();

        [AOT.MonoPInvokeCallback(typeof(Action<IntPtr, bool>))]
        private static void Handler_Void_Bool(IntPtr callback, bool arg)
            => NativeCallback.GetFrom<Action<bool>>(callback).Invoke(arg);

        [AOT.MonoPInvokeCallback(typeof(Action<IntPtr, int>))]
        private static void Handler_Void_Int(IntPtr callback, int arg)
            => NativeCallback.GetFrom<Action<int>>(callback).Invoke(arg);

        [AOT.MonoPInvokeCallback(typeof(Action<IntPtr, float>))]
        private static void Handler_Void_Float(IntPtr callback, float arg)
            => NativeCallback.GetFrom<Action<float>>(callback).Invoke(arg);

        [AOT.MonoPInvokeCallback(typeof(Action<IntPtr, string>))]
        private static void Handler_Void_String(IntPtr callback, string arg)
            => NativeCallback.GetFrom<Action<string>>(callback).Invoke(arg);
    }
}

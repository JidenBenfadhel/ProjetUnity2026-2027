using System;
using Cysharp.Threading.Tasks;

namespace Genies.Naf
{
#if GENIES_SDK && !GENIES_INTERNAL
    internal static class ActionExtensions
#else
    public static class ActionExtensions
#endif
    {
        public static Action AlwaysInMainThread(this Action action)
        {
            return () => Execute().Forget();
            async UniTaskVoid Execute() { await UniTask.SwitchToMainThread(); action.Invoke(); }
        }

        public static Action<T1> AlwaysInMainThread<T1>(this Action<T1> action)
        {
            return (T1 arg1) => Execute(arg1).Forget();
            async UniTaskVoid Execute(T1 arg1) { await UniTask.SwitchToMainThread(); action.Invoke(arg1); }
        }

        public static Action<T1, T2> AlwaysInMainThread<T1, T2>(this Action<T1, T2> action)
        {
            return (T1 arg1, T2 arg2) => Execute(arg1, arg2).Forget();
            async UniTaskVoid Execute(T1 arg1, T2 arg2) { await UniTask.SwitchToMainThread(); action.Invoke(arg1, arg2); }
        }

        public static Action<T1, T2, T3> AlwaysInMainThread<T1, T2, T3>(this Action<T1, T2, T3> action)
        {
            return (T1 arg1, T2 arg2, T3 arg3) => Execute(arg1, arg2, arg3).Forget();
            async UniTaskVoid Execute(T1 arg1, T2 arg2, T3 arg3) { await UniTask.SwitchToMainThread(); action.Invoke(arg1, arg2, arg3); }
        }

        public static Action<T1, T2, T3, T4> AlwaysInMainThread<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action)
        {
            return (T1 arg1, T2 arg2, T3 arg3, T4 arg4) => Execute(arg1, arg2, arg3, arg4).Forget();
            async UniTaskVoid Execute(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                await UniTask.SwitchToMainThread();
                action.Invoke(arg1, arg2, arg3, arg4);
            }
        }
    }
}

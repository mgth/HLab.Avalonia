using System.Runtime.CompilerServices;
using ReactiveUI;

namespace HLab.Base.Avalonia.Extensions
{
    public static class ReactiveObjectExtensions
    {
        public static TRet LazyGet<TObj, TRet>(
            this TObj @this,
            ref TRet backingField, 
            Func<TRet,bool> predicate,
            Func<TObj,Task> setter
            )
            where TObj : IReactiveObject
        {
            if (predicate(backingField))
            {
                Task.Run(() => setter(@this));
            }

            return backingField;
        }

        public static TRet? LazyGet<TObj, TRet>(
            this TObj @this,
            ref TRet? backingField,
            Func<TObj, Task> setter
        )
            where TObj : IReactiveObject
            where TRet : class
        {
            if (backingField == null)
            {
                Task.Run(() => setter(@this));
            }
            return backingField;
        }

        public static TRet? LazyGet<TObj, TRet>(
            this TObj @this,
            ref TRet? backingField,
            Action<TObj> setter
        )
            where TObj : IReactiveObject
            where TRet : class
        {
            if (backingField == null)
            {
                Task.Run(() => setter(@this));
            }
            return backingField;
        }
        //public static TRet? LazyGet<TObj, TRet>(
        //    this TObj @this,
        //    ref TRet? backingField,
        //    Func<TObj,TRet> setter
        //)
        //    where TObj : IReactiveObject
        //    where TRet : class
        //{
        //    if (backingField == null)
        //    {
        //        Task.Run(() =>
        //        {
        //            @this.RaiseAndSetIfChanged(ref backingField, setter(@this));
        //        });
        //    }
        //    return backingField;
        //}

    }
}

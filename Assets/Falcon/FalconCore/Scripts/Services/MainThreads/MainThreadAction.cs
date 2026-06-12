// using System;
// using System.Threading;
// using Falcon.FalconCore.Scripts.Entities;
// using Falcon.FalconCore.Scripts.Service.GameObjs;
// using Falcon.FalconCore.Scripts.Utils.FActions.Base;
// using Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts;
//
// namespace Falcon.FalconCore.Scripts.Service.MainThreads
// {
//     public class MainThreadAction : ChainAction
//     {
//         private static readonly FQueue<MainThreadAction> MainThreadActions = Init();
//         
//         private static FQueue<MainThreadAction> Init()
//         {
//             FQueue<MainThreadAction> queue = new();
//             FGameObj.Instance.OnUpdate += ( _, _ ) =>
//             {
//                 while( queue.TryDequeue( out var result ) )
//                 {
//                     result.Invoke();
//                 }
//             };
//
//             return queue;
//         }
//         public MainThreadAction(IContinuableAction baseAction) : base(baseAction)
//         {
//         }
//
//         public MainThreadAction(Action baseAction) : base(new UnitAction(baseAction))
//         {
//         }
//
//         public override void Invoke()
//         {
//             if (Thread.CurrentThread.ManagedThreadId == FThreadService.MainThreadId)
//             {
//                 base.Invoke();
//                 MainThreadActions.Enqueue( this );
//             }
//             else
//             {
//                 MainThreadActions.Enqueue( this );
//             }
//         }
//         
//     }
//     
//     public class MainThreadAction<T> : MainThreadAction, IChainAction<T>
//     {
//         public MainThreadAction(IContinuableAction<T> baseAction) : base(baseAction)
//         {
//         }
//
//         public MainThreadAction(Func<T> baseAction) : base(new UnitAction<T>(baseAction))
//         {
//         }
//
//         public bool TryInvoke(out T result)
//         {
//             if (FThreadService.MainThreadId.HasValue)
//             {
//                 Invoke();
//                 while (!Done && Exception != null)
//                 {
//                     Thread.Yield();
//                 }
//
//                 if (Exception != null) throw Exception;
//                 result = Result;
//                 return true;
//             }
//             else
//             {
//                 result = default(T);
//                 return false;
//             }
//         }
//
//         public T Result => ((IContinuableAction<T>)BaseAction).Result;
//     }
// }
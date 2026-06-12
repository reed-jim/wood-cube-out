using System;
using System.Threading;
using Falcon.FalconCore.Scripts.Logs;
using Falcon.FalconCore.Scripts.Services.GameObjs;
using Falcon.FalconCore.Scripts.Utils.Entities;
using Falcon.FalconCore.Scripts.Utils.FActions.Base;
using Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts;
using UnityEngine;

namespace Falcon.FalconCore.Scripts.Services.MainThreads
{
    public class FThreadService : IPioneerService
    {
        public static int? MainThreadId { get; private set; }
        
        internal static readonly FQueue<MainThreadAction> MainThreadActions = new FQueue<MainThreadAction>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            FGameObj.OnUpdate += ( ignored1, ignored2 ) =>
            {
                MainThreadAction result;
                if( MainThreadActions.TryDequeue( out result ) )
                {
                    result.Invoke();
                }
            };
            CoreLogger.Instance.Info("FThreadService init complete");

        }

        public void OnPreContinue()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
        }
    }
    
    public class MainThreadAction : ChainAction
    {
        public MainThreadAction(IContinuableAction baseAction) : base(baseAction)
        {
        }

        public MainThreadAction(Action baseAction) : base(new UnitAction(baseAction))
        {
        }

        public override void Invoke()
        {
            if (Thread.CurrentThread.ManagedThreadId == FThreadService.MainThreadId)
            {
                base.Invoke();
            }
            else
            {
                FThreadService.MainThreadActions.Enqueue( this );
            }
        }
    }
    
    public class MainThreadAction<T> : MainThreadAction, IChainAction<T>
    {
        public MainThreadAction(IContinuableAction<T> baseAction) : base(baseAction)
        {
        }

        public MainThreadAction(Func<T> baseAction) : base(new UnitAction<T>(baseAction))
        {
        }

        public bool TryInvoke(out T result)
        {
            if (FThreadService.MainThreadId.HasValue)
            {
                Invoke();
                while (!Done && Exception != null)
                {
                    Thread.Yield();
                }

                if (Exception != null) throw Exception;
                result = Result;
                return true;
            }

            result = default(T);
            return false;
        }

        public T Result => ((IContinuableAction<T>)BaseAction).Result;
    }
}
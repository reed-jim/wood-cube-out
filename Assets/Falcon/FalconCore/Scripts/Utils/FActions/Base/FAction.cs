using System;
using System.Threading;
using Falcon.FalconCore.Scripts.Logs;
using Falcon.FalconCore.Scripts.Services.GameObjs;
using Falcon.FalconCore.Scripts.Utils.Entities;
using UnityEngine;

namespace Falcon.FalconCore.Scripts.Utils.FActions.Base
{
    public abstract class FAction : IFAction
    {
        public abstract Exception Exception { get;}
        public abstract bool Done { get; }
        
        public virtual void Schedule()
        {
            ActionQueue.Enqueue(this);
        }

        public virtual void Cancel()
        {
            ActionQueue.Remove(this);
        }

        public abstract void Invoke();
        
        public abstract bool CanInvoke();

        #region Scheduling

        private static readonly FQueue<FAction> ActionQueue = new FQueue<FAction>();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitFAction()
        {
            FGameObj.OnUpdate += (a, b) =>
            {
                FAction action;
                if (ActionQueue.TryDequeue(out action))
                {
                    if (action.CanInvoke())
                        ThreadPool.QueueUserWorkItem(_ => { action.Invoke(); });
                    else
                        ActionQueue.Enqueue(action);
                }
            };
            CoreLogger.Instance.Info("FAction init complete");

        }

        #endregion
    }
}
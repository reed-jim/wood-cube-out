using System;
using System.Threading;
using Falcon.FalconCore.Scripts.Utils.Entities;
using Falcon.FalconCore.Scripts.Utils.FActions.Base;
using Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts;
using UnityEditor;
using UnityEditorInternal;

namespace Falcon.FalconCore.Editor.Services
{
    public static class EditorMainThreadService 
    {
        private static FQueue<EditorMainThreadAction> _actions;

        internal static FQueue<EditorMainThreadAction> Actions
        {
            get
            {
                if (_actions == null)
                {
                    _actions = new FQueue<EditorMainThreadAction>();
                    EditorApplication.update += () =>
                    {
                        EditorMainThreadAction action;
                        if(Actions.TryDequeue(out action))
                        {
                            action.Invoke();
                        }
                    };
                }

                return _actions;
            }
        }
    }
    
    public class EditorMainThreadAction : ChainAction
    {
        public EditorMainThreadAction(IContinuableAction baseAction) : base(baseAction)
        {
        }
        
        public EditorMainThreadAction(Action baseAction) : base(new UnitAction(baseAction))
        {
        }

        public override void Schedule()
        {
            EditorMainThreadService.Actions.Enqueue(this);
        }

        public override void Invoke()
        {
            if (InternalEditorUtility.CurrentThreadIsMainThread())
            {
                base.Invoke();
            }
            else
            {
                EditorMainThreadService.Actions.Enqueue(this);
            }
        }
    }
    
    public class EditorMainThreadAction<T> : EditorMainThreadAction, IChainAction<T>
    {
        public EditorMainThreadAction(IContinuableAction<T> baseAction) : base(baseAction)
        {
        }

        public EditorMainThreadAction(Func<T> baseAction) : base(new UnitAction<T>(baseAction))
        {
        }

        public bool TryInvoke(out T result)
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

        public T Result => ((IContinuableAction<T>)BaseAction).Result;
    }
}
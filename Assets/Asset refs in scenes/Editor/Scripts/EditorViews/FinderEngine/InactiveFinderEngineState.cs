#if UNITY_EDITOR

using System;
using UnityEngine;
using SearchEngine.Additions;

namespace SearchEngine.EditorViews.FinderEngine
{
    public class InactiveFinderEngineState : IFinderEngineStateInActive
    {
        public event Action EngineFinderInactive;
        
        public void ExecuteCurrGO(GameObject go)
        {
            ErrorMsg();
        }

        public void OnSceneComplete(string currScene)
        {
            ErrorMsg();
        }

        public void StopExecuting()
        {
            ErrorMsg();
        }

        private void ErrorMsg()
        {
            Debug.LogWarning("Engine Finder is inactive!");
            EventSender.SendEvent(EngineFinderInactive);            
        }

    }
}

#endif

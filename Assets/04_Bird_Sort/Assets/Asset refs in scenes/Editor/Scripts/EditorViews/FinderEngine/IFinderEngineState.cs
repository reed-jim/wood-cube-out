#if UNITY_EDITOR 

using UnityEngine;

namespace SearchEngine.EditorViews.FinderEngine
{
    public interface IFinderEngineState
    {
        void ExecuteCurrGO(GameObject go);
        void OnSceneComplete(string currScene);
        void StopExecuting();
    }
}

#endif
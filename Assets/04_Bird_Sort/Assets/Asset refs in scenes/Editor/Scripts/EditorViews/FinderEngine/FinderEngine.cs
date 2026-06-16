#if UNITY_EDITOR

using UnityEngine;
using SearchEngine.Additions;

namespace SearchEngine.EditorViews.FinderEngine
{
    public class FinderEngine
    {
        [SerializeField] private IFinderEngineState inactiveState;
        [SerializeField] private IFinderEngineState activeState;
        private IFinderEngineState state;

        public FinderEngine(IFinderEngineState inactiveState, IFinderEngineState activeState)
        {
            this.inactiveState = inactiveState;
            this.activeState = activeState;
            this.CheckSerializeFields();

            state = inactiveState;
        }

        public void SeInactiveState()
        {
            state = inactiveState;
        }

        public void SeActiveState()
        {
            state = activeState;
        }

        public void ExecuteCurrGO(GameObject go)
        {
            state.ExecuteCurrGO(go);
        }

        public void OnSceneComplete(string currScene)
        {
            state.OnSceneComplete(currScene);
        }

        public void StopExecuting()
        {
            state.StopExecuting();
        }
    }
}

#endif

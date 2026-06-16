#if UNITY_EDITOR

using UnityEngine;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Tables;

namespace SearchEngine.EditorViews
{
    public class SearchPanelEV : IEditorView
    {
        [SerializeField] private SPDefaultStateEV defaultState;
        [SerializeField] private SPLockedStateEV lockedState;
        [SerializeField] private SPInProgressStateEV inProgressState;
        private ISPState state;

        public const int ButtonHeight = 38;
        public const int StartButtonWidth = 240;
                
        public SearchPanelEV(SPDefaultStateEV defaultState, SPLockedStateEV lockedState, SPInProgressStateEV inProgressState)
        {
            this.defaultState = defaultState;
            this.lockedState = lockedState;
            this.inProgressState = inProgressState;
            this.CheckSerializeFields();

            state = defaultState;
        }

        public void SetDefaultState()
        {
            state = defaultState;
        }

        public void SetLockedState()
        {
            if (state == defaultState)
                state = lockedState;
        }

        public void SetInProgressState()
        {
            if (state == defaultState)
                state = inProgressState;
        }

        public void ShowGUI()
        {
            state.ShowGUI();
        }
    }
}

#endif
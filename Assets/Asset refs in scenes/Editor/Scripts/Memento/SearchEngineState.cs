#if UNITY_EDITOR

using System;
using SearchEngine.EditorViews;
using SearchEngine.EditorViews.GCModeFacade;
using SearchEngine.EditorWindows;
using UnityEngine;

namespace SearchEngine.Memento
{
    [Serializable]
    public class SearchEngineState : ScriptableObject, IValidatable
    {
        public SearchEnginesEV.Memento engines;
        public AFSearchEngineFacadeEV.Memento modeAF;
        public MRFSearchEngineFacadeEV.Memento modeMRF;
         
        public bool resultsEWVisible;
        public ResultsEW.Memento resultsEW;

        public bool Validate()
        {
            return
                engines != null
                && modeAF != null
                && modeMRF != null
                && resultsEW != null;
        }
    }
}

#endif
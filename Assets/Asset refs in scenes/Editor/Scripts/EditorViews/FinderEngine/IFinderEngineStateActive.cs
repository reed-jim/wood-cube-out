using System;

#if UNITY_EDITOR 

namespace SearchEngine.EditorViews.FinderEngine
{
    public interface IFinderEngineStateActive : IFinderEngineState
    {
        event Action Started;
    }
}

#endif


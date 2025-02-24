using System;

#if UNITY_EDITOR 

namespace SearchEngine.EditorViews.FinderEngine
{
    public interface IFinderEngineStateInActive : IFinderEngineState
    {
        event Action EngineFinderInactive;
    }
}

#endif


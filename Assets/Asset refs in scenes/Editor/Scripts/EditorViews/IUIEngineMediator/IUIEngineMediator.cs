#if UNITY_EDITOR

namespace SearchEngine.EditorViews.GCModeFacade
{
    public interface IUIEngineMediator
    {
        void ExecuteStart();
        void OnExecuteStop();
    }
}

#endif
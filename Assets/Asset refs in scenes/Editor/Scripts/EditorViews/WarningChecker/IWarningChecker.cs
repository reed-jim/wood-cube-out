#if UNITY_EDITOR

namespace SearchEngine.EditorViews.GCModeFacade
{
    public interface IWarningChecker
    {
        void Check(ref string warningMsg);
        bool Check();
    }
}

#endif
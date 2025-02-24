#if UNITY_EDITOR

namespace SearchEngine.EditorViews.GCModeFacade
{
    public abstract class WarningChecker
    {
        protected const string noItemsTemplate = "     add at least one {0} to \"{1}\"";
        protected const string noItemsActiveTemplate = "     activate at least one {0} in \"{1}\"";
    }
}

#endif
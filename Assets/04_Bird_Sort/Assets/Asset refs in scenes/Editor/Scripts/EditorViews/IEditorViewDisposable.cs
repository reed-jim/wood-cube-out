using System;

#if UNITY_EDITOR 

namespace SearchEngine.EditorViews
{
    public interface IEditorViewDisposable : IEditorView, IDisposable
    {

    }
}

#endif
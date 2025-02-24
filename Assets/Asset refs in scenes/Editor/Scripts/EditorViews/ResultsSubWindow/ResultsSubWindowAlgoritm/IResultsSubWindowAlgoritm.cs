#if UNITY_EDITOR 

using System;
using UnityEngine;

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    public interface IResultsSubWindowAlgoritm
    {
        event Action CloseButtonClicked;
        void ShowHeaderContent();
        void ShowMiddleContent();
        Vector2 WindowSize { set; }
    }
}

#endif
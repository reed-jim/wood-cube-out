#if UNITY_EDITOR

using UnityEngine;

namespace SearchEngine.Memento
{
    public static class MementoHelper
    {
        public static void ShowDataLoadingWarningMsg()
        {
            Debug.LogWarning("Invalid load data!");
        }
    }
}

#endif
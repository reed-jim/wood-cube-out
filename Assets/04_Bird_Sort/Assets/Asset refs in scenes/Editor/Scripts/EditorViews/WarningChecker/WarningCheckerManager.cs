#if UNITY_EDITOR

using SearchEngine.Additions;
using UnityEngine;

namespace SearchEngine.EditorViews.GCModeFacade
{
    public class WarningCheckerManager
    {
        [SerializeField] private IWarningChecker assets;
        [SerializeField] private IWarningChecker scenes;

        public WarningCheckerManager(IWarningChecker assets, IWarningChecker scenes)
        {
            this.assets = assets;
            this.scenes = scenes;
            this.CheckSerializeFields();
        }

        public string Check()
        {
            string warningMsg = string.Empty;
            assets.Check(ref warningMsg);
            scenes.Check(ref warningMsg);
            GetFinalMsg(ref warningMsg);
            return warningMsg;
        }

        private void GetFinalMsg(ref string warningMsg)
        {
            if (!string.IsNullOrEmpty(warningMsg))
            {
                warningMsg = string.Format("{0}{1}", "To enable search button please:\n", warningMsg);
            }
        }
    }
}

#endif
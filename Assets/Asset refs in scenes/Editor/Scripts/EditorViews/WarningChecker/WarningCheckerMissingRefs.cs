#if UNITY_EDITOR

using SearchEngine.Additions;
using UnityEngine;

namespace SearchEngine.EditorViews.GCModeFacade
{
    public class WarningCheckerMissingRefs : WarningChecker, IWarningChecker
    {
        [SerializeField] private MissingRefTypesEV misRefTypes;
        [SerializeField] private string listItem;
        [SerializeField] private string menuName;

        public WarningCheckerMissingRefs(MissingRefTypesEV misRefTypes, string listItem, string menuName)
        {
            this.misRefTypes = misRefTypes;
            this.listItem = listItem;
            this.menuName = menuName;
            this.CheckSerializeFields();
        }

        public void Check(ref string warningMsg)
        {
            if (Check())
                return;
            string noItems = string.Format(noItemsActiveTemplate, listItem, menuName);
            warningMsg = string.IsNullOrEmpty(warningMsg)
                ? noItems
                : string.Format("{0}\n{1}", warningMsg, noItems);
        }

        public bool Check()
        {
            return misRefTypes.GameObjects || misRefTypes.Components || misRefTypes.InspectorProps;
        }
    }
}

#endif
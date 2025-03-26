#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using UnityEngine;

namespace SearchEngine.EditorViews.GCModeFacade
{
    public class WarningCheckerAssets : WarningChecker, IWarningChecker
    {
        [SerializeField] private IList<AssetInfoData> assetsData;
        [SerializeField] private string listItem;
        [SerializeField] private string menuName;

        public WarningCheckerAssets(IList<AssetInfoData> assetsData, string listItem, string menuName)
        { 
            this.assetsData = assetsData;
            this.listItem = listItem;
            this.menuName = menuName;
            this.CheckSerializeFields();
        }
        
        public void Check(ref string warningMsg)
        {
            string noAssets = null;
            if (!assetsData.Any())
            {
                noAssets = string.Format(noItemsTemplate, listItem, menuName);
            }
            else if (!Check())
            {
                noAssets = string.Format(noItemsActiveTemplate, listItem, menuName);
            }

            if (noAssets != null)
            {
                warningMsg = string.IsNullOrEmpty(warningMsg)
                    ? noAssets
                    : string.Format("{0}\n{1}", warningMsg, noAssets);
            }
        }

        public bool Check()
        {
            return assetsData.Any(v => v.Active && v.Validate());
        }
    }
}

#endif
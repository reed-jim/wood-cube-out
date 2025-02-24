#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SearchEngine.EditorViews.Data;

namespace SearchEngine.EditorViews.AssetTypes
{
    public static class SortingHelper
    {
        public static Comparison<AssetInfoData> GetComparer(IEnumerable<AssetSortingTypes> options)
        {
            var comps = new Comparison<AssetInfoData>[options.Count()];
            for (int i = 0; i < options.Count(); i++)
            {
                comps[i] = SortingHelper.GetComparer(options.ElementAt(i));
            }

            return (x, y) =>
            {
                int result=0;
                foreach (var v in comps)
                {
                    result = v(x, y);
                    if (result != 0)
                    {
                        return result;
                    }
                }
                return result;
            };
        }

        public static Comparison<AssetInfoData> GetComparer(AssetSortingTypes type)
        {
            if (type == AssetSortingTypes.Folder)
                return CompareFolder;
            if (type == AssetSortingTypes.Name)
                return CompareName;
            if (type == AssetSortingTypes.Type)
                return CompareType;
            return null;
        }

        public static int CompareFolder(AssetInfoData x, AssetInfoData y)
        {
            return string.Compare(
                Path.GetDirectoryName(x.AssetPath),
                Path.GetDirectoryName(y.AssetPath));
        }

        public static int CompareName(AssetInfoData x, AssetInfoData y)
        {
            return string.Compare(
                Path.GetFileName(x.AssetPath),
                Path.GetFileName(y.AssetPath));
        }

        public static int CompareType(AssetInfoData x, AssetInfoData y)
        {
            int dif = x.AssetType - y.AssetType;
            return dif > 0 ? 
                1 : dif < 0 ?
                -1 : 0;
        }
    }
}

#endif
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.AssetTypes;
using SearchEngine.Memento;
using UnityEngine;

namespace SearchEngine.EditorViews.Data
{
    [Serializable]
    public class AssetInfoDataManagerAsList : ListWraper<AssetInfoData>, IOriginator<AssetInfoDataManagerAsList.Memento>
    {
        private AssetsFilterEV filter;
        private AssetSortingEV sorting;

        [SerializeField] private List<AssetInfoData> filterData = new List<AssetInfoData>();
        
        public AssetInfoDataManagerAsList(AssetsFilterEV filter, AssetSortingEV sorting)
        {
            this.filter = filter;
            this.sorting = sorting;
            this.CheckSerializeFields();
        }
        
        public void OnSortingChangeEnable()
        {
            if (sorting.Enabled)
            {
                data.Sort(sorting.GetComparer());                
            }          
        }
        public void OnSortingChangeSorting()
        {
            data.Sort(sorting.GetComparer());
        }

        public void OnFilterChangeEnable()
        {
            if (filter.Enabled)
            {
                foreach (var v in data)
                {
                    if(!AssetTypesHelper.CheckAsset(v.AssetType, filter.Filter))
                    {
                        filterData.Add(v);
                    }
                }
                foreach (var v in filterData)
                {
                    data.Remove(v);
                }
            }
            else
            {
                foreach (var v in filterData)
                {
                    SortingAdd(v);
                }
                filterData.Clear();
            }
        }
        public void OnFilterChangeFilter()
        {
            List<AssetInfoData> newFilterData = new List<AssetInfoData>();
            List<AssetInfoData> newNotFilterData = new List<AssetInfoData>();
            foreach (var v in filterData)
            {
                if (AssetTypesHelper.CheckAsset(v.AssetType, filter.Filter))
                {
                    newNotFilterData.Add(v);
                }
            }
            foreach (var v in data)
            {
                if (!AssetTypesHelper.CheckAsset(v.AssetType, filter.Filter))
                {
                    newFilterData.Add(v);
                }
            }
            foreach (var v in newFilterData)
            {
                filterData.Add(v);
                data.Remove(v);
            }
            foreach (var v in newNotFilterData)
            {
                filterData.Remove(v);
                SortingAdd(v);
            }
        }

        private void SortingAdd(AssetInfoData item)
        {
            if (sorting.Enabled && data.Any())
                SortingAddAlg(item);
            else
                data.Add(item);
        }
        private void SortingAddAlg(AssetInfoData item)
        {
            var comp = sorting.GetComparer();
            int begin = 0;
            int end = data.Count-1;
            int cur;
            while(end-begin > 1)
            {
                cur = (begin + end) / 2;
                if (comp(data[begin], data[end]) > 0)
                {
                    end = cur;
                }
                else
                {
                    begin = cur;
                }
            }
            if (comp(data[begin], item) > 0)
            {
                data.Insert(begin, item);
                return;
            }
            if (comp(data[end], item) >= 0)
            {
                data.Insert(end, item);
                return;
            }
            if (end == data.Count-1)
            {
                data.Add(item);
            }
        }

        public override void Add(AssetInfoData item)
        {
            if (filter.Enabled && !AssetTypesHelper.CheckAsset(item.AssetType, filter.Filter))
            {
                //filterData.Add(item);+check for duplicates
            }
            else
            {
                SortingAdd(item);
            }
        }
        public override void Insert(int index, AssetInfoData item)
        {
            if (filter.Enabled && !AssetTypesHelper.CheckAsset(item.AssetType, filter.Filter))
            {
                //filterData.Add(item);+check for duplicates
            }
            else
            {
                if (sorting.Enabled)
                    SortingAddAlg(item);  
                else
                    data.Insert(index, item);
            }
        }
        

        public override void Clear()
        {
            data.Clear();
        }
        public override bool Remove(AssetInfoData item)
        {
            return data.Remove(item);
        }
        public override void RemoveAt(int index)
        {
            data.RemoveAt(index);
        }


        #region  memento part
        public Memento GetMemento()
        {
            return new Memento
            {
                data = CopyAbleHelper.ToList(this.data),
                filterData = CopyAbleHelper.ToList(this.filterData),
            };
        }

        public void SetMemento(Memento mem)
        {
            if (mem == null || !mem.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            this.data = CopyAbleHelper.ToList(mem.data);
            this.filterData = CopyAbleHelper.ToList(mem.filterData);
        }
        [Serializable]
        public class Memento : IValidatable
        {
            public List<AssetInfoData> data;
            public List<AssetInfoData> filterData;

            public bool Validate()
            {
                return
                    data != null
                    && filterData != null
                    && data.All(v => v != null)
                    && filterData.All(v => v != null);
            }
        }
        #endregion
    }
}

#endif
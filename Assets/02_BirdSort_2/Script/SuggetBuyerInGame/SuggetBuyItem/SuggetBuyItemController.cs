using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BirdSort2
{
    public class SuggetBuyItemController : MonoBehaviour
    {
        public const string IS_BOUGHT_ON_SUGGET = "SuggetBuyItem_is_bought_on_sugget";
        public const string NUM_USED_ITEM_AFTER_SUGGET = "SuggetBuyItem_num_used_item_after_sugget";

        public enum StateBuyItem
        {
            Default = 0,//Mặc định
            Bought = 1,//Mua Item trong Sugget
            Skip = 2//Bỏ qua Sugget (Ko mua)
        }

        [System.Serializable]
        public class DataUseItem : SelfDefineNoneMono
        {
            public const string NUM_USE_ITEM = "SuggetBuyItem_num_use_item_";

            public GiftType typeItem;
            public int priovity;
            public int NumUsed
            {
                get
                {
                    return PlayerPrefs.GetInt(NUM_USE_ITEM + typeItem, 0);
                }
                set
                {
                    PlayerPrefs.SetInt(NUM_USE_ITEM + typeItem, value);
                }
            }

            public DataUseItem(GiftType typeItem)
            {
                this.typeItem = typeItem;
            }
        }

        [System.Serializable]
        public class ItemUse
        {
            public GiftType typeItem;
            public int priovity;
        }

        [SerializeField] private List<ItemUse> lstItemUse;
        private List<DataUseItem> datasUse;

        public static bool isFirstShow = true;

        public static int IsBoughtOnSugget
        {
            get
            {
                return PlayerPrefs.GetInt(IS_BOUGHT_ON_SUGGET, 0);
            }
            set
            {
                PlayerPrefs.SetInt(IS_BOUGHT_ON_SUGGET, value);
            }
        }

        public int NumUsedItemAfterSugget
        {
            get
            {
                return PlayerPrefs.GetInt(NUM_USED_ITEM_AFTER_SUGGET, 0);
            }
            set
            {
                PlayerPrefs.SetInt(NUM_USED_ITEM_AFTER_SUGGET, value);
            }
        }

        public void Init()
        {
            datasUse = new List<DataUseItem>();
            for (int i = 0; i < lstItemUse.Count; i++)
            {
                int index = i;
                DataUseItem dt = new DataUseItem(lstItemUse[index].typeItem);
                dt.priovity = lstItemUse[index].priovity;

                datasUse.Add(dt);
            }
        }

        public DataUseItem GetDataItem(GiftType typeItem)
        {
            for (int i = 0; i < datasUse.Count; i++)
            {
                if (datasUse[i].typeItem == typeItem)
                    return datasUse[i];
            }

            return null;
        }

        public void OnUseItem(GiftType typeItem)
        {
            var dtItem = GetDataItem(typeItem);
            if (dtItem != null)
            {
                dtItem.NumUsed++;
            }

            NumUsedItemAfterSugget++;
        }

        public bool IsCanSugget()
        {
            if (isFirstShow)
                return true;
            // Item đã sử dụng đang k tăng sau khi suggest nên k có lần suggest tiếp theo
            if (IsBoughtOnSugget == (int)StateBuyItem.Skip && NumUsedItemAfterSugget <= 0)
                return false;

            return true;
        }


        public void OnSuggetBuyItem()
        {
            if (isFirstShow)
            {
                isFirstShow = false;
            }

            var typeItemSugget = GetItemMatchCondition();
            SuggetBuyItemBox.Setup(true).Show(typeItemSugget);
        }

        public void OnBuyItemOnSugget()
        {
            ResetData();
            IsBoughtOnSugget = (int)StateBuyItem.Bought;
            // LongTH - Add - kiểm tra đã mua item trong suggest
            UseProfile.hadBuySuggest = true;
        }

        public void OnSkipSugget()
        {
            ResetData();
            IsBoughtOnSugget = (int)StateBuyItem
                .Skip;
            // LongTH - Add - kiểm tra không mua item trong suggest
            UseProfile.hadBuySuggest = false;
        }


        public GiftType GetItemMatchCondition()
        {
            bool isAllItemDontUse = true;
            for (int i = 0; i < datasUse.Count; i++)
            {
                if (datasUse[i].NumUsed > 0)
                {
                    isAllItemDontUse = false;
                    break;
                }
            }

            List<DataUseItem> lstDt = new List<DataUseItem>();

            if (isAllItemDontUse)
            {
                lstDt = Helper.SelectionSortListSelf<DataUseItem>(datasUse, (a, b) =>
                {
                    if ((int)(((DataUseItem)a).priovity) < (int)(((DataUseItem)b).priovity))
                        return 1;
                    else
                        return -1;
                });
            }
            else
            {
                lstDt = Helper.SelectionSortListSelf<DataUseItem>(datasUse, (a, b) =>
                   {
                       if ((int)(((DataUseItem)a).NumUsed) < (int)(((DataUseItem)b).NumUsed))
                           return 1;
                       else
                           return -1;
                   });
            }
            Debug.Log("Suggest + " + datasUse.Count);
            //int minNumItem = UseProfile.GetNumItem(lstDt[0].typeItem);
            //int indexItem = 0;
            //for (int i = 1; i < lstDt.Count; i++)
            //{
            //    int numItem = UseProfile.GetNumItem(lstDt[i].typeItem);
            //    if (minNumItem > numItem)
            //    {
            //        indexItem = i;
            //        minNumItem = numItem;
            //    }
            //}
            Debug.Log("Suggest + " + lstDt[0].typeItem);
            return lstDt[0].typeItem;
        }

        public void ResetData()
        {
            for (int i = 0; i < datasUse.Count; i++)
            {
                datasUse[i].NumUsed = 0;
            }
            NumUsedItemAfterSugget = 0;
            IsBoughtOnSugget = (int)StateBuyItem.Default;
        }
    }
}

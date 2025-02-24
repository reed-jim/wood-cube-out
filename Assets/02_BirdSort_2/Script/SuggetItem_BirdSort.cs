namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Newtonsoft.Json;
    using DG.Tweening;

    public class SuggetItem_BirdSort : ItemBase
    {


        Level_BirdSort_2 level;
        public List<CloneData> lsCloneData;
        public List<StandingBase> lsStandingBase;
        public List<Solution> lsSolutions;
        public HashSet<string> visitedStates;
        private bool isSolved; // Trạng thái đã giải xong hay chưa
        private StandingBase standFrom;
        private StandingBase standTo;
        public GameObject hand;
        public bool handInBird;
        private Action<object> tempAction;


        public override void Init()
        {
            base.Init();
            
        }


        public int LimitBrand
        {
            get
            {
                switch (GamePlayController_BirdSort.Instance.level.typeStandInLevel)
                {
                    case TypeStand.Slot_4:
                        return 4;
                        break;
                    case TypeStand.Slot_5:
                        return 5;
                        break;
                    case TypeStand.Slot_6:
                        return 6;
                        break;
                    case TypeStand.Slot_7:
                        return 7;
                        break;
                    case TypeStand.Slot_8:
                        return 8;
                        break;
                }
                return 4;
            }
        }


        public void HandleSugget()
        {
            level = GamePlayController_BirdSort.Instance.level;
            handInBird = false;
            standFrom = new StandingBase();
            standTo = new StandingBase();
            lsCloneData = new List<CloneData>();
            lsStandingBase = new List<StandingBase>();
      

            for (int i = 0; i < level.standsInLevel_Real.Count; i++)
            {
                if (level.standsInLevel_Real[i].btnCick.activeSelf)
                {
                    lsStandingBase.Add(level.standsInLevel_Real[i]);
                }
            }
            for (int i = 0; i < lsStandingBase.Count; i++)
            {
                lsCloneData.Add(new CloneData() { id = lsStandingBase[i].ID, data = lsStandingBase[i].CompareBirdToId });
            }
            isSolved = false;
            lsSolutions = new List<Solution>();
            visitedStates = new HashSet<string>();
            FindSolution(lsCloneData);
            if (lsSolutions.Count > 0)
            {
                standFrom = level.GetStandingBaseReal(lsSolutions[0].From);
                standTo = level.GetStandingBaseReal(lsSolutions[0].To);
                standFrom.birdsOnArray[0].HandleOnActionSuggetBooster();
                standTo.HandleOnActionSuggetBooster();
                SpawnHand();
                tempAction = (param) => OnClickBrand((int)param);
                EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.EVENT_CLICK_STANDING, tempAction);



            }
            else
            {
                //GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                //  (
                // GamePlayController_BirdSort.Instance.gameScene.btnSuggetBooster.transform.position,
                //  "Use another booster",
                //  Color.white,
                //  isSpawnItemPlayer: true
                //  );


            }



        }
        public void HandleOffSugget()
        {
            if (standFrom != null && standTo != null)
            {
                hand.gameObject.SetActive(false);
                standFrom.birdsOnArray[0].HandleOffActionSuggetBooster();
                standTo.HandleOffActionSuggetBooster();
            }
        }
        private void FindSolution(List<CloneData> param)
        {



            for (int i = 0; i < param.Count; i++)
            {
                // Nếu thùng không rỗng

                if (param[i].data.Count > 0)
                {
                    if (param[i].data[0].isLock == false)
                    {
                        int color = param[i].data[0].id; // Lấy màu đầu tiên trong thùng            
                                                         // Duyệt qua các thùng khác
                        for (int j = 0; j < param.Count; j++)
                        {

                            if (i != j && param[j].data.Count < LimitBrand && (param[j].data.Count == 0 ))
                            {
                                lsSolutions.Add(new Solution() { From = param[i].id, To = param[j].id });

                            }
                        }
                    }


                }
            }
        }
        private void SpawnHand()
        {
            hand.gameObject.SetActive(true);
            hand.transform.position = standFrom.birdsOnArray[0].gameObject.transform.position;
            // hand.transform.parent = standFrom.birdsOnArray[0].gameObject.transform;

        }
        private void OnClickBrand(int idBrand)
        {




            if (standFrom != null && standTo != null)
            {
                if (handInBird && idBrand != standTo.ID)
                {
                    hand.gameObject.SetActive(false);
                    standTo.HandleOffActionSuggetBooster();
                    standFrom.birdsOnArray[0].HandleOffActionSuggetBooster();
                    EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.EVENT_CLICK_STANDING, tempAction);
                    BirdSort2.GamePlayController_BirdSort.Instance.gameScene.GetItem(GiftType.ShuffleItem_BirdSort).OffUseButton(true);
                    return;
                }

                if (idBrand != standFrom.ID && idBrand != standTo.ID && !handInBird)
                {
                    hand.gameObject.SetActive(false);
                    standFrom.birdsOnArray[0].HandleOffActionSuggetBooster();
                    standTo.HandleOffActionSuggetBooster();
                    EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.EVENT_CLICK_STANDING, tempAction);
                    BirdSort2.GamePlayController_BirdSort.Instance.gameScene.GetItem(GiftType.ShuffleItem_BirdSort).OffUseButton(true);
                    return;
                }

                if (idBrand == standTo.ID && handInBird)
                {
                    hand.gameObject.SetActive(false);
                    standTo.HandleOffActionSuggetBooster();
                    EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.EVENT_CLICK_STANDING, tempAction);
                    BirdSort2.GamePlayController_BirdSort.Instance.gameScene.GetItem(GiftType.ShuffleItem_BirdSort).OffUseButton(true);
                    return;
                }



                if (idBrand == standFrom.ID && !handInBird)
                {
                    // hand.transform.parent = standTo.slots[standTo.slots.Count - 1].gameObject.transform;
                    hand.transform.DOMove(standTo.slots[standTo.slots.Count - 1].gameObject.transform.position, 0.5f);
                    handInBird = true;
                    return;
                }





            }



        }



        public bool CanSugget
        {

            get
            {
                foreach (var item in level.standsInLevel_Real)
                {
                    if (!item.AllBirdIsInStand)
                    {
                        return false;
                    }
                }
                var standChosing = BirdSort2.GamePlayController_BirdSort.Instance.level.standChosing;
                if (standChosing != null)
                {
                    foreach (var item in standChosing.birdsOnArray)
                    {
                        item.DisableOutLine();
                    }
                    BirdSort2.GamePlayController_BirdSort.Instance.level.standChosing = null;
                }

                if (hand.activeSelf)
                {
                    return false;
                }
                
                if(BirdSort2.GamePlayController_BirdSort.Instance.tutorialController.isHasTutInLevel)
                {
                    return false;
                }
                

                BirdSort2.GamePlayController_BirdSort.Instance.gameScene.GetItem(GiftType.ShuffleItem_BirdSort).OffUseButton(false);
                HandleSugget();
                return true;
            }
        }


        protected override bool ActiveItemHandle()
        {
            return CanSugget;
        }

        public override bool IsCanUseItem()
        {
            throw new NotImplementedException();
        }

        public override void EndItem()
        {
          
        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Init();
                HandleSugget();
            }
        }


    }

    [System.Serializable]
    public class CloneData
    {
        public int id;
        public List<DataBirdInBrand> data;



    }
    [System.Serializable]
    public class Solution
    {
        public int From;
        public int To;
    }
    [System.Serializable]
    public class DataBirdInBrand
    {
        public int id;
        public bool isLock;
    }
}
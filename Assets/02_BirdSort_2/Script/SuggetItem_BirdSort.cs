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
      

           
            for (int i = 0; i < lsStandingBase.Count; i++)
            {
            }
            isSolved = false;
            lsSolutions = new List<Solution>();
            visitedStates = new HashSet<string>();
            FindSolution(lsCloneData);
            if (lsSolutions.Count > 0)
            {
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

        }
        private void OnClickBrand(int idBrand)
        {




            if (standFrom != null && standTo != null)
            {
                

            }



        }



        public bool CanSugget
        {

            get
            {
                return false;
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
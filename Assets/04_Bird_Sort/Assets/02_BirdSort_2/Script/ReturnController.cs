namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class DataReturn
    {
        public List<DataReturnModle> data;

        public DataReturn()
        {
            data = new List<DataReturnModle>();
        }
    }

    public class DataReturnModle
    {
        public BirdBase bird;

        public StandingBase oldStand;
        public int idOldSlot;

        public StandingBase newStand;
        public int idNewSlot;
    }

    public class ReturnController : MonoBehaviour
    {
        public Stack<DataReturn> stackReturn;

        private int m_isLockReturn;
        public bool IsLockReturn
        {
            get
            {
                return m_isLockReturn > 0;
            }
            set
            {
                if (value)
                {
                    m_isLockReturn++;
                    GamePlayController_BirdSort.Instance.itemController.InteractButton(false);
                    GamePlayController_BirdSort.Instance.itemController.suggetUseItemInGame.ResetSugget();

                }
                else
                {
                    m_isLockReturn--;
                    if (m_isLockReturn < 0)
                        m_isLockReturn = 0;
                    if (m_isLockReturn == 0)
                    {
                        GamePlayController_BirdSort.Instance.itemController.InteractButton(true);

                    }
                }
            }

        }

        public void Init()
        {
            stackReturn = new Stack<DataReturn>();

        }

        public bool IsHasReturn()
        {
            return stackReturn.Count > 0;
        }    

        public void AddStack(DataReturn data)
        {
            stackReturn.Push(data);
        }

        public void Return()
        {
           
            var data = stackReturn.Pop();

            if (data != null)
            {
                IsLockReturn = true;
                GamePlayController_BirdSort.Instance.tutorialController.OnClickHintReturnBtn();
                GamePlayController_BirdSort.Instance.extralPlayController.ReturnExtrals();
                bool isReturnScore = false;
                int idBirdBack = -1;
                int idStandBack = -1;
                for (int i = 0; i < data.data.Count; i++)
                {
                    int index = i;

                    //  Debug.Log("============data.data ========= " + data.data[index].bird.gameObject.GetInstanceID());
                    if (data.data[index].newStand != null)
                    {
                        if (data.data[index].newStand.birdsOn.Count > 0)
                        {
                            var birdFly = data.data[index].newStand.birdsOn.Pop();
                            if (birdFly != null)
                            {
                                if (data.data[index].oldStand != null)
                                    data.data[index].oldStand.BackTurn(birdFly, isOldStand: true);
                            }
                        }
                        else
                        {
                            if (data.data[index].oldStand != null)
                                data.data[index].oldStand.BackTurn(data.data[index].bird, isOldStand: true);
                        }
                    }
                    else
                    {
                        if (data.data[index].oldStand != null)
                        {
                            data.data[index].oldStand.BackTurn(data.data[index].bird, isOldStand: true);
                            isReturnScore = true;
                            idBirdBack = data.data[index].bird.id;
                            idStandBack = data.data[index].oldStand.ID;
                        }
                    }
                }
                if (isReturnScore)
                {
                    GamePlayController_BirdSort.Instance.extralPlayController.ReAppearExtrals(idBirdBack, idStandBack);
                }
               
            }
       
        }

        public bool IsHasBirdInSaveElement(int idBird, DataReturn data)
        {
            for (int i = 0; i < data.data.Count; i++)
            {
                if (idBird == data.data[i].bird.gameObject.GetInstanceID())
                    return true;
            }

            return false;
        }
        public void ResetReturn()
        {
            stackReturn.Clear();
            m_isLockReturn = 0;
        }
    }

}
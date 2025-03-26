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
using BirdSort2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DataReturn
{
    public BirdBase bird;
    public SlotTran slot;
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

    public void AddStack(DataReturn data)
    {
        stackReturn.Push(data);
    }

    public void ResetStack()
    {
        stackReturn.Clear();
    }    

    public bool Return(bool isUse = true)
    {
        if (GamePlayController.Instance.state != StateGame.Playing)
            return false;
        if (IsLockReturn)
            return false;
        if (stackReturn.Count <= 0)
        {
            return false;
        }

        bool isHasMatch3 = true;

        while (isHasMatch3)
        {
            if (stackReturn.Count <= 0)
            {
                return false;
            }
            var data = isUse ? stackReturn.Pop() : stackReturn.Peek();

            if (data != null)
            {
                if (data.bird.state != StateBird.Match3)
                {
                    if (isUse)
                    {
                        isHasMatch3 = false;
                        data.bird.ComeToGadern();
                        var slotControler = GamePlayController.Instance.level.slotController;
                        for (int i = 0; i < slotControler.slots.Count; i++)
                        {
                            int index = i;
                            if (slotControler.slots[index].IsEmpty)
                                continue;
                            if (slotControler.slots[index].birdOn.gameObject.GetInstanceID() == data.bird.gameObject.GetInstanceID())
                            {
                                slotControler.slots[index].birdOn = null;
                                slotControler.ReSortSlot(slotControler.slots[index].id);
                                break;
                            }

                        }
                    }
                    ;
                    GamePlayController.Instance.itemController.suggetUseItemInGame.ResetSugget();
                    return true;
                }
            }
        }

        return false;

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            Return();
    }
}

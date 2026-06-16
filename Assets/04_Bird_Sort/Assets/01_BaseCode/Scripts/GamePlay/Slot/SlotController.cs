using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SlotController : MonoBehaviour
{
    public List<SlotTran> slots;
    public Image waringSlot;

    public void Init()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int index = i;
            slots[i].Init(index);
        }
    }

    public bool IsHasSlot()
    {

        bool isHasSlot = false;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                isHasSlot = true;
                break;
            }
        }

        if (!isHasSlot)
            return false;
        else
            return true;
    }

    public int GetNumSlotEmpty()
    {
        int numSlot = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                numSlot++;
            }
        }

        return numSlot;
    }

    public SlotTran CheckSlot(BirdBase birdCome)
    {
        SlotTran resultSlot = null;

        bool isHasSlot = false;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                isHasSlot = true;
                break;
            }
        }

        if (!isHasSlot)
            return null;

        bool isHasBirdSame = false;
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            int index = i;
            if (slots[index].IsEmpty)
                continue;
            if (index + 1 >= slots.Count)
                continue;

            if (slots[index].birdOn.id == birdCome.id)
            {
                isHasBirdSame = true;
                if (!slots[index + 1].IsEmpty)
                {
                    //Dịch bird
                    for (int j = index + 1; j < slots.Count - 1; j++)
                    {
                        int _in = j;

                        if (!slots[_in].IsEmpty)
                            slots[_in].birdOn.FlyToSlot(slots[_in + 1].posMiddle.position, slots[_in + 1].posTrans.position, slots[_in + 1].indexLayer, () =>
                            {
                                GamePlayController.Instance.level.CheckMatch3Bird();
                            });
                    }

                    for (int j = slots.Count - 1; j > index + 1; j--)
                    {
                        slots[j].birdOn = slots[j - 1].birdOn;
                    }

                }
                GamePlayController.Instance.level.CheckRequiremntTicked(birdCome.id);
                slots[index + 1].birdOn = birdCome;
                resultSlot = slots[index + 1];
                birdCome.FlyToSlot(slots[index + 1].posMiddle.position, slots[index + 1].posTrans.position, slots[index + 1].indexLayer, () => { }, actionComeDontDestry: () =>
                 {
                     GamePlayController.Instance.level.CheckMatch3Bird();
                 }, actionScaleDone: () => { GamePlayController.Instance.returnController.IsLockReturn = false; });
                break;
            }
        }
        if (!isHasBirdSame)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                int index = i;
                if (slots[index].IsEmpty)
                {
                    slots[index].birdOn = birdCome;
                    GamePlayController.Instance.level.CheckRequiremntTicked(birdCome.id);
                    birdCome.FlyToSlot(slots[index].posMiddle.position, slots[index].posTrans.position, slots[index].indexLayer, () => { }, actionComeDontDestry: () =>
                    {
                        GamePlayController.Instance.level.CheckMatch3Bird();
                    }, actionScaleDone: () => { GamePlayController.Instance.returnController.IsLockReturn = false; });
                    resultSlot = slots[index];
                    break;
                }
            }
        }
        CheckWaring();
        return resultSlot;
    }

    public IEnumerator ReloadSlot(System.Func<bool> condition)
    {
        GamePlayController.Instance.returnController.IsLockReturn = false;
        yield return new WaitUntil(condition);
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
            {
                if (slots[i].birdOn.state == StateBird.Match3)
                {
                    slots[i].birdOn = null;
                }
            }
        }

        //Dịch các bird lên
        List<BirdBase> lstTempBirds = new List<BirdBase>();
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
            {
                lstTempBirds.Add(slots[i].birdOn);
            }
            slots[i].birdOn = null;
        }

        for (int i = 0; i < lstTempBirds.Count; i++)
        {
            int index = i;
            slots[index].birdOn = lstTempBirds[index];
            if (Vector2.Distance(slots[index].birdOn.transform.position, slots[index].posTrans.position) > 0.5f)
                slots[index].birdOn.FlyToSlot(slots[index].posMiddle.position, slots[index].posTrans.position, slots[index].indexLayer, () =>
                {
                    GamePlayController.Instance.level.CheckMatch3Bird();
                });
        }


        CheckWaring();
    }

    public void ReSortSlot(int idSlot)
    {
        //Dịch bird lên trước
        for (int j = idSlot + 1; j < slots.Count; j++)
        {
            int _in = j;

            if (!slots[_in].IsEmpty)
                slots[_in].birdOn.FlyToSlot(slots[_in - 1].posMiddle.position, slots[_in - 1].posTrans.position, slots[_in - 1].indexLayer, () =>
                {
                    //GamePlayController.Instance.level.CheckMatch3Bird();
                });
        }

        for (int j = idSlot; j < slots.Count - 1; j++)
        {
            slots[j].birdOn = slots[j + 1].birdOn;
        }
        CheckWaring();
    }

    public void ResetSlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int index = i;
            if (slots[index].IsEmpty)
                continue;

            slots[index].birdOn.ComeToGadern();
            slots[index].birdOn = null;
        }
        CheckWaring();
    }

    public void CheckWaring()
    {
        int numSlotHas = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
                numSlotHas++;
        }

        if (numSlotHas >= 6)
        {
            waringSlot.gameObject.SetActive(true);
            waringSlot.DOKill();
            waringSlot.DOFade(0.25f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            waringSlot.DOKill();
            waringSlot.DOFade(0f, 0.2f).OnComplete(() => { waringSlot.gameObject.SetActive(false); });
        }
    }

    /// <summary>
    /// Cảnh báo nguy hiểm khi còn 1 slot mà kó có 1 cặp giống nhau trong 7 slot
    /// </summary>
    /// <returns></returns>
    public bool IsWarningDanger()
    {
        Dictionary<int, int> coupleBirdDict = new Dictionary<int, int>();//<ID, số lượng>

        int numSlotHas = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
            {
                numSlotHas++;

                if (coupleBirdDict.ContainsKey(slots[i].birdOn.id))
                {
                    coupleBirdDict[slots[i].birdOn.id]++;
                }
                else
                {
                    coupleBirdDict.Add(slots[i].birdOn.id, 1);
                }
            }
        }

        if (numSlotHas >= 6)
        {
            //Tìm cặp giống nhau
            bool isHasCouple = false;
            foreach (var item in coupleBirdDict)
            {
                if (item.Value > 1)
                {
                    isHasCouple = true;
                    return false;
                }
            }

            if (!isHasCouple)
                return true;
        }

        return false;
    }
}

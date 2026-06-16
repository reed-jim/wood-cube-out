using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using static SaleSegemntData;

public class EndLessSaleBox : BaseBox
{
    public static EndLessSaleBox instance;
    public static EndLessSaleBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<EndLessSaleBox>(PathPrefabs.ENDLESS_SALE_BOX));
            instance.InitState();
        }
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }



    [SerializeField] private Text Timetxt;
    [SerializeField] private List<Transform> ItemPos;
    [Header("Process")]
    [SerializeField] private Slider process;
    [SerializeField] private Text goal;
    [SerializeField] private Text levelPack;

    public List<EndlessElement> lstElements;

    private EventBase eventSaleController;
    private EndlessSale config;

    [SerializeField] private RectTransform parentRect;
    [SerializeField] private Button btnInfor;
    bool isCounting = false;
    int indexClaim = -1;

    private float CurrentProgessShow
    {
        get
        {
            return PlayerPrefs.GetFloat("Current_Item", 0);
        }
        set
        {
            GPlayerPrefs.SetFloat("Current_Item", value);
            GPlayerPrefs.Save();
        }
    }

    public int CurrentIndex
    {
        get
        {
            return PlayerPrefs.GetInt("Current_Index", 0);
        }
        set
        {
            GPlayerPrefs.SetInt("Current_Index", value);
            GPlayerPrefs.Save();
        }
    }

    public int StartPack
    {
        get
        {
            return PlayerPrefs.GetInt("Start_Pack", 0);
        }
        set
        {
            GPlayerPrefs.SetInt("Start_Pack", value);
            GPlayerPrefs.Save();
        }
    }

    public bool isPlayingAnim = false;

    public float moveDuration = 0.125f;        // th?i gian head di chuy?n t? di?m i ? i+1

    private void Update()
    {
        if (!isCounting) return;

        if (eventSaleController != null)
        {
            Timetxt.text = TimeManager.ShowTime6((long)eventSaleController.TimeLeft);

            if (eventSaleController.TimeLeft <= 0)
            {
                Timetxt.text = "00:00:00";
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_HOME);
                isCounting = false;
                Close();
            }
        }
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Close()
    {
        if (isPlayingAnim)
        {
            return;
        }
        base.Close();
    }
    public void InitState()
    {
        config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive();
        eventSaleController = BirdSortGameController.Instance.eventSaleController.GetEventSales(config.gameTheme);

        isCounting = true;

        //set value
        foreach (var item in lstElements)
        {
            item.numPos = lstElements.IndexOf(item);
            item.lstReward = config.rewardData[item.numPos].rewardData;
            item.requireToken = config.rewardData[item.numPos].requireToken;
            item.Init();
        }

        foreach (var item in lstElements)
        {
            indexClaim = -1;
            if (config.rewardData[item.numPos].iapPackName != TypePackIAP.None)
            {
                indexClaim = lstElements.IndexOf(item);
                if (indexClaim <= StartPack)
                    continue;
                break;
            }
        }

        //StartCoroutine(MoveAlongPath(StartPack, EndPack));
        levelPack.text = (CurrentIndex + 1).ToString();
        if (CurrentIndex + 1 <= eventSaleController.config.rewardData.Count)
        {
            int currentProgessShow = CurrentIndex > 0 ? Mathf.Abs(eventSaleController.data.RememberCurrentNumItem - eventSaleController.config.rewardData[CurrentIndex - 1].requireToken) : eventSaleController.data.RememberCurrentNumItem;
            int currentItemRequirement = 0;
            currentItemRequirement = eventSaleController.config.rewardData[CurrentIndex].requireToken;
            int targetShow = 0;

            if (CurrentIndex == 0)
                targetShow = eventSaleController.config.rewardData[CurrentIndex + 1].requireToken;
            else
                targetShow = Mathf.Abs(currentItemRequirement - eventSaleController.config.rewardData[CurrentIndex - 1].requireToken);


            goal.text = currentProgessShow + "/" + targetShow;

            SetProgessBarImg((float)currentProgessShow / targetShow, false);

            setPackNoAnim(StartPack, StartPack + 6);
        }
        else
        {
            EndlessDone(false);
            if (HomeController.Instance.eventBaseView.controller.EndlessEnd)
            {
                EndlessClaimDone(false);
            }
            else
            {
                setPackNoAnim(StartPack, StartPack + 6);
            }
        }


        btnInfor.onClick.AddListener(() => EndLessTutBox.Setup().Show());
    }

    IEnumerator MoveAlongPath(int start = 0, int end = 6)
    {
        for (int i = ItemPos.Count - 1; i >= 0; i--)
        {
            Vector2 target = ItemPos[i].GetComponent<RectTransform>().anchoredPosition;

            // tween head
            lstElements[start].GetComponent<RectTransform>().DOAnchorPos(target, moveDuration).SetEase(Ease.Linear);
            lstElements[start].transform.DOScale(1f, moveDuration).SetEase(Ease.Linear);

            // tween các segment theo v? trí mà head dã di tru?c dó v?i delay tang d?n
            for (int s = start + 1; s < end; s++)
            {
                if (i == 0 || s - start > i)
                {
                    break;
                }
                float delay = moveDuration * s;
                // luu v? trí head hi?n t?i (phiên b?n cu) d? segment theo
                Vector2 followPos = ItemPos[i].GetComponent<RectTransform>().anchoredPosition;
                lstElements[s].GetComponent<RectTransform>().DOAnchorPos(followPos, moveDuration).SetEase(Ease.Linear).SetDelay(delay);
                lstElements[s].transform.DOScale(1f, moveDuration).SetEase(Ease.Linear).SetDelay(delay);
            }

            // d?i head hoàn thành tru?c khi di next
            yield return new WaitForSeconds(moveDuration);
        }
        yield return new WaitForSeconds(moveDuration);

        SetStatusPack(start);
    }

    void setPackNoAnim(int start = 0, int end = 6)
    {
        isPlayingAnim = true;
        for (int i = ItemPos.Count - 1; i >= 0; i--)
        {
            Vector2 target = ItemPos[i].GetComponent<RectTransform>().anchoredPosition;

            // tween head
            lstElements[start].GetComponent<RectTransform>().anchoredPosition = target;
            lstElements[start].transform.localScale = Vector3.one;

            // tween các segment theo v? trí mà head dã di tru?c dó v?i delay tang d?n
            for (int s = start + 1; s < end; s++)
            {
                if (i == 0 || s - start > i || s > lstElements.Count - 1)
                {
                    break;
                }
                //float delay = moveDuration * s;
                // luu v? trí head hi?n t?i (phiên b?n cu) d? segment theo
                Vector2 followPos = ItemPos[i].GetComponent<RectTransform>().anchoredPosition;
                lstElements[s].GetComponent<RectTransform>().anchoredPosition = followPos;
                lstElements[s].transform.localScale = Vector3.one;
            }
        }
        if (CurrentIndex == 0)
            DoUpLevel();
        else
            StartCoroutine(ShowProgess());

    }

    public void SetUpPack(int index)
    {
        if (index == eventSaleController.config.rewardData.Count - 1)
        {
            lstElements[index].transform.DOScale(0f, moveDuration / 2).SetEase(Ease.Flash).OnComplete(() =>
            {
                EndlessClaimDone();
            });
        }
        else
        {
            lstElements[index].transform.DOScale(0f, moveDuration / 2).SetEase(Ease.Flash).OnComplete(() =>
            {
                for (int i = 0; i < ItemPos.Count; i++)
                {
                    if (index + 1 + i > lstElements.Count - 1)
                    {
                        continue;
                    }
                    Vector2 target = ItemPos[i].GetComponent<RectTransform>().anchoredPosition;
                    lstElements[index + 1 + i].transform.DOScale(1f, moveDuration).SetEase(Ease.Linear);
                    if (i == 0)
                    {
                        lstElements[index + 1 + i].GetComponent<RectTransform>().DOAnchorPos(target, moveDuration).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            StartPack = index + 1;
                            foreach (var item in lstElements)
                            {
                                indexClaim = -1;
                                if (config.rewardData[item.numPos].iapPackName != TypePackIAP.None)
                                {
                                    indexClaim = lstElements.IndexOf(item);
                                    if (indexClaim <= StartPack)
                                        continue;
                                    break;
                                }
                            }
                            StartCoroutine(ShowProgess());
                        });
                    }
                    else
                    {
                        lstElements[index + 1 + i].GetComponent<RectTransform>().DOAnchorPos(target, moveDuration).SetEase(Ease.Linear);
                    }
                }
            });
        }

    }

    void SetStatusPack(int index, bool anim = true)
    {
        if (index < eventSaleController.data.CurrentIndexReward && (index <= indexClaim || indexClaim == -1))
        {
            lstElements[index].setActivePackage(anim);
            if (index == eventSaleController.config.rewardData.Count - 1)
            {
                EndlessDone();
            }
        }
        isPlayingAnim = false;
    }

    [SerializeField]
    private RectTransform back;
    [SerializeField]
    private RectTransform Tick;
    [SerializeField]
    private Image GoalImg;
    [SerializeField]
    private Sprite doneImg;
    [SerializeField]
    private GameObject TickDone;

    void EndlessDone(bool anim = true)
    {
        if (anim)
        {
            GoalImg.transform.DOScale(0f, 0.3f).OnComplete(() =>
            {
                GoalImg.sprite = doneImg;
                TickDone.SetActive(true);
                levelPack.gameObject.SetActive(false);
                GoalImg.transform.DOScale(1f, 0.3f);
            });
        }
        else
        {
            process.value = 1f;
            GoalImg.sprite = doneImg;
            TickDone.SetActive(true);
            levelPack.gameObject.SetActive(false);
            goal.gameObject.SetActive(false);
        }
    }

    void EndlessClaimDone(bool anim = true)
    {
        if (anim)
        {
            back.DOKill();
            Sequence seq = DOTween.Sequence();

            // 1?? N?Y LÊN NH? TRU?C
            seq.Append(
                back.DOAnchorPosY(165f, 0.3f)
                     .SetEase(Ease.OutBack, 3f)
            );

            // 2?? B? KÉO TU?T XU?NG C?C NHANH
            seq.Append(
                back.DOAnchorPosY(-5000f, 0.5f)
                     .SetEase(Ease.InExpo)
            ).OnComplete(() =>
            {
                Tick.DOScale(3f, 0.5f).SetEase(Ease.InExpo);
            });

            seq.Append(
         back.DOShakeAnchorPos(0.08f, 20f, 16));
        }
        else
        {
            back.anchoredPosition = new Vector2(back.anchoredPosition.x, -1350f);
            Tick.DOScale(3f, 0.5f).SetEase(Ease.InExpo);
        }
        HomeController.Instance.eventBaseView.controller.EndlessEnd = true;
        isPlayingAnim = false;
    }

    protected IEnumerator ShowProgess()
    {
        yield return new WaitForSeconds(0.5f);
        int currentItemRequirement = 0;
        int currentProgessShow = 0;
        int levelup = 0;
        if (CurrentIndex < eventSaleController.config.rewardData.Count)
        {
            currentItemRequirement = eventSaleController.config.rewardData[CurrentIndex].requireToken;
        }
        else
        {
            currentItemRequirement = eventSaleController.config.rewardData[CurrentIndex - 1].requireToken;
        }

        if ((indexClaim == -1 || CurrentIndex <= indexClaim) && (StartPack + 1 > CurrentIndex || CurrentIndex + 1 == eventSaleController.config.rewardData.Count))
        {
            if (eventSaleController.data.CurrentNumItem >= eventSaleController.config.rewardData[CurrentIndex].requireToken)
            {
                DoUpLevel();
                if (StartPack != CurrentIndex)
                {
                    levelup = 1;
                }
                currentProgessShow = eventSaleController.data.CurrentNumItem - eventSaleController.config.rewardData[CurrentIndex - levelup].requireToken;
                if (CurrentIndex < eventSaleController.config.rewardData.Count - 1)
                {
                    currentItemRequirement = eventSaleController.config.rewardData[CurrentIndex + 1 - levelup].requireToken;
                }
                else
                {
                    currentItemRequirement = eventSaleController.config.rewardData[CurrentIndex - levelup].requireToken;
                }

            }
            else
            {
                currentProgessShow = eventSaleController.data.CurrentNumItem - eventSaleController.config.rewardData[CurrentIndex - 1].requireToken;
                SetStatusPack(StartPack, false);
            }
        }
        else
        {
            currentProgessShow = eventSaleController.data.CurrentNumItem - eventSaleController.config.rewardData[CurrentIndex - 1].requireToken;
            SetStatusPack(StartPack, false);
        }

        int temp = CurrentIndex;
        if (CurrentIndex < eventSaleController.config.rewardData.Count)
        {
            temp = CurrentIndex;
        }
        else
        {
            temp = CurrentIndex - 1;
        }
        //currentProgessShow = CurrentIndexReward - yêu c?u c?a m?c tru?c dó
        int targetShow = Mathf.Abs(currentItemRequirement - eventSaleController.config.rewardData[temp - levelup].requireToken);
        if (targetShow == 0)
            targetShow = eventSaleController.config.rewardData[temp].requireToken - eventSaleController.config.rewardData[temp - 1].requireToken;
        if (CurrentIndex + 1 > eventSaleController.config.rewardData.Count)
        {
            goal.DOFade(0f, 0.5f).OnComplete(() => goal.gameObject.SetActive(false));
            currentProgessShow = targetShow;
        }
        goal.text = currentProgessShow + "/" + targetShow;
        SetProgessBarImg((float)currentProgessShow / targetShow, true, 0.5f, actionProgessDone: () =>
        {
            eventSaleController.data.RememberCurrentNumItem = eventSaleController.data.CurrentNumItem;
        });

    }

    protected void SetProgessBarImg(float fillAmountValue, bool isAnim = true, float delay = 0.1f, UnityAction actionProgessDone = null)
    {
        if (fillAmountValue > 1)
            fillAmountValue = 1;
        if (fillAmountValue < 0)
            fillAmountValue = 0;

        if (isAnim)
        {
            process.DOValue(fillAmountValue, 0.75f).OnComplete(() => { actionProgessDone?.Invoke(); }).SetDelay(delay);
        }
        else
        {
            process.value = fillAmountValue;
        }

    }

    void DoUpLevel()
    {
        UnityAction actionFillDone = () =>
        {
            CurrentIndex++;
            levelPack.transform.parent.DOScale(1.2f, 0.5f).OnComplete(() =>
            {
                levelPack.text = (CurrentIndex + 1).ToString();
                levelPack.transform.parent.DOScale(1f, 0.5f);
            });
            process.value = 0;
            SetStatusPack(StartPack);
        };

        if (CurrentIndex != eventSaleController.data.CurrentIndexReward && (StartPack + 1 > CurrentIndex || CurrentIndex == eventSaleController.config.rewardData.Count))
        {
            if (process.value >= 0.9f)
            {
                actionFillDone?.Invoke();
            }
            else
            {
                process.DOValue(1f, 0.5f).OnComplete(() =>
                {
                    actionFillDone?.Invoke();
                });
            }
        }
        else
        {
            SetStatusPack(StartPack, false);
        }

    }
    public override void CreateInstance()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<EndLessSaleBox>(PathPrefabs.ENDLESS_SALE_BOX));
            instance.InitState();
        }
        instance.isPostEventPopupClose = true;
        instance.Show();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class ItemUI : MonoBehaviour
{
    public GiftType typeItem;

    [SerializeField] protected GameObject bodyUnlock;
    [SerializeField] protected Image iconItemImg;
    [SerializeField] protected Text numItemTxt;
    [SerializeField] protected Button useBtn;
    [SerializeField] protected GameObject addObj;
    [SerializeField] protected GameObject useObj;

    [SerializeField] protected GameObject lockObj;
    [SerializeField] protected Text levelUnlockTxt;
    protected int levelUnlock;

    [SerializeField] protected GameObject unlimitedObj;
    [SerializeField] protected Text timeCoolDownUnlimited;

    [Header("Sugget Use")]
    [SerializeField] protected Image fxSuggetUse;
    public Sprite hightLightItem;
    [SerializeField] protected GameObject iconScaleObj;

    protected float coolDownTime;
    protected bool isCooldowning;

    protected Action<object> actionReloadItem;

    public virtual void Init(GiftType typeItem)
    {
        actionReloadItem = (s) => ReloadItem((GiftType)s);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.BUYED_ITEM, actionReloadItem);

        var data = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig.GetBoot(typeItem);
        if (data != null)
        {
            levelUnlock = data.unlockLevel;
        }

        this.typeItem = typeItem;
        iconItemImg.sprite = GameController.Instance.dataContain.giftDatabase.GetIconItem(typeItem);
        useBtn.onClick.AddListener(UseItem);
        //addBtn.onClick.AddListener(OpenShop);
        InitState();
    }

    public virtual void InitState()
    {
     
        numItemTxt.text = UseProfile.GetNumItem(typeItem).ToString();

        useObj.gameObject.SetActive(false);
        addObj.gameObject.SetActive(false);
        numItemTxt.gameObject.SetActive(false);

        int numItem = UseProfile.GetNumItem(typeItem);
        if (numItem > 0)
        {
            useObj.gameObject.SetActive(true);
            numItemTxt.gameObject.SetActive(true);
        }
        else
        {
            addObj.gameObject.SetActive(true);
        }

        if (UseProfile.CurrentLevel < levelUnlock)
        {
            lockObj.gameObject.SetActive(true);
            levelUnlockTxt.text = string.Format(Localization.Get("s_unlock_level"), levelUnlock);
            bodyUnlock.gameObject.SetActive(false);
        }
        else
        {
            lockObj.gameObject.SetActive(false);
            bodyUnlock.gameObject.SetActive(true);
        }

        LoadUnlimited();
    }

    protected virtual void UseItem()
    {
        if (UseProfile.CurrentLevel < levelUnlock)
            return;
        int numItem = UseProfile.GetNumItem(typeItem);
        bool isUnlimited = UseProfile.IsUnlimitedItem(typeItem);

        if (numItem > 0 || isUnlimited)
        {
            if (BirdSort2.GamePlayController_BirdSort.Instance.itemController.UseItem(typeItem))
            {
                GameController.Instance.dataContain.giftDatabase.Claim(typeItem, -1, Reason.in_campain_level);

                InitState();

                if (BirdSort2.GamePlayController_BirdSort.Instance != null)
                {
                    BirdSort2.GamePlayController_BirdSort.Instance.tutItemInGameController.NextTut();
                }
            }
            if (!BirdSort2.UseProfile.hadBuySuggest)
                BirdSort2.GamePlayController_BirdSort.Instance.suggetBuyItemController.OnUseItem(typeItem);
        }
        else
        {
            //Mở shop
            OpenShop();
        }
        GameController.Instance.musicManager.PlayClickSound();
    }

    protected virtual void OpenShop()
    {
        // if (BirdSort2.GamePlayController_BirdSort.Instance != null)
        // {
        //     BirdSort2.GamePlayController_BirdSort.Instance.level.PauseLevel(true);
        //     var box = BuyItemBox.Setup();
        //     box.Show(typeItem);

        //     box.OnCloseBox = () =>
        //     {
        //         BirdSort2.GamePlayController_BirdSort.Instance.level.PauseLevel(false);
        //     };
        // }
        // else
        // {
        //     BuyItemBox.Setup().Show(typeItem);
        // }


    }

    protected virtual void ReloadItem(GiftType typeItem)
    {
        if (this.typeItem != typeItem)
            return;
        InitState();
    }

    protected virtual void LoadUnlimited()
    {
        if (UseProfile.IsUnlimitedItem(typeItem))
        {
            unlimitedObj.gameObject.SetActive(true);
            isCooldowning = true;
            coolDownTime = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), UseProfile.GetTimeEndUnlimitedItem(typeItem));
        }
        else
        {
            unlimitedObj.gameObject.SetActive(false);
            isCooldowning = false;
        }
    }

    protected Sequence suggetUseSequence;
    public virtual void SuggetUseItem(bool isSugget)
    {
        if (isSugget)
        {
            fxSuggetUse.gameObject.SetActive(true);
            fxSuggetUse.sprite = hightLightItem;

            if (suggetUseSequence != null)
                suggetUseSequence.Kill();

            iconScaleObj.transform.DOKill();


            suggetUseSequence = DOTween.Sequence();
            suggetUseSequence.Append(iconScaleObj.transform.DOScale(1.2f, 0.3f).OnComplete(() =>
            {
                iconScaleObj.transform.DOKill();
                iconScaleObj.transform.DOLocalRotate(new Vector3(0, 0, 20), 0.15f).SetLoops(6, LoopType.Yoyo);
                iconScaleObj.transform.DOScale(1f, 0.3f).SetDelay(0.9f);
            }));
            suggetUseSequence.AppendInterval(2f);
            suggetUseSequence.SetLoops(-1, LoopType.Restart);
        }
        else
        {
            fxSuggetUse.gameObject.SetActive(false);
            if (suggetUseSequence != null)
                suggetUseSequence.Kill();
            iconScaleObj.transform.DOKill();
            iconScaleObj.transform.localScale = Vector3.one;
            iconScaleObj.transform.localEulerAngles = Vector3.zero;
        }
    }

    protected void Update()
    {
        if (isCooldowning)
        {
            coolDownTime -= Time.unscaledDeltaTime;
            timeCoolDownUnlimited.text = TimeManager.ShowTime2((long)coolDownTime);
            if (coolDownTime <= 0)
            {
                LoadUnlimited();
            }
        }
    }

    public void OffUseButton(bool param)
    {
        useBtn.interactable = param;
    }

    protected virtual void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.BUYED_ITEM, actionReloadItem);
    }

    public void InteractBtnUse(bool isInteract)
    {
        useBtn.interactable = isInteract;
        if (isInteract)
        {
            iconItemImg.DOKill();
            iconItemImg.DOColor(new Color(1, 1, 1, 1), 0.3f);
        }   
        else
        {
            float ap = 180f / 255;
            iconItemImg.DOKill();
            iconItemImg.DOColor(new Color(ap, ap, ap, 1), 0.3f);
        }    
    }    

    public void SetActiveBtn(bool isActive)
    {
        if(isActive)
        {
            this.gameObject.SetActive(true);
            this.transform.DOKill();
            this.transform.localScale = Vector3.zero;
            this.transform.DOScale(1, 0.3f);
        }
        else
        {
            this.transform.DOKill();
            this.transform.DOScale(0, 0.3f).OnComplete(() => { this.gameObject.SetActive(false); });
        }    
    }    
}

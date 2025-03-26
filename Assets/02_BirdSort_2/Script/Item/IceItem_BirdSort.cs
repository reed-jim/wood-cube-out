using System.Collections;
using System.Collections.Generic;
using BirdSort2;
using UnityEngine;
using DG.Tweening;


public class IceItem_BirdSort : ItemBase
{
    private bool isUsing;
    private float timer;
    [SerializeField] private int timeIce = 10;

    public static bool isUseIce;
    public CanvasGroup iceFXObj;

    [SerializeField] protected AudioClip iceClip;
    [SerializeField] protected AudioSource iceSource;

    public override void Init()
    {
        base.Init();
        isUseIce = false;
    }

    public override bool ActiveItem()
    {
        if (isUsing)
            return false;

        return base.ActiveItem();
    }

    protected override bool ActiveItemHandle()
    {
        isUsing = true;
        isUseIce = true;
        timer = 0;
        ActiveFX(true);
        BirdSort2.GamePlayController_BirdSort.Instance.itemController.suggetUseItemInGame.ResetSugget();
        return true;
    }

    public override void EndItem()
    {
        isUsing = false;
        timer = 0;
        isUseIce = false;
        ActiveFX(false);
      
        
        var textTime = BirdSort2.GamePlayController_BirdSort.Instance.gameScene.timePlayLevelTxt;
        
        textTime.color = new Color32(255, 255, 255, 255);
    }

    public void ActiveFX(bool isActive)
    {
        if (isActive)
        {
            //Diễn FX
          
            var itemUI = BirdSort2.GamePlayController_BirdSort.Instance.gameScene.itemsUI[typeItem];

            var textTime = BirdSort2.GamePlayController_BirdSort.Instance.gameScene.timePlayLevelTxt;

            var itemFX = BirdSort2.GamePlayController_BirdSort.Instance.itemController.view.GetItemFX();
            itemFX.transform.position = itemUI.transform.position;
            itemFX.Show(
                typeItem,
                -1,
                textTime.gameObject.transform.position,
                10,
                () =>
                {
                    isUsing = true;
                    isUseIce = true;
                    timer = 0;
                
                    textTime.color = new Color32(4,195,254,255);
                    textTime.transform.DOKill();
                    textTime.transform.DOScale(1.1f, 0.3f).OnComplete(() =>
                    {
                        textTime.transform.DOScale(1f, 0.3f);

                        iceFXObj.gameObject.SetActive(true);
                        iceFXObj.DOKill();
                        iceFXObj.DOFade(1, 0.5f);
                      //  GameController.Instance.musicManager.PlayOneShot(iceClip, iceSource);
                    });

                },
                isTime: false);
        }
        else
        {
            iceFXObj.DOKill();
            iceFXObj.DOFade(0, 0.5f).OnComplete(() =>
            {
                iceFXObj.gameObject.SetActive(false);
            });
        }
    }

    private void Update()
    {
      
        if (!isUsing)
            return;
        if (GamePlayController_BirdSort.Instance.state != BirdSort2.StateGame.Playing)
        {
            return;
        }
        timer += Time.deltaTime;
        if (timer >= timeIce)
        {
            EndItem();
        }
    }

    public override bool IsCanUseItem()
    {
        if (UseProfile.CurrentLevel
            < GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig.GetBoot(GiftType.AddBox).unlockLevel)
            return false;

        if (UseProfile.GetNumItem(typeItem) <= 0)
            return false;

        if (isUsing)
            return false;

        return true;
    }
}

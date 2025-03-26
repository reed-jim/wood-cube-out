using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CoreGameData;
using DG.Tweening;

public class TutorialItemInGame : TutorialBase
{
    public const string TUT_ITEM_UI = "tut_item_ui";

    [SerializeField] private GameObject parentTut;
    [SerializeField] private Image panelBlack;
    public GiftType typeItem;
    private BootsConfig bootConfig;
    [SerializeField] private GameObject tutObj;


    public override void Init(TutorialFunController controller)
    {
        base.Init(controller);
        parentTut.gameObject.SetActive(false);
        tutObj.gameObject.SetActive(false);

        bootConfig = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig;
    }

    public override bool IsCanShowTut()
    {
        if (UseProfile.CurrentLevel != bootConfig.GetBoot(typeItem).unlockLevel)
            return false;

        return base.IsCanShowTut();
    }

    public override bool IsCanEndTut()
    {
        return true;
    }

    public override void StartTut()
    {
        BirdSort2.GamePlayController_BirdSort.Instance.gameScene.layoutGroup.enabled = false;
        tutObj.gameObject.SetActive(true);
        parentTut.gameObject.SetActive(true);
        parentTut.transform.SetAsLastSibling();
        panelBlack.DOKill();
        panelBlack.color = new Color(0, 0, 0, 0);
        panelBlack.DOFade(215f / 255, 0.3f).OnComplete(()=> 
        {
           
        });
        //BirdSort2.GamePlayController_BirdSort.Instance.countDownController.PauseCoolDown();
        BirdSort2.GamePlayController_BirdSort.Instance.isTempPauseLevel = true;
        //panelBlack.color = new Color(0, 0, 0, 215f / 255);
       // BirdSort2.GamePlayController_BirdSort.Instance.gameScene.layoutGroup.enabled = false;
        var itemUI = BirdSort2.GamePlayController_BirdSort.Instance.gameScene.itemsUI[typeItem];
        itemUI.transform.SetAsLastSibling();

        
    }

    protected override void SetNameTut()
    {
        nameTut = TUT_ITEM_UI + typeItem;
    }

    public override void OnEndTut()
    {
        base.OnEndTut();
        
        panelBlack.DOKill();
        panelBlack.DOFade(0, 0.15f).OnComplete(() => { parentTut.gameObject.SetActive(false); });
     //   BirdSort2.GamePlayController_BirdSort.Instance.countDownController.PauseCoolDown();
        BirdSort2.GamePlayController_BirdSort.Instance.isTempPauseLevel = false;
    }
}

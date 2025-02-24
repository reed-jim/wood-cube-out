using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CoreGameData;
using DG.Tweening;

public class TutorialCampainBox : TutorialBase
{
    public const string TUT_ITEM_CAMPAIN = "tut_item_campain";

    [SerializeField] private GameObject parentTut;
    [SerializeField] private Image panelBlack;
    public GiftType typeItem;
    private BootsConfig bootConfig;
    [SerializeField] private GameObject tutObj;
    [SerializeField] private SellectItemUI itemUI;
    [SerializeField] private Button[] continueBtns;

    public override void Init(TutorialFunController controller)
    {
        base.Init(controller);
        parentTut.gameObject.SetActive(false);
        tutObj.gameObject.SetActive(false);

        bootConfig = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig;

        for (int i = 0; i < continueBtns.Length; i++)
        {
            int index = i;
            continueBtns[index].onClick.AddListener(OnClickContinue);
        }
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
        tutObj.gameObject.SetActive(true);
        parentTut.gameObject.SetActive(true);
        parentTut.transform.SetAsLastSibling();
        panelBlack.DOKill();
        panelBlack.color = new Color(0, 0, 0, 0);
        panelBlack.DOFade(215f / 255, 0.3f).OnComplete(() =>
        {

        });
        itemUI.transform.SetAsLastSibling();
    }

    protected override void SetNameTut()
    {
        nameTut = TUT_ITEM_CAMPAIN + typeItem;
    }

    public override void OnEndTut()
    {
        base.OnEndTut();

        panelBlack.DOKill();
        panelBlack.DOFade(0, 0.15f).OnComplete(() => { parentTut.gameObject.SetActive(false); });
    }

    private void OnClickContinue()
    {
        if (!IsCanShowTut())
            return;

        UseProfile.UseItem(typeItem, true);
        itemUI.UpdateUI();
        controller.NextTut();
    }    
}

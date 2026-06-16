using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestStarInfoBox : BaseBox
{
    private static ChestStarInfoBox instance;
    public static ChestStarInfoBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<ChestStarInfoBox>(PathPrefabs.CHEST_STAR_INFO_BOX));
            instance.Init();
        }
        instance.InitState();
        return instance;
    }

    [SerializeField] protected Image progessImg;
    [SerializeField] protected Text progessTxt;
    [SerializeField] protected Button continueBtn;
    public void Init()
    {
        continueBtn.onClick.AddListener(Close);
    }

    public void InitState()
    {
        progessTxt.text = StartChestController.CurrentStar + "/" + GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.starChestConfig.startRequirement;
        progessImg.fillAmount = (float)StartChestController.CurrentStar / GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.starChestConfig.startRequirement;
    }
}

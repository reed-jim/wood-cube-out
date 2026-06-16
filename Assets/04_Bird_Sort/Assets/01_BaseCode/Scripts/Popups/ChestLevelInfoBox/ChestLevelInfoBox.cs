using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestLevelInfoBox : BaseBox
{
    private static ChestLevelInfoBox instance;
    public static ChestLevelInfoBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<ChestLevelInfoBox>(PathPrefabs.CHEST_LEVEL_INFO_BOX));
            instance.Init();
        }
        instance.InitState();
        return instance;
    }

  
    [SerializeField] protected Text levelPassTxt;
    [SerializeField] protected Button continueBtn;

    public void Init()
    {
        continueBtn.onClick.AddListener(Close);
    }

    public void InitState()
    {
        int levelSpacing = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelChestConfig.levelSpacing;
        levelPassTxt.text = string.Format(Localization.Get("s_level_chest_detail"), levelSpacing * (((UseProfile.CurrentLevel) / levelSpacing) + 1));
    }
}

using EnhancedScrollerDemos.SnappingDemo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SuggetPopupHomeController;

public class Tool : MonoBehaviour
{
    [SerializeField] private TMP_InputField levelTxt;
    [SerializeField] private Button btnJump;
    [SerializeField] private Button btnChallenge;
    [SerializeField] private Button btnPopup;
    [SerializeField] private Toggle IsRemoveAds;
    [SerializeField] private List<PackBaseController> salePacks;
    [SerializeField] private List<BaseBox> boxBases;
    // Start is called before the first frame update
    void Start()
    {
#if !TESTER
        gameObject.SetActive(false);
#endif
        btnJump.onClick.AddListener(() => changeLevel());
        btnChallenge.onClick.AddListener(() => changeChallenge());
        btnPopup.onClick.AddListener(() => openPopup());
        IsRemoveAds.isOn = BirdSortGameController.Instance.useProfile.IsRemoveAds;
    }

    void changeLevel()
    {
        int level = int.Parse(levelTxt.text);

        if (level != 0)
        {
            UseProfile.CurrentLevel = level;
            BirdSortGameController.Instance.useProfile.CurrentLevelPlay = level;
        }
        else
        {
            return;
        }
        UseProfile.AddItem(GiftType.ReturnItem_BirdSort, 10000);
        UseProfile.AddItem(GiftType.AddBrandItem_BirdSort, 100);
        UseProfile.AddItem(GiftType.ShuffleItem_BirdSort, 100);
        UseProfile.AddItem(GiftType.BreakTheRule_BirdSort, 100);
        BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY);
    }

    void changeChallenge()
    {
        int level = int.Parse(levelTxt.text);

        if (level!=0)
        {
            BirdSortGameController.Instance.useProfile.CurrentHardLevel = level;
        }
        else
        {
            return;
        }
        UseProfile.AddItem(GiftType.ReturnItem_BirdSort,10000);
        UseProfile.AddItem(GiftType.AddBrandItem_BirdSort,100);
        UseProfile.AddItem(GiftType.ShuffleItem_BirdSort,100);
        UseProfile.AddItem(GiftType.BreakTheRule_BirdSort,100);
        BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY_CHALLENGE);
    }

    void openPopup()
    {
        LevelChestController.CurrentLevelChest += 10;
        StartChestController.CurrentStar += 20;

        SuggetPopupHomeController sugget = HomeController.Instance.suggetPopupHomeController;
        sugget.Init();
        sugget.RegisterSuggetPopupShow();
        for (int i = 0; i < salePacks.Count; i++)
        {
            var packShow = salePacks[i];
            sugget.AddActionShow(new SuggetPopupData()
            {
                actionShow = () =>
                {
                    packShow.ShowPopup(true);
                },
                priovity = PriovitySugget.Important
            }
            );
        }

        for (int i = 0; i < boxBases.Count; i++)
        {
            var boxShow = boxBases[i];
            sugget.AddActionShow(new SuggetPopupData()
            {
                actionShow = () => {
                    boxShow.CreateInstance();
                },
                priovity = PriovitySugget.HighQuality
            });
        }

        StartCoroutine(sugget.StartShowPopup());
    }

    public void setRemoveAds()
    {
        BirdSortGameController.Instance.useProfile.IsRemoveAds = IsRemoveAds.isOn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

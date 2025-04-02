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
    [SerializeField] private Button btnPopup;
    [SerializeField] private Toggle IsRemoveAds;
    [SerializeField] private List<PackBaseController> salePacks;
    [SerializeField] private List<BaseBox> boxBases;
    // Start is called before the first frame update
    void Start()
    {

        //gameObject.SetActive(false);
        btnJump.onClick.AddListener(() => changeLevel());
        //btnPopup.onClick.AddListener(() => openPopup());
        IsRemoveAds.isOn = GameController.Instance.useProfile.IsRemoveAds;
    }

    void changeLevel()
    {
        int level = int.Parse(levelTxt.text);

        if (level!=0)
        {
            UseProfile.CurrentLevel = level;
            GameController.Instance.useProfile.CurrentLevelPlay = level;
        }
        else
        {
            return;
        }
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

        // for (int i = 0; i < boxBases.Count; i++)
        // {
        //     var boxShow = boxBases[i];
        //     sugget.AddActionShow(new SuggetPopupData()
        //     {
        //         actionShow = () => {
        //             boxShow.CreateInstance();
        //         },
        //         priovity = PriovitySugget.HighQuality
        //     });
        // }

        StartCoroutine(sugget.StartShowPopup());
    }

    public void setRemoveAds()
    {
        GameController.Instance.useProfile.IsRemoveAds = IsRemoveAds.isOn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

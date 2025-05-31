using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LeaderBroadPage : PageBase
{
    public Button worldBtn;
    public Button localBtn;
    public GameObject scrollContent;
    public Text comingSoonText;
    public Image worldTabIcon;
    public Image localTabIcon;
    public Sprite activateTabIcon;
    public Sprite deactivateTabIcon;

    [SerializeField] private IQLeaderboard leaderBoard;
    [HideInInspector] public bool isGetLeaderBroadDone;
    public GameObject noInternetObj;

    public override void Init(PageController controller)
    {
        worldBtn.onClick.AddListener(() => ShowWorld());
        localBtn.onClick.AddListener(() => ShowLocal());

        //leaderBoard.Init(this);

        base.Init(controller);
        
    }

    public override void InitState()
    {
       
    }

    public override void OnChoicePage()
    {
        choicePageUI.OnChoicePage();
        comingSoonText.gameObject.SetActive(true);
       
        //ShowWorld();
    }

    #region Public Method

    public void ShowWorld()
    {
        //this.InitState();
        GameController.Instance.musicManager.PlayClickSound();
        comingSoonText.gameObject.SetActive(false);
        worldTabIcon.sprite = activateTabIcon;
        localTabIcon.sprite = deactivateTabIcon;
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            noInternetObj.gameObject.SetActive(true);
            scrollContent.SetActive(false);
        }
        else
        {
            noInternetObj.gameObject.SetActive(false);
            leaderBoard.InitState();
            scrollContent.SetActive(true);
        }
    }
    public void ShowLocal()
    {
        GameController.Instance.musicManager.PlayClickSound();
        comingSoonText.gameObject.SetActive(true);
        scrollContent.SetActive(false);
        localTabIcon.sprite = activateTabIcon;
        worldTabIcon.sprite = deactivateTabIcon;
    }
  
    #endregion
}

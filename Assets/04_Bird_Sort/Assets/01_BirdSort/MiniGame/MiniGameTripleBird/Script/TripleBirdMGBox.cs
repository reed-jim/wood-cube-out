using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class TripleBirdMGBox : BaseBox
{
    #region Instance
    private static TripleBirdMGBox instance;
    public static TripleBirdMGBox Setup(bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<TripleBirdMGBox>(PathPrefabs.MINI_GAME_TRIPLE_BIRD_BOX));
            instance.Init();
        }
        instance.InitState();
        return instance;
    }
    #endregion
    #region Variable
    public Button btnClose;
    public Button btnPlusEgg;
    public Button btnShopEgg;
    public TripleBirdMGLogic miniGameLogic;
    #endregion
    public void Init()
    {
        btnClose.onClick.AddListener(() => HandleClose());
        btnPlusEgg.onClick.AddListener(OpenPlushEgg);
        btnShopEgg.onClick.AddListener(OpenShopEgg);
        miniGameLogic.Init();
        Input.multiTouchEnabled = false;
        if (HomeController.Instance.miniGameController.tripleBirdMG.hand.gameObject.activeSelf == true)
        {
            HomeController.Instance.miniGameController.tripleBirdMG.hand.gameObject.SetActive(false);
        }
    }
    public void InitState()
    {
        miniGameLogic.InitState();
    }
    public void OpenPlushEgg()
    {
        TripleBirdMiniGameShopBox.Setup(miniGameLogic).Show();
        GameController.Instance.musicManager.PlayClickSound();
    }
    public void OpenShopEgg()
    {
        TripleBirdMiniGameShopBox.Setup(miniGameLogic).Show();
        GameController.Instance.musicManager.PlayClickSound();
    }
    public void HandleClose()
    {
        HomeController.Instance.miniGameController.tripleBirdMG.hand.gameObject.SetActive(false);
        Close();
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.admobAds.ShowInterstitial(actionWatchLog:"Video_CloseMinigame");
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        Input.multiTouchEnabled = true;
    }
}

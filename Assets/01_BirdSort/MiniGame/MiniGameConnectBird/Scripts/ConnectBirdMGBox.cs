using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class ConnectBirdMGBox : BaseBox
{
    #region Instance
    private static ConnectBirdMGBox instance;
    public static ConnectBirdMGBox Setup(bool isPostEventPopupClose = false, UnityAction actionOpenBoxSave = null, Action<BaseBox> OnLoadedFirst = null, Action<BaseBox> OnLoaded = null)
    {
        if (instance == null)
        {
            BoxController.Instance.GetBoxAsync(
                instance,
                (m_instance) =>
                {
                    instance = (ConnectBirdMGBox)m_instance;
                  
                    instance.Init();
                    instance.isPostEventPopupClose = isPostEventPopupClose;
                    instance.InitState();
                    if (OnLoadedFirst != null)
                        OnLoadedFirst(instance);
                    instance.Show();
                      
                },
                (instance) =>
                {
                    instance.Show();
                    if (OnLoaded != null)
                        OnLoaded(instance);
                },
                PathPrefabs.MINI_GAME_CONNECT_BIRD_BOX);
        }
        else
        {
            if (OnLoaded != null)
            {
                OnLoaded(instance);
                // instance.InitState();
            }
            instance.isPostEventPopupClose = isPostEventPopupClose;
            instance.InitState();
            instance.Show();
        }

        return instance;
    }
    #endregion

    #region Variable
    public Button btnClose;
    public Button btnClose1;
    public Button btnPlusEgg;
    public Button btnShopEgg;
    public ConnectBirdMGLogic miniGameLogic;

    public static bool IsShowedTutFirstTime
    {
        get { return PlayerPrefs.GetInt("IsShowedTutFirstTime_ConnectBirdMGBox", 0) == 1; }
        set
        {
            PlayerPrefs.SetInt("IsShowedTutFirstTime_ConnectBirdMGBox", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }    

    #endregion

    public override void Show()
    {
        base.Show();
        instance.InitState();
        isClosing = false;
        Input.multiTouchEnabled = false;
        if (HomeController.Instance.miniGameController.connectBirdMG.hand.gameObject.activeSelf == true)
        {
            HomeController.Instance.miniGameController.connectBirdMG.hand.gameObject.SetActive(false);
        }    
    }



    public void Init()
    {
        btnClose.onClick.AddListener(Close);
        btnClose1.onClick.AddListener(Close);
        btnPlusEgg.onClick.AddListener(OpenPlushEgg);
        btnShopEgg.onClick.AddListener(OpenShopEgg);
        miniGameLogic.Init(this);
        
    }
    public void InitState()
    {
        
        miniGameLogic.firtOpen = false;
        miniGameLogic.InitState();
        GameTimer.Instance.StartTimer();
    }

    public void OpenPlushEgg()
    {
        ConnectBirdMiniGameShopBox.Setup(miniGameLogic).Show();
        GameController.Instance.musicManager.PlayClickSound();
    }

    public void OpenShopEgg()
    {
        ConnectBirdMiniGameShopBox.Setup(miniGameLogic).Show();
        GameController.Instance.musicManager.PlayClickSound();
    }

    public void HandleClose()
    {
        // miniGameLogic.board.ResetBoard();
        //miniGameLogic.board.SpawnBackGroundFirst();
        miniGameLogic.coolSteptTut = 0;
        miniGameLogic.tut.OffTut();
        HomeController.Instance.miniGameController.connectBirdMG.hand.gameObject.SetActive(false);
        Close();
        GameController.Instance.musicManager.PlayClickSound();
        //GameController.Instance.admobAds.ShowInterstitial(actionWatchLog: "Video_CloseMinigame");
        
      
    }

    private bool isClosing;
    public override void Close()
    {
        if (isClosing)
        {
            return;
        }

        
        if (miniGameLogic.isPlaying)
        {
            return;
        }
        miniGameLogic.coolSteptTut = 0;
        miniGameLogic.tut.OffTut();
        HomeController.Instance.miniGameController.connectBirdMG.hand.gameObject.SetActive(false);
        base.Close();
        UseProfile.LastTimeOpenMinigame = DateTime.Now;
        isClosing = true;
        GameController.Instance.musicManager.PlayClickSound();
        //GameController.Instance.admobAds.ShowInterstitial(actionWatchLog: "Video_CloseMinigame");
        
        /*try
        {
            if (GameTimer.Instance.SecondsTime >= 5 && GameTimer.Instance.SecondsTime <= 172800)
            {
                DWHLog.Log.SessionLog(UseProfile.CurrentLevel, GameTimer.Instance.SecondsTime, "minigame_connect_bird");
                GameTimer.Instance.StartTimer();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }*/
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        miniGameLogic.board.ResetBoard();
        Input.multiTouchEnabled = true;
    }





}

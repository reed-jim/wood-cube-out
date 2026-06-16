using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using BirdSort2;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;

public class HomeScene : BaseScene
{
    public LableInfo lableInfo;
    public HealthTopUI healthTopUI;
    public CoinTopUI coinTopUI;
    [SerializeField] private Button settingBtn;
    public ItemViewHome itemViewHome;
    public Button btnConnectMiniGame;

    private Action<object> actionOpenShop;
    public void Init()
    {
        itemViewHome.Init();

        actionOpenShop = (sender) => { OpenShop(); };
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.OPEN_SHOP, actionOpenShop);

        lableInfo.Init();
        healthTopUI.Init();
        coinTopUI.Init();

        settingBtn.onClick.AddListener(OnClickSetting);
    }


    public void OpenShop()
    {
        HomeController.Instance.pageController.ChangePage((int)PageName.Shop);
    }

    public void OnClickSetting()
    {
        SettingBox.Setup().Show();
        BirdSortGameController.Instance.musicManager.PlayClickSound();
    }

    /// <summary>
    /// Hàm g?i khi stack Box = 0
    /// </summary>
    public override void OnEscapeWhenStackBoxEmpty()
    {
        //Hi?n th? popup b?n có mu?n thoát game ko?
    }

    private void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.OPEN_SHOP, actionOpenShop);
    }
    public void OnClickBtnConnectBirdMNGame()
    {
        BirdSortGameController.Instance.musicManager.PlayClickSound();
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            BirdSortGameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
            (
                btnConnectMiniGame.transform.position,
                Localization.Get(LocalizationKey.PLEASE_CONNECT_INTERNET),
                Color.white,
                isSpawnItemPlayer: true
            );
        }
        else
        {
            try
            {
                if (GameTimer.Instance.SecondsTime >= 5 && GameTimer.Instance.SecondsTime <= 172800)
                {
                    new FSessionLog("minigame_connect_bird", TimeSpan.FromSeconds(GameTimer.Instance.SecondsTime), UseProfile.CurrentLevel).Send();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            HomeController.Instance.miniGameController.OpenMiniGameBox();
        }
    }
}

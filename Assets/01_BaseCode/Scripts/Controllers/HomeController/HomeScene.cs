using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using BirdSort2;

public class HomeScene : BaseScene
{
    public LableInfo lableInfo;
    public HealthTopUI healthTopUI;
    public CoinTopUI coinTopUI;
    [SerializeField] private Button settingBtn;
    public ItemViewHome itemViewHome;
    public Button btnConnectMiniGame;

    private Action<object> actionOpenShop;
    public  void Init()
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
        GameController.Instance.musicManager.PlayClickSound();
    }    

    /// <summary>
    /// Hàm gọi khi stack Box = 0
    /// </summary>
    public override void OnEscapeWhenStackBoxEmpty()
    {
        //Hiển thị popup bạn có muốn thoát game ko?
    }

    private void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.OPEN_SHOP, actionOpenShop);
    }
    public void OnClickBtnConnectBirdMNGame()
    {
        GameController.Instance.musicManager.PlayClickSound();
       /* if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
            (
                btnConnectMiniGame.transform.position,
                Localization.Get(LocalizationKey.PLEASE_CONNECT_INTERNET),
                Color.white,
                isSpawnItemPlayer: true
            );
        }
        else
        {
            /*try
            {
                if (GameTimer.Instance.SecondsTime >= 5 && GameTimer.Instance.SecondsTime <= 172800)
                {
                    DWHLog.Log.SessionLog(UseProfile.CurrentLevel, GameTimer.Instance.SecondsTime, "gameplay");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }*/
        HomeController.Instance.miniGameController.OpenMiniGameBox();
    }
}

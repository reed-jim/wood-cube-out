using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class SuggetLoginPvPBox : BaseBox
{
    public void Init()
    {
        btnClose.onClick.AddListener(CloseButtonHandle);
        btnLoginFB.onClick.AddListener(OnClickLoginFb);
        //btnLoginGG.onClick.AddListener(OnClickLoginGG);
        btnConfirm.onClick.AddListener(OnClickConfirm);
        if (UseProfile.NamePlayer.Contains("Bird#"))
        {
            ipfName.gameObject.SetActive(true);
            btnConfirm.gameObject.SetActive(true);
        }
        else
        {
            ipfName.gameObject.SetActive(false);
            btnConfirm.gameObject.SetActive(false);
        }
    }

    private void OnClickConfirm()
    {
        GameController.Instance.musicManager.PlayClickSound();
        HandleButtonConfirm();
    }

    private void OnClickLoginGG()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.loginController.Login(TypeLogin.GG);
    }

    private void OnClickLoginFb()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.loginController.Login(TypeLogin.FB);
    }


    public void InitState(Action close)
    {
        EvtClose = null;
        EvtClose = close;

        actionCloseEvent = ClosePopUp;
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.ON_LOGIN_GG_OR_FB_COMPLETE, actionCloseEvent);
    }

    public void CloseButtonHandle()
    {
        GameController.Instance.musicManager.PlayClickSound();
        ClosePopUp(null);
    }
    private void ClosePopUp(object param)
    {
        EvtClose?.Invoke();
        Close();
      //  GamePlayController.Instance.gameScene.lableInfo.InitState();
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.ON_LOGIN_GG_OR_FB_COMPLETE, actionCloseEvent);
 
    }
    public void InputName(string param)
    {
        inputString = param;
    }

    private void HandleButtonConfirm()
    {
        if (string.IsNullOrEmpty(inputString))
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
            (
                transform.position,
                "Pls enter your name!",
                Color.white,
                true
            );
        }
        else
        {
            var box = WaitingBox.Setup();
            box.ShowWaiting();
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                btnConfirm.interactable = false;
                PlayFabClientAPI.UpdateUserTitleDisplayName(
                    new UpdateUserTitleDisplayNameRequest { DisplayName = inputString }, s =>
                    {
                        UseProfile.NamePlayer = inputString;
                        // GameController.Instance.playerData.SaveData();
                        ipfName.gameObject.SetActive(false);
                        btnConfirm.gameObject.SetActive(false);
                        box.HideWaiting();
                        ClosePopUp(null);
                        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.UPDATE_NAME_AVATAR);
                    }, e =>
                    {
                        Debug.LogError(e);
                        box.HideWaiting();
                        btnConfirm.interactable = true;
                        GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp(
                            transform.position,"Name already taken!",Color.red,true);
                    });
                PlayFabClientAPI.UpdateAvatarUrl(new UpdateAvatarUrlRequest { ImageUrl = UseProfile.AvatarLink }, s => { },
                    e => { });
            }
            else
            {
                GameController.Instance.loginController.ReLoginPlayFab(HandleButtonConfirm,(() =>
                {
                    box.HideWaiting();
                    GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                        transform.position,
                         Localization.Get("s_no_internet"),
                        Color.white,
                        true
                    );
                }));
            }
            
        }
    }

    private string GetLastFiveCharOfId(string param)
    {
        if (string.IsNullOrEmpty(param))
        {
            return "00000";
        }
        var a = "";
        for (var i = 1; i < 6; i++) a += param[param.Length - i];


        return a;
    }

    #region instance

    private static SuggetLoginPvPBox instance;

    public static SuggetLoginPvPBox Setup(bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SuggetLoginPvPBox>(PathPrefabs.SUGGET_LOGIN_BOX));
            instance.Init();
        }

        instance.InitState(actionOpenBoxSave);


        return instance;
    }

    #endregion

    #region Variable

    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnLoginFB;
    [SerializeField] private Button btnLoginGG;
    [SerializeField] private GameObject ipfName;
    [SerializeField] private Button btnConfirm;

    private Action EvtClose;
    private string inputString;

    private Action<object> actionCloseEvent;

    #endregion
}

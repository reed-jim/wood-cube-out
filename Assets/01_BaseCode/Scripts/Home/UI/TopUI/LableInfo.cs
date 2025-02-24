using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Facebook.Unity;
using System;
using UniRx;

public class LableInfo : MonoBehaviour
{
    protected IDisposable _downloadAvatarDispose;

    #region Variable
    [SerializeField] private RawImage thumnail;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Image frames;
    [SerializeField] private Button btnShowPopupSuggetLogin;
    [SerializeField] private Text tvName;
    [SerializeField] private bool wasNativeSize;
    private string inputString;
    Texture2D test;
    private Action<object> actionSaveDataSuccess;
    private bool isInited;
    #endregion

    [SerializeField] private GameObject notiObj;


    #region PublicMethod
    public void Init()
    {
        if (!isInited)
        {
            btnConfirm.onClick.RemoveAllListeners();
            btnConfirm.onClick.AddListener(() => HandleButtonConfirm());
            btnShowPopupSuggetLogin.onClick.RemoveAllListeners();
            btnShowPopupSuggetLogin.onClick.AddListener(() => HandleShowPopupSuggetLogin());
            isInited = true;
        }
        InitState();
    }

    public void InitState()
    {
        SetUp();
        actionSaveDataSuccess = (parm) => { SetUp(); };
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.UPDATE_NAME_AVATAR, actionSaveDataSuccess);
    }

    public void InputName(string param)
    {
        inputString = param;
    }
    #endregion
    #region Private Method
    private void SetUp()
    {
        if (UseProfile.IsLoggedInByFBorGG)
        {
            btnShowPopupSuggetLogin.gameObject.SetActive(false);
            HandleLocalAvatar();
            if (!string.IsNullOrEmpty(UseProfile.AvatarLink))
            {
                _downloadAvatarDispose = Context.DownloadOrCache(UseProfile.AvatarLink, true)
                                  .CatchIgnore()
                                  .Subscribe(www =>
                                  {
                                      thumnail.texture = www;
                                    //Debug.Log("CCCCCCCCCCCCCCCCCCCCCCC");
                                  });
            }
            tvName.text = UseProfile.NamePlayer;
            notiObj.gameObject.SetActive(false);
        }
        else
        {
            HandleLocalAvatar();
            try
            {
                if (UseProfile.NamePlayer == "Bird#" + GetLastFiveCharOfId(UseProfile.PlayFabId))
                {
                    tvName.gameObject.SetActive(true);
                    tvName.text = UseProfile.NamePlayer;
                }
                else
                {
                    tvName.gameObject.SetActive(true);
                    tvName.text = UseProfile.NamePlayer;
                }
            }
            catch
            {
                tvName.gameObject.SetActive(true);
                if (UseProfile.NamePlayer == "")
                {
                    tvName.text = "Bird#" + GetLastFiveCharOfId(GUtils.GetDeviceId());
                    Debug.LogError("GetDeviceId " + GetLastFiveCharOfId(GUtils.GetDeviceId()));
                }
                else
                {
                    tvName.text = UseProfile.NamePlayer;
                }

            }

            if (UseProfile.NamePlayer.Contains("Bird#"))
            {
                if (notiObj != null)
                    notiObj.gameObject.SetActive(true);
            }
            else
            {
                if (notiObj != null)
                    notiObj.gameObject.SetActive(false);
                
                //Temp no login FB
                btnShowPopupSuggetLogin.gameObject.SetActive(false);
            }
        }
    }

    private void HandleLocalAvatar()
    {
        var sprite = (Texture)Resources.Load("UI/Sprite/Avatars/Avatar0", typeof(Texture));
        thumnail.texture = sprite;
        //frames.sprite = GameController.Instance.rankController.GetRankData(UseProfile.ScorePvP).frames;
        //if (wasNativeSize)
        //    return;
        //frames.SetNativeSize();

        //Debug.Log("UUUUUUUUUUUUUUUUUUU");
        //  Debug.LogError("UseProfile.ScorePvP " + UseProfile.ScorePvP);

    }
    private void HandleBtnEdit()
    {
        btnConfirm.gameObject.SetActive(true);
        tvName.gameObject.SetActive(false);

    }
    private void HandleButtonSetAvatar()
    {

        //PopupSelectAvatar.Setup().Show();

    }
    private void HandleButtonConfirm()
    {
        if (inputString == null || inputString == "")
        {
            btnConfirm.gameObject.SetActive(false);
            tvName.gameObject.SetActive(true);
            tvName.text = UseProfile.NamePlayer;
            Debug.LogError("HandleNull");
        }
        else
        {
            tvName.gameObject.SetActive(true);
            UseProfile.NamePlayer = inputString;
            tvName.text = UseProfile.NamePlayer;
            btnConfirm.gameObject.SetActive(false);
            Debug.LogError("HandleButtonConfirm");
        }
        GameController.Instance.musicManager.PlayClickSound();
    }
    private void HandleShowPopupSuggetLogin()
    {
        SuggetLoginPvPBox.Setup(false, () => SetUp()).Show();
        GameController.Instance.musicManager.PlayClickSound();
    }
    private string GetLastFiveCharOfId(string param)
    {
        //if (param.Length <=0)
        //{
        //    return "00000";
        //}
        string a = "";

        for (int i = 1; i < 6; i++)
        {
            a += param[param.Length - i];
        }


        return a;



    }
    private void OnDisable()
    {
        try
        {
            _downloadAvatarDispose.Dispose();
        }
        catch
        {

        }
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.UPDATE_NAME_AVATAR, actionSaveDataSuccess);
    }

    #endregion

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SetUp();
        }    
    }

}


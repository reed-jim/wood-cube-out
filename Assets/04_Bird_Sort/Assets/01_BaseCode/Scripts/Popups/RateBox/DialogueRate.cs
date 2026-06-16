using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;

public class DialogueRate : BaseBox
{

    private static DialogueRate instance;
    public static DialogueRate Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<DialogueRate>(PathPrefabs.RATE_GAME_BOX));
            instance.Init();
        }
        //ChickenDataManager.CountTillShowRate = 0;
        instance.gameObject.SetActive(true);
        instance.isPostEventPopupClose = isPostEventPopupClose;
        instance.InitState();
        return instance;
    }
    private const int MIN_API_LEVEL_REVIEW = 21;
    [SerializeField] private ReviewInAppController reviewInAppController;


    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private List<Sprite> lstSprStar;
    [SerializeField] private List<Button> lstBtnStar;
    [SerializeField] private List<Image> lstImgStar;
    private int countStar;

    [SerializeField] private DOTweenAnimation animTut;

    public void Init()
    {
        btnConfirm.onClick.AddListener(RateAction);
        btnClose.onClick.AddListener(CloseAction);
    }
    public void InitState()
    {
        for (int i = 0; i < lstImgStar.Count; i++)
        {
            int index = i + 1;
            // lstBtnStar[i].onClick.AddListener(() => { ClickStar(index); });
            lstImgStar[i].sprite = lstSprStar[0];
        }
        countStar = 0;
        animTut.gameObject.SetActive(true);
        animTut.DOPlay();
    }

    public void OnAnimTutComplete()
    {
        animTut.DORewind();
        animTut.gameObject.SetActive(false);
    }
    public void ClickStar(int index)
    {
        countStar = index;
        for (int i = 0; i < lstImgStar.Count; i++)
        {
            lstImgStar[i].sprite = lstSprStar[0];
        }
        for (int i = 0; i < index; i++)
        {
            lstImgStar[i].sprite = lstSprStar[1];
        }
        //GameController.Instance.musicManager.Pla();
    }
    public void RateAction()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (countStar <= 0)
            return;
        if (countStar == 5)
        {
            UseProfile.CanShowRate = false;

            try
            {
#if UNITY_ANDROID
                if (RemoteConfigController.GetBoolConfig("on_review_inapp_rate", false))
                {
                    if (Context.GetSDKLevel() >= MIN_API_LEVEL_REVIEW)
                    {
                        reviewInAppController.ShowReview(() =>
                        {
                            Application.OpenURL(Config.OPEN_LINK_RATE);

                        });
                    }
                    else
                    {
                        Application.OpenURL(Config.OPEN_LINK_RATE);
                    }
                }
                else
                {
                    Application.OpenURL(Config.OPEN_LINK_RATE);
                }
#else
            Application.OpenURL(Config.OPEN_LINK_RATE);
           // OnCloseWithNeverShow();
#endif
            }
            catch
            {

            }

            CloseAction();
        }
        else
        {
            ShowTextThankRate();
            CloseAction();
        }
    }
    public void CloseAction()
    {
        GameController.Instance.musicManager.PlayClickSound();
        Close();
    }

    public void ShowTextThankRate()
    {
        //StartCoroutine(Helper.StartAction(() =>
        //{
        GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
    (
       btnConfirm.transform.position,
        "Thank you for the review!",
        Color.white,
        isSpawnItemPlayer: true
    );
        // }, 0.5f));
    }
}
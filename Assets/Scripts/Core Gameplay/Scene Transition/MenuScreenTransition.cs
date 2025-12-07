using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreenTransition : MonoBehaviour
{
    [SerializeField] private Image blackOverlay;
    [SerializeField] private Image mainBackground;

    [Header("LOADING LEVEL IN PROGRESS")]
    [SerializeField] private RectTransform fakeLoadingContainer;
    [SerializeField] private CanvasGroup fakeLoadingCanvasGroup;
    [SerializeField] private RectTransform rotatingLoadingIcon;
    [SerializeField] private Image blackBackgroundToPassPolicy;
    private Coroutine _fakeLoadingCoroutine;

    [SerializeField] private IntVariable currentLevel;

    private List<Tween> _tweens;

    void Awake()
    {
        _tweens = new List<Tween>();

        blackBackgroundToPassPolicy.gameObject.SetActive(false);
    }

    void Start()
    {
        mainBackground.sprite = DataManager.Ins.PictureDatas.allPictureDatas[DataManager.Ins.PlayerData.CurrentBackgroundId].mainImage;
        mainBackground.FitImageToRectTransform();

        if (GamePersistentVariable.hasAppJustLaunched)
        {
            fakeLoadingContainer.gameObject.SetActive(false);

            GamePersistentVariable.hasAppJustLaunched = false;
        }
        else
        {
            FakeLoadingForAds();
        }
    }

    private void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    private void BlackScreenTransition()
    {
        blackOverlay.color = ColorUtil.WithAlpha(blackOverlay.color, 1);

        blackOverlay.gameObject.SetActive(true);

        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Alpha(blackOverlay, 1, 0, startDelay: 0.5f, duration: 0.3f).OnComplete(() =>
        {
            blackOverlay.gameObject.SetActive(false);
        }));
    }

    #region FAKE LOADING FOR ADS
    private IEnumerator WaitForShowAdAfterLoading()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        blackBackgroundToPassPolicy.gameObject.SetActive(true);

        while (!GamePersistentVariable.isLoadingComplete)
        {
            yield return waitForSeconds;
        }

        AdmobAdsMax.Instance.ShowInterstitialAdmob(
            onAdClosed: () =>
            {
                blackBackgroundToPassPolicy.gameObject.SetActive(false);
            },
            isShowAdBreak: false
        );
    }

    private void FakeLoadingForAds()
    {
        GamePersistentVariable.isMenuLoading = true;

        fakeLoadingContainer.gameObject.SetActive(true);

        fakeLoadingCanvasGroup.alpha = 1;

        if (_fakeLoadingCoroutine != null)
        {
            StopCoroutine(_fakeLoadingCoroutine);
        }

        AdmobAdsMax.Instance.ShowMRec();

        float minWaitTime = 2;

        if (GamePersistentVariable.doubleInterAdsConfig.isEnableMrec)
        {
            minWaitTime = 4.5f;
        }

        _tweens.Add(Tween.Delay(minWaitTime).OnComplete(() =>
        {
            AdmobAdsMax.Instance.HideMRec();

            blackBackgroundToPassPolicy.gameObject.SetActive(true);

            AdmobAdsMax.Instance.ShowInterstitialAdmob(
                onAdClosed: () =>
                {
                    blackBackgroundToPassPolicy.gameObject.SetActive(false);

                    SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "Switch to Menu");

                    GamePersistentVariable.isMenuLoading = false;
                },
                isShowAdBreak: true
            );

            _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
            {
                fakeLoadingCanvasGroup.alpha = newVal;
            })
            .OnComplete(() =>
                {
                    fakeLoadingContainer.gameObject.SetActive(false);
                })
            );
        }));
    }

    private IEnumerator RotatingIcon()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        while (true)
        {
            rotatingLoadingIcon.eulerAngles -= new Vector3(0, 0, 8);

            yield return waitForSeconds;
        }
    }
    #endregion
}

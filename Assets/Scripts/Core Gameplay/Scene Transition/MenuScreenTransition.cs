using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreenTransition : MonoBehaviour
{
    [SerializeField] private Image blackOverlay;

    [Header("LOADING LEVEL IN PROGRESS")]
    [SerializeField] private RectTransform fakeLoadingContainer;
    [SerializeField] private CanvasGroup fakeLoadingCanvasGroup;
    [SerializeField] private RectTransform rotatingLoadingIcon;
    [SerializeField] private Image blackBackgroundToPassPolicy;
    private Coroutine _fakeLoadingCoroutine;

    private List<Tween> _tweens;

    void Awake()
    {
        _tweens = new List<Tween>();

        DoubleInterAdsConfig doubleInterAdsConfig = new DoubleInterAdsConfig();

        doubleInterAdsConfig.isEnable = true;
        doubleInterAdsConfig.isEnableDoubleInter = true;
        doubleInterAdsConfig.cooldown = 15;

        GamePersistentVariable.doubleInterAdsConfig = doubleInterAdsConfig;

        if (GamePersistentVariable.doubleInterAdsConfig.isEnable)
        {
            if (GamePersistentVariable.hasAppJustLaunched)
            {
                fakeLoadingContainer.gameObject.SetActive(false);

                StartCoroutine(WaitForShowAdAfterLoading());

                GamePersistentVariable.hasAppJustLaunched = false;
            }
            else
            {
                FakeLoadingForAds();
            }
        }
        else
        {
            BlackScreenTransition();
        }

        blackBackgroundToPassPolicy.gameObject.SetActive(false);
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

        while (!GamePersistentVariable.isLoadingComplete)
        {
            yield return waitForSeconds;
        }

        AdmobAdsMax.Instance.ShowInterstitialAdmob(isShowAdBreak: false);
    }

    private void FakeLoadingForAds()
    {
        fakeLoadingContainer.gameObject.SetActive(true);

        fakeLoadingCanvasGroup.alpha = 1;

        if (_fakeLoadingCoroutine != null)
        {
            StopCoroutine(_fakeLoadingCoroutine);
        }

        _fakeLoadingCoroutine = StartCoroutine(RotatingIcon());

        AdmobAdsMax.Instance.ShowMRec();

        float minWaitTime = 2;

        if (GamePersistentVariable.remoteConfigIsShowMREC)
        {
            minWaitTime = 3;
        }

        _tweens.Add(Tween.Delay(minWaitTime).OnComplete(() =>
        {
            AdmobAdsMax.Instance.HideMRec();

            blackBackgroundToPassPolicy.gameObject.SetActive(true);

            AdmobAdsMax.Instance.ShowInterstitialAdmob(
                onAdClosed: () =>
                {
                    blackBackgroundToPassPolicy.gameObject.SetActive(false);
                },
                isShowAdBreak: false
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

            StopCoroutine(_fakeLoadingCoroutine);
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

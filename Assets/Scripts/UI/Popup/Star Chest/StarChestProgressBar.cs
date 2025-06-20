using System;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StarChestProgressBar : SaferioTweenBehaviour
{
    [SerializeField] private Image fill;

    [SerializeField] private RectTransform starGroupRT;
    [SerializeField] private CanvasGroup starGroup;
    [SerializeField] private TMP_Text starText;
    [SerializeField] private Transform claimNotification;
    [SerializeField] private GameObject buttonNameContainer;

    [SerializeField] private ParticleSystem fx;

    private Vector3 _starGroupInitialPosition;

    public static event Action openStarChestPopupEvent;

    protected override void MoreActionInAwake()
    {
        StarChestPopup.claimStarChestEvent += Refresh;
        SceneManager.sceneLoaded += OnSceneLoaded;

        _starGroupInitialPosition = starGroupRT.localPosition;

        Refresh();
    }

    protected override void MoreActionOnDestroy()
    {
        StarChestPopup.claimStarChestEvent -= Refresh;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        fill.fillAmount = 0;

        starGroupRT.gameObject.SetActive(false);

        StartCoroutine(WaitForMenuLoaded());
    }

    private void Refresh()
    {
        // bool isRewardAvailable = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData()).starCollected >= GameConstants.REQUIRE_STAR;

        // claimNotification.gameObject.SetActive(false);

        // if (isRewardAvailable)
        // {
        //     claimNotification.gameObject.SetActive(true);

        //     StartCoroutine(AnimateClaimNotification());
        // }
        // else
        // {
        //     claimNotification.gameObject.SetActive(false);
        // }

        // buttonNameContainer.SetActive(!isRewardAvailable);
    }

    private IEnumerator AnimateClaimNotification()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(0.3f);

        int step = 0;
        bool isWaiting = false;

        while (true)
        {
            if (!isWaiting)
            {
                if (step == 0)
                {
                    AddTween(Tween.Scale(claimNotification, 1.03f, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        step = 1;
                        isWaiting = false;
                    }));
                }
                else
                {
                    AddTween(Tween.Scale(claimNotification, 0.97f, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        step = 0;
                        isWaiting = false;
                    }));
                }

                isWaiting = true;
            }

            yield return waitOneSecond;
        }
    }

    private void SetProgress()
    {
        StarLevelData data = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        fill.fillAmount = 0;

        AddTween(Tween.Custom(0, data.starCollected % GameConstants.REQUIRE_STAR / (float)GameConstants.REQUIRE_STAR, duration: 0.3f, onValueChange: newVal =>
        {
            fill.fillAmount = newVal;
        })
            .OnComplete(() =>
            {
                bool isRewardAvailable = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData()).starCollected >= GameConstants.REQUIRE_STAR;

                if (isRewardAvailable)
                {
                    if (GamePersistentVariable.isLevelChestClaimable)
                    {
                        GamePersistentVariable.isLevelChestClaimable = false;
                    }
                    else
                    {
                        AdmobAdsMax.Instance.ResetAdsCooldown();

                        openStarChestPopupEvent?.Invoke();
                    }

                    GamePersistentVariable.isStarChestClaimable = true;
                }
            })
        );
    }

    private IEnumerator WaitForMenuLoaded()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        GamePersistentVariable.isStarChestClaimable = false;

        while (GamePersistentVariable.isMenuLoading)
        {
            yield return waitForSeconds;
        }

        yield return new WaitForSeconds(0.8f);

        PlayFx();
    }

    private void PlayFx()
    {
        StarLevelData data = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        if (data.newGainedItem == 0)
        {
            starGroupRT.gameObject.SetActive(false);

            SetProgress();

            return;
        }
        else
        {
            starText.text = $"+{data.newGainedItem}";

            data.newGainedItem = 0;

            DataUtility.Save(GameConstants.STAR_LEVEL_DATA, data);
        }

        starGroup.gameObject.SetActive(true);

        starGroup.alpha = 0;

        fill.fillAmount = 0;
        // fill.fillAmount = cachedPrevStarCollected % GameConstants.REQUIRE_STAR / (float)GameConstants.REQUIRE_STAR;

        AddSequence(
            Tween.Alpha(starGroup, 1, duration: 0.3f)
            .Chain(
                Tween.LocalPositionX(starGroupRT, _starGroupInitialPosition.x - 100, duration: 0.15f)
            )
            .Chain(
                Tween.LocalPositionX(starGroupRT, fill.rectTransform.localPosition.x, duration: 0.3f)
            )
            .OnComplete(() =>
            {
                fx.Play();
                SoundManager.Instance.PlaySoundUnlockScrewBox();

                AddTween(Tween.Alpha(starGroup, 0, duration: 0.3f));

                SetProgress();
            })
        );
    }
}

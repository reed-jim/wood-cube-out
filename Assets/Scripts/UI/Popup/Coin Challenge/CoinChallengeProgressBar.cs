using System;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CoinChallengeProgressBar : SaferioTweenBehaviour
{
    [SerializeField] private Image fill;

    [SerializeField] private RectTransform starGroupRT;
    [SerializeField] private CanvasGroup starGroup;
    [SerializeField] private TMP_Text starText;
    [SerializeField] private Transform claimNotification;
    [SerializeField] private GameObject buttonNameContainer;

    [SerializeField] private ParticleSystem fx;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private CoinChallengeConfigContainer coinChallengeConfigContainer;

    private Vector3 _starGroupInitialPosition;

    public static event Action openCoinChallengePopupEvent;

    protected override void MoreActionInAwake()
    {
        CoinChallengePopup.claimCoinChallengeEvent += Refresh;
        SceneManager.sceneLoaded += OnSceneLoaded;

        _starGroupInitialPosition = starGroupRT.localPosition;

        Refresh();
    }

    protected override void MoreActionOnDestroy()
    {
        CoinChallengePopup.claimCoinChallengeEvent -= Refresh;
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

    private bool IsClaimable()
    {
        CoinChallengeData data = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

        int currentMilestone = data.CurrentMilestone;

        bool isClaimable = false;

        for (int i = 0; i < data.IsMilestonesClaimed.Length; i++)
        {
            if (i >= currentMilestone)
            {
                continue;
            }
            else
            {
                if (!data.IsMilestonesClaimed[i])
                {
                    data.IsMilestonesClaimed[i] = true;

                    isClaimable = true;
                }
            }
        }

        return isClaimable;
    }

    private void Refresh()
    {
        claimNotification.gameObject.SetActive(false);

        // bool isClaimable = IsClaimable();

        // if (IsClaimable())
        // {
        //     claimNotification.gameObject.SetActive(true);

        //     StartCoroutine(AnimateClaimNotification());
        // }
        // else
        // {
        //     claimNotification.gameObject.SetActive(false);
        // }

        // buttonNameContainer.SetActive(!isClaimable);
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
        CoinChallengeData data = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

        int currentMilestone = data.CurrentMilestone;
        int requiredItem = coinChallengeConfigContainer.RequiredItems[currentMilestone];
        float progress = data.EventItemCollectedOfMilstones[currentMilestone] / (float)requiredItem;

        fill.fillAmount = 0;

        AddTween(Tween.Custom(0, progress, duration: 0.3f, onValueChange: newVal =>
        {
            fill.fillAmount = newVal;
        })
            .OnComplete(() =>
            {
                bool isClaimable = false;

                for (int i = 0; i < data.IsMilestonesClaimed.Length; i++)
                {
                    if (i > currentMilestone)
                    {
                        continue;
                    }
                    else
                    {
                        bool isValid = false;

                        if (i == currentMilestone)
                        {
                            if (data.EventItemCollectedOfMilstones[i] >= coinChallengeConfigContainer.RequiredItems[i] &&
                                !data.IsMilestonesClaimed[i]
                            )
                            {
                                isValid = true;
                            }
                        }
                        else
                        {
                            if (!data.IsMilestonesClaimed[i])
                            {
                                isValid = true;
                            }
                        }

                        if (isValid)
                        {
                            isClaimable = true;

                            break;
                        }
                    }
                }

                if (GamePersistentVariable.currentLevel < 30)
                {
                    isClaimable = false;
                }

                if (isClaimable)
                {
                    if (!GamePersistentVariable.isStarChestClaimable && !GamePersistentVariable.isLevelChestClaimable)
                    {
                        AdmobAdsMax.Instance.ResetAdsCooldown();

                        openCoinChallengePopupEvent?.Invoke();
                    }
                }
            })
        );
    }

    private IEnumerator WaitForMenuLoaded()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        while (GamePersistentVariable.isMenuLoading)
        {
            yield return waitForSeconds;
        }

        yield return new WaitForSeconds(0.8f);

        PlayFx();
    }

    private void PlayFx()
    {
        CoinChallengeData data = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

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

            DataUtility.Save(GameConstants.COIN_CHALLENGE_DATA, data);
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

                AddTween(Tween.Alpha(starGroup, 0, duration: 0.3f));

                SetProgress();
            })
        );
    }
}

using System;
using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelChestProgressBar : SaferioTweenBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private Transform claimNotification;
    [SerializeField] private GameObject buttonNameContainer;

    [SerializeField] private IntVariable currentLevel;

    public static event Action openLevelChestPopupEvent;
    public static event Action openChestEvent;

    protected override void MoreActionInAwake()
    {
        ChestLevelPopup.claimChestLevelEvent += SetProgress;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void MoreActionOnDestroy()
    {
        ChestLevelPopup.claimChestLevelEvent -= SetProgress;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        SetProgress();
    }

    private void Refresh()
    {
        // bool isRewardAvailable = currentLevel.Value >=
        //     DataUtility.Load(GameConstants.CHEST_LEVEL, new ChestLevelData()).currentRequiredLevel;

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
        Refresh();

        fill.fillAmount = 0;

        float finalAmount = currentLevel.Value % 10 / 10f;

        bool isRewardAvailable = currentLevel.Value >= DataUtility.Load(GameConstants.CHEST_LEVEL, new ChestLevelData()).currentRequiredLevel;

        if (isRewardAvailable)
        {
            // GamePersistentVariable.isLevelChestClaimable = true;
        }

        AddTween(Tween.Custom(0, finalAmount, duration: 0.3f, onValueChange: newVal =>
        {
            fill.fillAmount = newVal;
        })
            .OnComplete(() =>
            {
                if (isRewardAvailable)
                {
                    StartCoroutine(WaitForMenuLoaded());

                    // openLevelChestPopupEvent?.Invoke();
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

        // AdmobAdsMax.Instance.ResetAdsCooldown();

        // openLevelChestPopupEvent?.Invoke();

        GamePersistentVariable.chestQueue.Enqueue("Level Chest");

        openChestEvent?.Invoke();
    }
}

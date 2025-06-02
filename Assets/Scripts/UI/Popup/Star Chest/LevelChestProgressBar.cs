using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class LevelChestProgressBar : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private Transform claimNotification;
    [SerializeField] private GameObject buttonNameContainer;

    [SerializeField] private IntVariable currentLevel;

    void Awake()
    {
        ChestLevelPopup.claimChestLevelEvent += SetProgress;

        SetProgress();
    }

    void OnDestroy()
    {
        ChestLevelPopup.claimChestLevelEvent -= SetProgress;
    }

    private void Refresh()
    {
        bool isRewardAvailable = currentLevel.Value >=
            DataUtility.Load(GameConstants.CHEST_LEVEL, new ChestLevelData()).currentRequiredLevel;

        if (isRewardAvailable)
        {
            claimNotification.gameObject.SetActive(true);

            StartCoroutine(AnimateClaimNotification());
        }
        else
        {
            claimNotification.gameObject.SetActive(false);
        }

        buttonNameContainer.SetActive(!isRewardAvailable);
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
                    Tween.Scale(claimNotification, 1.03f, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        step = 1;
                        isWaiting = false;
                    });
                }
                else
                {
                    Tween.Scale(claimNotification, 0.97f, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        step = 0;
                        isWaiting = false;
                    });
                }

                isWaiting = true;
            }

            yield return waitOneSecond;
        }
    }

    private void SetProgress()
    {
        Refresh();

        fill.fillAmount = currentLevel.Value % 10 / 10f;
    }
}

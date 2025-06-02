using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
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

    protected override void MoreActionInAwake()
    {
        StarChestPopup.claimStarChestEvent += Refresh;

        _starGroupInitialPosition = starGroupRT.localPosition;

        Refresh();

        PlayFx();
    }

    protected override void MoreActionOnDestroy()
    {
        StarChestPopup.claimStarChestEvent -= Refresh;
    }

    private void Refresh()
    {
        bool isRewardAvailable = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData()).starCollected >= GameConstants.REQUIRE_STAR;

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
        }));
    }

    private void PlayFx()
    {
        StarLevelData data = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        int cachedPrevStarCollected = data.prevStarCollected;

        if (data.prevStarCollected == data.starCollected)
        {
            starGroupRT.gameObject.SetActive(false);

            SetProgress();

            return;
        }
        else
        {
            starText.text = $"+{data.starCollected - data.prevStarCollected}";

            data.prevStarCollected = data.starCollected;

            DataUtility.Save(GameConstants.STAR_LEVEL_DATA, data);
        }

        starGroup.gameObject.SetActive(true);

        starGroup.alpha = 0;

        fill.fillAmount = cachedPrevStarCollected % GameConstants.REQUIRE_STAR / (float)GameConstants.REQUIRE_STAR;

        AddSequence(
            Tween.Alpha(starGroup, 1, duration: 0.3f, startDelay: 1)
            .Chain(
                Tween.LocalPositionX(starGroupRT, _starGroupInitialPosition.x + 100, duration: 0.15f)
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

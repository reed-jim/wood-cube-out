using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarChestProgressBar : MonoBehaviour
{
    [SerializeField] private Image fill;

    [SerializeField] private RectTransform starGroupRT;
    [SerializeField] private CanvasGroup starGroup;
    [SerializeField] private TMP_Text starText;

    [SerializeField] private ParticleSystem fx;

    private Vector3 _starGroupInitialPosition;

    void Awake()
    {
        _starGroupInitialPosition = starGroupRT.localPosition;

        PlayFx();
    }

    private void SetProgress()
    {
        StarLevelData data = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        fill.fillAmount = 0;

        Tween.Custom(0, data.starCollected % 10 / 10f, duration: 0.3f, onValueChange: newVal =>
        {
            fill.fillAmount = newVal;
        });
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

        fill.fillAmount = cachedPrevStarCollected % 10 / 10f;

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

            Tween.Alpha(starGroup, 0, duration: 0.3f);

            SetProgress();
        });
    }
}

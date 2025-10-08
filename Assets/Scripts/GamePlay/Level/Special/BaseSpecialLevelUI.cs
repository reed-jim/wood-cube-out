using NamPhuThuy.GamePlay;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SpecialLevelUIState
{
    None = 0,
    Setup = 1,
    Showing = 2,
    ReActive = 3,
    UpdateProgress = 4,
    UpdateTimeCount = 5,
    Hide
}

public class BaseSpecialLevelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] protected RectTransform container;
    [SerializeField] protected CanvasGroup lockedGroup;
    [SerializeField] protected TMP_Text[] conditionTexts;
    [SerializeField] protected TMP_Text timeCountingText;
    [SerializeField] protected Image tickIcon;

    [SerializeField] protected LevelMode levelMode;

    void Awake()
    {
        BaseSpecialLevelManager.updateUI += UpdateUI;

        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        BaseSpecialLevelManager.updateUI -= UpdateUI;
    }

    protected virtual void Setup(LevelMode levelMode, SpecialLevelTaskRecord record = null)
    {
        if (levelMode == this.levelMode)
        {
            gameObject.SetActive(true);

            container.gameObject.SetActive(false);
            timeCountingText.gameObject.SetActive(false);
            tickIcon.gameObject.SetActive(false);
        }
    }

    protected virtual void ShowUI()
    {
        SaferioTween.ScaleUpWithPunch(container, 1);
    }

    protected virtual void HideUI()
    {
        SaferioTween.ScaleDownWithPunch(container);
    }

    protected virtual void ReActive()
    {
        foreach (var text in conditionTexts)
        {
            SaferioTween.ScaleUpWithPunch(text.transform, 1, delay: 0.3f);
        }

        SaferioTween.ScaleUpWithPunch(lockedGroup.transform, 1, delay: 0.3f);

        SaferioTween.ScaleDownWithPunch(tickIcon.transform);
        SaferioTween.ScaleDownWithPunch(timeCountingText.transform);
    }

    protected virtual void UpdateUI(LevelMode levelMode, SpecialLevelUIState state, object[] parameters)
    {

    }

    protected virtual void UpdateTimeCount(int remaningTime)
    {
        timeCountingText.text = $"Lock in {remaningTime}s";
    }
}

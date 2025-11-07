using System.Collections;
using Dreamteck.Splines.Primitives;
using NamPhuThuy.GamePlay;
using NamPhuThuy.VFX;
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
    
    // [SerializeField] protected Image reloadMaskImage;

    [SerializeField] protected LevelMode levelMode;
    
    protected Coroutine monitoringCoroutine;

    #region MonoBehaviour Callbacks

    void Awake()
    {
        BaseSpecialLevelManager.updateUI += UpdateUI;

        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        BaseSpecialLevelManager.updateUI -= UpdateUI;
    }

    #endregion

    protected virtual void Setup(LevelMode levelMode, SpecialLevelTaskRecord record = null)
    {
        if (levelMode == this.levelMode)
        {
            gameObject.SetActive(true);
            // StartCoroutine(IEActiveObjectDelay(0.3f));

            container.gameObject.SetActive(false);
            timeCountingText.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator IEActiveObjectDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(true);
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

        SaferioTween.ScaleDownWithPunch(timeCountingText.transform);
    }

    protected virtual void UpdateUI(LevelMode levelMode, SpecialLevelUIState state, object[] parameters)
    {
        switch (state)
        {
            case SpecialLevelUIState.Hide:
                
                
                break;
        }
    }

    protected virtual void UpdateTimeCount(int remaningTime)
    {
        // timeCountingText.text = $"Lock in {remaningTime}s";
        timeCountingText.text = $"{remaningTime}";
    }
}

using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RustyScrewSpecialLevelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform container;
    [SerializeField] private CanvasGroup lockedGroup;
    [SerializeField] private TMP_Text[] conditionTexts;
    [SerializeField] private TMP_Text timeCountingText;
    [SerializeField] private Image tickIcon;

    [SerializeField] private Image rustyScrew;
    [SerializeField] private Image tempRustyScrew;
    [SerializeField] private Sprite[] rustyProgressSprites;

    void Awake()
    {
        RustyScrewSpecialLevelManager.updateUI += UpdateUI;

        container.gameObject.SetActive(false);
        timeCountingText.gameObject.SetActive(false);
        tickIcon.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        RustyScrewSpecialLevelManager.updateUI -= UpdateUI;
    }

    private void UpdateUI(RustyScrewLevelActionUI action, object[] parameters)
    {
        if (action == RustyScrewLevelActionUI.Showing)
        {
            ShowUI();
        }
        if (action == RustyScrewLevelActionUI.MakeScrewRusty)
        {
            MakeScrewRusty();
        }
        else if (action == RustyScrewLevelActionUI.UpdateRustyLevel)
        {
            UpdateRustyLevel((int)parameters[0]);
        }
    }

    private void ShowUI()
    {
        SaferioTween.ScaleUpWithPunch(container, 1);
    }

    private void MakeScrewRusty()
    {
        foreach (var text in conditionTexts)
        {
            SaferioTween.ScaleUpWithPunch(text.transform, 1, delay: 0.3f);
        }

        SaferioTween.ScaleUpWithPunch(lockedGroup.transform, 1, delay: 0.3f);

        SaferioTween.ScaleDownWithPunch(tickIcon.transform);
        SaferioTween.ScaleDownWithPunch(timeCountingText.transform);
    }

    private void UpdateRustyLevel(int rustyClearingLevel)
    {
        conditionTexts[0].text = $"{rustyClearingLevel}/{3}";

        if (rustyClearingLevel == 0)
        {
            foreach (var text in conditionTexts)
            {
                SaferioTween.ScaleDownWithPunch(text.transform);
            }

            SaferioTween.ScaleDownWithPunch(lockedGroup.transform);

            SaferioTween.ScaleUpWithPunch(tickIcon.transform, 1, delay: 0.3f);
            SaferioTween.ScaleUpWithPunch(timeCountingText.transform, 1, delay: 0.3f);
        }

        // Rusty
        if (rustyClearingLevel < rustyProgressSprites.Length)
        {
            tempRustyScrew.gameObject.SetActive(true);

            tempRustyScrew.sprite = rustyScrew.sprite;

            rustyScrew.color = ColorUtil.WithAlpha(rustyScrew.color, 0);
            rustyScrew.sprite = rustyProgressSprites[rustyClearingLevel];

            Tween.Alpha(rustyScrew, 1, duration: 0.3f);
            Tween.Alpha(tempRustyScrew, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                tempRustyScrew.gameObject.SetActive(false);
            });
        }
        else
        {
            tempRustyScrew.gameObject.SetActive(false);

            Tween.Alpha(rustyScrew, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                rustyScrew.gameObject.SetActive(false);
            });
        }
    }
}

using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum VoyageScreenMode
{
    Normal,
    Mini
}

public class VoyageScreen : MonoBehaviour
{
    [Header("Normal Mode")]
    [SerializeField] private RectTransform normalModeContainer;
    [SerializeField] private CanvasGroup normalModeCanvasGroup;
    [SerializeField] private Button minimizeButton;

    [Header("Mini Mode")]
    [SerializeField] private RectTransform miniModeContainer;
    [SerializeField] private CanvasGroup miniModeCanvasGroup;
    [SerializeField] private Button maximizeButton;
    [SerializeField] private TMP_Text miniModeText;

    private VoyageScreenMode voyageScreenMode;
    private string primaryHex;

    private void Awake()
    {
        SpaceShip.changeCurrentVoyageProgressEvent += OnChangeCurrentVoyageProgress;

        minimizeButton.onClick.AddListener(ShowMiniMode);
        maximizeButton.onClick.AddListener(ShowNormalMode);
    }

    void OnDestroy()
    {
        SpaceShip.changeCurrentVoyageProgressEvent -= OnChangeCurrentVoyageProgress;
    }

    void Start()
    {
        primaryHex = DynamicThemeManager.Ins.PrimaryHex;
    }

    private void ShowNormalMode()
    {
        DisableChangeVisibilityButtons();

        normalModeContainer.gameObject.SetActive(true);

        Tween.Alpha(miniModeCanvasGroup, 1, duration: 0.3f);

        HideMiniMode();

        voyageScreenMode = VoyageScreenMode.Normal;
    }

    private void HideNormalMode()
    {
        Tween.Alpha(miniModeCanvasGroup, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            normalModeContainer.gameObject.SetActive(false);

            EnableChangeVisibilityButtons();
        });
    }

    private void ShowMiniMode()
    {
        DisableChangeVisibilityButtons();

        miniModeContainer.gameObject.SetActive(true);

        Tween.Alpha(miniModeCanvasGroup, 1, duration: 0.3f);

        HideNormalMode();

        voyageScreenMode = VoyageScreenMode.Mini;
    }

    private void HideMiniMode()
    {
        Tween.Alpha(miniModeCanvasGroup, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            miniModeContainer.gameObject.SetActive(false);

            EnableChangeVisibilityButtons();
        });
    }

    private void EnableChangeVisibilityButtons()
    {
        minimizeButton.interactable = true;
        maximizeButton.interactable = true;
    }

    private void DisableChangeVisibilityButtons()
    {
        minimizeButton.interactable = false;
        maximizeButton.interactable = false;
    }

    #region Mini Mode
    private void UpdateMiniMode(float progress)
    {
        miniModeText.text = $"{CommonUtil.FormatNumber(progress * 100)}% to <color={primaryHex}>{StarManager.Ins.GetNextStarPathConfig().starName}";
    }
    #endregion

    #region Event
    private void OnChangeCurrentVoyageProgress(float progress)
    {
        if (voyageScreenMode == VoyageScreenMode.Mini)
        {
            UpdateMiniMode(progress);
        }
        else
        {

        }
    }
    #endregion
}

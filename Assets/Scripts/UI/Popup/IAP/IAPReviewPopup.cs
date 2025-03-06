using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IAPReviewPopup : BasePopup
{
    [SerializeField] private TMP_Text packNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image icon;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("DATA")]
    [SerializeField] private IAPPackageData[] packagesIAP;
    [SerializeField] private Sprite[] packIcons;

    #region PRIVATE FIELD
    private int _currentPackIndex;
    #endregion

    protected override void MoreActionInAwake()
    {
        ShowPack(0);
    }

    protected override void RegisterMoreEvent()
    {
        buyButton.onClick.AddListener(ReviewBuy);
        nextButton.onClick.AddListener(Next);
        prevButton.onClick.AddListener(Prev);
    }

    private void ShowPack(int packIndex)
    {
        packNameText.text = packagesIAP[packIndex].ProductName;
        priceText.text = $"${packagesIAP[packIndex].Price}";
        icon.sprite = packIcons[packIndex];

        UIUtil.SetSizeKeepRatioX(icon, 0.35f * container.sizeDelta.x);
    }

    private void ReviewBuy()
    {

    }

    private void Next()
    {
        _currentPackIndex++;

        ShowPack(_currentPackIndex);
    }

    private void Prev()
    {
        _currentPackIndex--;

        ShowPack(_currentPackIndex);
    }
}

using System;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text languageText;

    [Header("Flags")]
    [SerializeField] private bool isSelected;

    [Header("Selected state")]
    [SerializeField] private Image selectedBackground;
    [SerializeField] private Image tick;

    [Header("Unselected state")]
    [SerializeField] private Image unselectedBackground;

    [SerializeField] private Material activeTextMaterial;
    [SerializeField] private Material inactiveTextMaterial;

    [SerializeField] private string language;

    private Action _onSelect;

    void Awake()
    {
        LanguagePopup.changeLanguageEvent += OnLanguageChanged;

        selectButton.onClick.AddListener(Select);
    }

    void OnDestroy()
    {
        LanguagePopup.changeLanguageEvent -= OnLanguageChanged;
    }

    public void SetStateBasedOnLanguage(string language)
    {
        isSelected = language == this.language;

        if (isSelected)
        {
            selectedBackground.gameObject.SetActive(true);
            unselectedBackground.gameObject.SetActive(false);
            languageText.fontMaterial = activeTextMaterial;
            SaferioTween.AlphaShow(tick);
        }
        else
        {
            selectedBackground.gameObject.SetActive(false);
            unselectedBackground.gameObject.SetActive(true);
            languageText.fontMaterial = inactiveTextMaterial;
            SaferioTween.AlphaHide(tick);
        }
    }

    public void RegisterOnSelectAction(Action onSelected)
    {
        _onSelect = onSelected;
    }

    private void Select()
    {
        _onSelect?.Invoke();
    }

    public bool IsThisLanguageSelected(string currentLanguage)
    {
        return currentLanguage == language;
    }

    private void OnLanguageChanged(string language)
    {
        Debug.Log($"OnLanguageChanged {language}");
        SetStateBasedOnLanguage(language);
    }
}

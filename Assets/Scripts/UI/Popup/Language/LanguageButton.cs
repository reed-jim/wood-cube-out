using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image activeIcon;
    [SerializeField] private TMP_Text languageText;

    [Header("Flags")] 
    [SerializeField] private bool isSelected;
    
    [Header("Selected state")]
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Image selectedBackground;
    
    [Header("Unselected state")]
    [SerializeField] private Material unselectedMaterial;
    [SerializeField] private Image unselectedBackground;
    
    
    [SerializeField] private string language;

    private Action _onSelect;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void SetStateBasedOnLanguage(string language)
    {
        isSelected = language == this.language;

        activeIcon.gameObject.SetActive(isSelected);

        if (isSelected)
        {
            /*languageText.color = ColorUtil.GetColorFromHex("#83EE64");
            languageText.fontSharedMaterial = selectedMaterial;*/
            selectedBackground.gameObject.SetActive(true);
            unselectedBackground.gameObject.SetActive(false);
        }
        else
        {
            /*languageText.color = ColorUtil.GetColorFromHex("#ffffff");
            languageText.fontSharedMaterial = unselectedMaterial;*/
            selectedBackground.gameObject.SetActive(false);
            unselectedBackground.gameObject.SetActive(true);
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
}

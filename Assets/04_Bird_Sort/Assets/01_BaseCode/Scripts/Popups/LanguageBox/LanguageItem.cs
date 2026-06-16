using System;
using UnityEngine;
using UnityEngine.UI;

public class LanguageItem : MonoBehaviour
{
    [SerializeField] private Text languageText;
    [SerializeField] private Image selectedImage;
    private Action<LanguageItem> _clickAction;
    private SystemLanguage? _language;
    [SerializeField] private Sprite onSprite, offSprite;

    private void OnEnable()
    {
        if (_language.HasValue)
            languageText.text = GetLanguageText(_language.Value);
    }

    public void Init(SystemLanguage language,Action<LanguageItem> clickAction)
    {
        _language = language; 
        _clickAction = clickAction;
        languageText.text = GetLanguageText(_language.Value);
    }

    private string GetLanguageText(SystemLanguage language)
    {
        
        return string.CompareOrdinal(Localization.language, SystemLanguage.English.ToString()) == 0
            ? language.ToString()
            : string.Format("{1}  ({0})", Localization.Get(language.ToString()), language);
            
            
        /*return string.CompareOrdinal(Localization.language, language.ToString()) == 0
            ? Localization.Get(language.ToString())
            : string.Format("{1}<size=10>({0})</size>", Localization.Get(language.ToString()),
                Localization.Get(language.ToString(), language));*/
                
    }

    public void OnClickFlagButton() {
        if (_clickAction != null) _clickAction(this);

        GameController.Instance.musicManager.PlayClickSound();
    }

    public void ActiveSelected(bool isSelect) {
        if (isSelect)
            selectedImage.sprite = onSprite;
        else
            selectedImage.sprite = offSprite;
    }
}

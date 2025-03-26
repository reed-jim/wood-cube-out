using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LanguageBox : BaseBox
{
    private static LanguageBox instance;
    public static LanguageBox Setup(bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            // Create popup and attach it to UI
            instance = Instantiate(Resources.Load<LanguageBox>(PathPrefabs.LANGUAGE_BOX));
        }
        
        return instance;
    }

    [SerializeField] private LanguageSpriteDictionary flagDict;
    [SerializeField] private Transform parentGroup;
    [SerializeField] private LanguageItem sampleFlagButton;
    private LanguageItem[] _flagItems;
    private Action _changeLanguageAction, _closeAction;
    [SerializeField] private Button closeButton;

    #region Unity Method
    private void Start()
    {
        closeButton.onClick.AddListener(OnClickDismiss);
        Init(ReloadFlag, () => { Close(); });
    }

    #endregion

    #region Public Methods

    public void Init(Action changeLanguageAction = null, Action closeAction = null) {
        _flagItems = new LanguageItem[flagDict.Value.Count];
        int i = 0;
        foreach (var lanuage in flagDict.Value) {
            var newFlag = Instantiate(sampleFlagButton, parentGroup.transform, false);
            newFlag.Init(lanuage, item => {
                OnSelectFlagItem(item);
                OnClickLanguage(lanuage.ToString());
            });
            newFlag.ActiveSelected(
                string.Compare(Localization.language, lanuage.ToString(), StringComparison.Ordinal) == 0);
            
            newFlag.name = lanuage.ToString();
            _flagItems[i] = newFlag;
            i++;
        }
        _changeLanguageAction = changeLanguageAction;
        _closeAction = closeAction;
    }

    public void OnClickLanguage(string language) {
        if (string.CompareOrdinal(Localization.language, language) != 0)
        {
            //PlayerPrefs.SetString("Language", language);
            Localization.LoadCSV(Localization.loadFunction.Invoke(language), true);
            Localization.language = language;
        }

        var uiLnaguage = FindObjectsOfType<LocalizeBehaviour>();
        for (var i = 0; i < uiLnaguage.Length; i++) {
            uiLnaguage[i].OnLocalize();
        }
        if (_changeLanguageAction != null)
            _changeLanguageAction();

        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_LANGUAGE);
        OnClickDismiss();
    }

    public void OnClickDismiss() {
        if (_closeAction != null)
            _closeAction();
    }

    #endregion

    #region Private Methods
    private void OnSelectFlagItem(LanguageItem item) {
        foreach (LanguageItem flagItem in _flagItems) {
            flagItem.ActiveSelected(flagItem == item);            
        }
    }
    
    private void ReloadFlag()
    {
        //languageText.text = Localization.Get(Localization.language);
    }
    #endregion

    
}
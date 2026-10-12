using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CompensatePopup : BasePopup
{
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text idText;

    [SerializeField] private Button redeemButton;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action updateLevelEvent;
    public static event Action updateCoinTextEvent;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        redeemButton.onClick.AddListener(Redeem);
    }

    protected override void Show(Action onCompletedAction = null)
    {
        idText.text = $"ID: {GamePersistentVariable.FirebaseUserId}";

        base.Show(onCompletedAction);
    }

    private void Redeem()
    {
        
    }

    #region UTIL
    private int ExtractIntFromString(string input)
    {
        string pattern = @"\d+";

        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            return int.Parse(match.Value);
        }

        return 0;
    }
    #endregion

    [System.Serializable]
    public class ValidUniqueDeviceIdentifierContainer
    {
        public string[] validIdentifiers;
    }
}

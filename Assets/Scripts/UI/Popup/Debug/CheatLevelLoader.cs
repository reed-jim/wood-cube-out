using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheatLevelLoader : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("The InputField (TMP) to type the level number.")]
    [SerializeField] private TMP_InputField tmpInputField;
    
    [Tooltip("The standard InputField (Legacy) to type the level number (if not using TMP).")]
    [SerializeField] private InputField legacyInputField;

    [Tooltip("The Button that will trigger the level load when clicked.")]
    [SerializeField] private Button loadButton;
    [SerializeField] private DebugPopup debugPopupController;


    private void Start()
    {
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadLevelClicked);
        }
        else
        {
            Debug.LogWarning("CheatLevelLoader: Load Button is not assigned in the Inspector!");
        }
    }

    private void OnDestroy()
    {
        if (loadButton != null)
        {
            loadButton.onClick.RemoveListener(OnLoadLevelClicked);
        }
    }

    /// <summary>
    /// Reads the input text and triggers the level loading sequence.
    /// </summary>
    public void OnLoadLevelClicked()
    {
        string levelStr = string.Empty;

        // Get text from TMP InputField if assigned, otherwise fallback to Legacy InputField
        if (tmpInputField != null)
        {
            levelStr = tmpInputField.text;
        }
        else if (legacyInputField != null)
        {
            levelStr = legacyInputField.text;
        }

        if (string.IsNullOrEmpty(levelStr))
        {
            Debug.LogWarning("CheatLevelLoader: Level input text is empty.");
            return;
        }

        if (int.TryParse(levelStr.Trim(), out int levelNum))
        {
            if (levelNum > 0)
            {
                Debug.Log($"CheatLevelLoader: Requesting load of level {levelNum}...");
                // Trigger the existing static debug event that LevelLoader subscribes to.
                DebugPopup.TriggerToLevelEvent(levelNum);
                debugPopupController.HidePopup();   
            }
            else
            {
                Debug.LogError("CheatLevelLoader: Level number must be greater than 0.");
            }
        }
        else
        {
            Debug.LogError($"CheatLevelLoader: Invalid level number format: '{levelStr}'");
        }
    }
}

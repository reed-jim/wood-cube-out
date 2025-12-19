#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using Lean.Localization;

public class LeanLocalizationUtilityTool : EditorWindow
{
    private string lastExportPath;
    private int exportedCount;

    #region Translate single text
    private string gameObjectName = "";
    private string englishText = "";
    private string translationFilePath = "";
    private int appliedCount = 0;
    #endregion

    [MenuItem("Tools/Lean Localization Utility Tool")]
    static void Open()
    {
        GetWindow<LeanLocalizationUtilityTool>("Lean Localization Utility Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("LeanPhrase English Text Export", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Exports English entries from all LeanPhrase components in loaded scenes.\n" +
            "Each GameObject will be written as one line.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Export English Text", GUILayout.Height(40)))
        {
            ExportEnglish();
        }

        if (GUILayout.Button("Import Language", GUILayout.Height(40)))
        {
            ImportLanguage("Japanese");
        }

        EditorGUILayout.Space();

        if (!string.IsNullOrEmpty(lastExportPath))
        {
            EditorGUILayout.LabelField("Last Export:", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(lastExportPath, GUILayout.Height(16));
            EditorGUILayout.LabelField("Lines Exported:", exportedCount.ToString());
        }

        #region Translate single text
        EditorGUILayout.Space(80);

        GUILayout.Label("Apply Translations to LeanPhrase", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("All Languages", "English, French, German, Italian, Japanese, Korean, Portuguese, Russian, Spanish");
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        gameObjectName = EditorGUILayout.TextField("GameObject Name", gameObjectName);
        englishText = EditorGUILayout.TextField("English Text", englishText);

        EditorGUILayout.BeginHorizontal();
        translationFilePath = EditorGUILayout.TextField("Translation File", translationFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select Translation File", Application.dataPath, "txt");
            if (!string.IsNullOrEmpty(path))
                translationFilePath = path;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply Translations", GUILayout.Height(40)))
        {
            if (string.IsNullOrEmpty(gameObjectName) || string.IsNullOrEmpty(englishText) || string.IsNullOrEmpty(translationFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please fill in all fields.", "OK");
            }
            else
            {
                ApplyTranslationsFromFile(gameObjectName, englishText, translationFilePath);
            }
        }
        #endregion
    }

    void ExportEnglish()
    {
        string path = EditorUtility.SaveFilePanel(
            "Export English Localization",
            Application.dataPath,
            "English_Text_Export.txt",
            "txt"
        );

        if (string.IsNullOrEmpty(path))
            return;

        StringBuilder sb = new StringBuilder();
        exportedCount = 0;

        Transform leanLocalization = Selection.activeTransform;

        LeanPhrase[] phrases = TransformUtil.GetComponentsArrayFromAllChildren<LeanPhrase>(leanLocalization);

        foreach (var phrase in phrases)
        {
            foreach (var entry in phrase.Entries)
            {
                if (entry.Language == "English" && !string.IsNullOrEmpty(entry.Text))
                {
                    sb.AppendLine($"{entry.Text}");
                    exportedCount++;
                    break;
                }
            }
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        lastExportPath = path;
        Debug.Log($"LeanPhrase English export complete ({exportedCount} lines)");
    }

    private int ImportLanguage(string language)
    {
        string title = "Select Translation File";
        string defaultFolder = Application.dataPath; // Starts in Assets folder
        string extension = "txt"; // Only show .txt files

        // Open file panel
        string filePath = EditorUtility.OpenFilePanel(title, defaultFolder, extension);

        if (!File.Exists(filePath))
        {
            Debug.LogError("Translation file not found: " + filePath);
            return 0;
        }

        Transform leanLocalization = Selection.activeTransform;

        LeanPhrase[] phrases = TransformUtil.GetComponentsArrayFromAllChildren<LeanPhrase>(leanLocalization);

        string[] lines = File.ReadAllLines(filePath);
        int updatedCount = 0;
        int count = Mathf.Min(phrases.Length, lines.Length);

        for (int i = 0; i < count; i++)
        {
            LeanPhrase phrase = phrases[i];
            string translatedText = lines[i].Trim();
            if (string.IsNullOrEmpty(translatedText)) continue;

            bool updated = false;
            foreach (var entry in phrase.Entries)
            {
                if (entry.Language == language)
                {
                    entry.Text = translatedText;
                    updated = true;
                    updatedCount++;
                    EditorUtility.SetDirty(phrase);
                    break;
                }
            }

            if (!updated)
            {
                var newEntry = new LeanPhrase.Entry
                {
                    Language = language,
                    Text = translatedText,
                    Object = null
                };
                phrase.Entries.Add(newEntry);
                updatedCount++;
                EditorUtility.SetDirty(phrase);
            }
        }

        EditorUtility.SetDirty(Selection.activeTransform);

        Debug.Log($"{language} translations imported: {updatedCount} entries updated.");

        return updatedCount;
    }

    private void ApplyTranslationsFromFile(string gameObjectName, string englishText, string translationFilePath)
    {
        if (!File.Exists(translationFilePath))
        {
            Debug.LogError("Translation file not found: " + translationFilePath);
            return;
        }

        string[] lines = File.ReadAllLines(translationFilePath);
        int lineIndex = 0;

        GameObject target = GameObject.Find(gameObjectName);
        if (target == null)
        {
            Debug.LogWarning($"GameObject not found: {gameObjectName}");
            return;
        }

        LeanPhrase phrase = target.GetComponent<LeanPhrase>();
        if (phrase == null)
        {
            Debug.LogWarning($"LeanPhrase component not found on: {gameObjectName}");
            return;
        }

        // Find English entry
        LeanPhrase.Entry englishEntry = null;
        foreach (var entry in phrase.Entries)
        {
            if (entry.Language == "English" && entry.Text == englishText)
            {
                englishEntry = entry;
                break;
            }
        }

        if (englishEntry == null)
        {
            Debug.LogWarning($"English entry not found or does not match: {englishText}");
            return;
        }

        // Assign translations line by line to other languages
        foreach (var entry in phrase.Entries)
        {
            if (entry.Language == "English") continue;

            if (lineIndex >= lines.Length)
            {
                Debug.LogWarning($"Not enough lines in translation file for all non-English entries of {gameObjectName}");
                break;
            }

            entry.Text = lines[lineIndex].Trim();

            lineIndex++;

            EditorUtility.SetDirty(phrase);
        }

        EditorUtility.SetDirty(Selection.activeTransform);

        Debug.Log($"Applied {lineIndex} translations to {gameObjectName}");
    }
}
#endif

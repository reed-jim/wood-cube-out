#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LevelOverviewWindow : EditorWindow
{
    public enum UITheme
    {
        ModernSlate,
        MidnightSynth,
        NordicLight,
        CorgiDelight,
        Dracula
    }

    private struct ThemePalette
    {
        public Color bgDark;
        public Color bgCard;
        public Color bgHeader;
        public Color bgRowAlt;
        public Color accentPrimary;
        public Color textMain;
        public Color textMuted;
        public Color lowDiff;
        public Color midDiff;
        public Color highDiff;
    }

    private const float CARD_HEIGHT = 80f;
    private const string PREF_KEY_THEME = "LevelOverviewWindow_SelectedTheme";

    private DefaultAsset levelsFolder;
    private string search = "";
    private Vector2 scroll;
    private UITheme currentTheme = UITheme.ModernSlate;

    private List<LevelData> levels = new List<LevelData>();
    private Dictionary<string, int> difficultyCounts = new Dictionary<string, int>();

    private GUIStyle titleStyle, subtitleStyle, cardTitleStyle, cardNumberStyle, tableHeaderStyle, tableRowStyle;
    private ThemePalette palette;

    [MenuItem("Tools/Level Overview")]
    public static void ShowWindow()
    {
        var window = GetWindow<LevelOverviewWindow>("Level Overview");
        window.minSize = new Vector2(820, 520);
        window.Show();
    }

    private void OnEnable()
    {
        currentTheme = (UITheme)EditorPrefs.GetInt(PREF_KEY_THEME, (int)UITheme.ModernSlate);
        ApplyThemePalette();
        CreateStyles();
        LoadLevels();
    }

    private void OnGUI()
    {
        CreateStyles();
        DrawBackground();
        DrawHeader();
        DrawToolbar();

        if (levelsFolder == null)
        {
            DrawEmptyState();
            return;
        }

        DrawOverview();
        DrawDifficultyDistribution();
        DrawLevelList();
    }

    // ------------------------------------------------------------
    // THEMES & STYLES
    // ------------------------------------------------------------

    private void ApplyThemePalette()
    {
        switch (currentTheme)
        {
            case UITheme.MidnightSynth:
                palette = new ThemePalette
                {
                    bgDark = new Color(0.07f, 0.05f, 0.12f),
                    bgCard = new Color(0.12f, 0.09f, 0.20f),
                    bgHeader = new Color(0.18f, 0.13f, 0.28f),
                    bgRowAlt = new Color(0.09f, 0.07f, 0.15f),
                    accentPrimary = new Color(1.00f, 0.00f, 0.55f),
                    textMain = new Color(0.98f, 0.95f, 1.00f),
                    textMuted = new Color(0.65f, 0.55f, 0.75f),
                    lowDiff = new Color(0.00f, 0.95f, 1.00f),
                    midDiff = new Color(1.00f, 0.80f, 0.00f),
                    highDiff = new Color(1.00f, 0.10f, 0.40f)
                };
                break;

            case UITheme.NordicLight:
                palette = new ThemePalette
                {
                    bgDark = new Color(0.94f, 0.95f, 0.96f),
                    bgCard = new Color(1.00f, 1.00f, 1.00f),
                    bgHeader = new Color(0.88f, 0.90f, 0.92f),
                    bgRowAlt = new Color(0.90f, 0.92f, 0.94f),
                    accentPrimary = new Color(0.15f, 0.45f, 0.85f),
                    textMain = new Color(0.12f, 0.14f, 0.18f),
                    textMuted = new Color(0.42f, 0.45f, 0.50f),
                    lowDiff = new Color(0.05f, 0.65f, 0.50f),
                    midDiff = new Color(0.88f, 0.55f, 0.05f),
                    highDiff = new Color(0.85f, 0.20f, 0.25f)
                };
                break;

            case UITheme.CorgiDelight:
                palette = new ThemePalette
                {
                    bgDark = new Color(0.96f, 0.93f, 0.88f),
                    bgCard = new Color(1.00f, 0.98f, 0.94f),
                    bgHeader = new Color(0.91f, 0.85f, 0.76f),
                    bgRowAlt = new Color(0.93f, 0.88f, 0.81f),
                    accentPrimary = new Color(0.86f, 0.52f, 0.22f), // Warm Corgi Fur Orange
                    textMain = new Color(0.24f, 0.18f, 0.14f),
                    textMuted = new Color(0.55f, 0.46f, 0.38f),
                    lowDiff = new Color(0.38f, 0.68f, 0.45f),     // Gentle Sage Green
                    midDiff = new Color(0.90f, 0.62f, 0.18f),     // Toasted Caramel
                    highDiff = new Color(0.82f, 0.32f, 0.26f)      // Paw Pad Pink-Red
                };
                break;

            case UITheme.Dracula:
                palette = new ThemePalette
                {
                    bgDark = new Color(0.16f, 0.16f, 0.21f),
                    bgCard = new Color(0.26f, 0.27f, 0.35f),
                    bgHeader = new Color(0.20f, 0.21f, 0.28f),
                    bgRowAlt = new Color(0.18f, 0.19f, 0.25f),
                    accentPrimary = new Color(0.74f, 0.46f, 1.00f), // Dracula Purple
                    textMain = new Color(0.95f, 0.96f, 0.96f),
                    textMuted = new Color(0.38f, 0.44f, 0.64f),
                    lowDiff = new Color(0.31f, 0.98f, 0.48f),     // Mint Green
                    midDiff = new Color(0.95f, 0.98f, 0.55f),     // Soft Yellow
                    highDiff = new Color(1.00f, 0.34f, 0.34f)      // Red
                };
                break;

            case UITheme.ModernSlate:
            default:
                palette = new ThemePalette
                {
                    bgDark = new Color(0.05f, 0.06f, 0.08f),
                    bgCard = new Color(0.09f, 0.10f, 0.13f),
                    bgHeader = new Color(0.14f, 0.16f, 0.20f),
                    bgRowAlt = new Color(0.07f, 0.08f, 0.10f),
                    accentPrimary = new Color(0.38f, 0.40f, 0.98f),
                    textMain = new Color(0.96f, 0.97f, 0.98f),
                    textMuted = new Color(0.55f, 0.58f, 0.65f),
                    lowDiff = new Color(0.00f, 0.82f, 0.90f),
                    midDiff = new Color(1.00f, 0.70f, 0.18f),
                    highDiff = new Color(1.00f, 0.25f, 0.42f)
                };
                break;
        }
    }

    private void CreateStyles()
    {
        titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = palette.textMain } };
        subtitleStyle = new GUIStyle(EditorStyles.label) { fontSize = 11, normal = { textColor = palette.textMuted } };
        cardTitleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 10, fontStyle = FontStyle.Bold, normal = { textColor = palette.textMuted } };
        cardNumberStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = palette.textMain } };
        tableHeaderStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 10, normal = { textColor = palette.textMuted } };
        tableRowStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 11, normal = { textColor = palette.textMain }, hover = { textColor = palette.accentPrimary } };
    }

    // ------------------------------------------------------------
    // HEADER & TOOLBAR
    // ------------------------------------------------------------

    private void DrawHeader()
    {
        GUILayout.Space(16);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);

        EditorGUILayout.BeginVertical();
        string headerText = currentTheme == UITheme.CorgiDelight ? "LEVEL OVERVIEW 🐶" : "LEVEL OVERVIEW";
        GUILayout.Label(headerText, titleStyle);
        GUILayout.Label("A clear look at your level difficulty distribution", subtitleStyle);
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        EditorGUI.BeginChangeCheck();
        UITheme newTheme = (UITheme)EditorGUILayout.EnumPopup(currentTheme, GUILayout.Width(130), GUILayout.Height(28));
        if (EditorGUI.EndChangeCheck())
        {
            currentTheme = newTheme;
            EditorPrefs.SetInt(PREF_KEY_THEME, (int)currentTheme);
            ApplyThemePalette();
        }

        GUILayout.Space(8);

        if (GUILayout.Button("Export CSV", GUILayout.Width(100), GUILayout.Height(28)))
            ExportToCSV();

        GUILayout.Space(6);

        if (GUILayout.Button("Refresh", GUILayout.Width(75), GUILayout.Height(28)))
            LoadLevels();

        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(14);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);

        EditorGUI.BeginChangeCheck();
        levelsFolder = (DefaultAsset)EditorGUILayout.ObjectField("Levels Folder", levelsFolder, typeof(DefaultAsset), false, GUILayout.Height(22));
        if (EditorGUI.EndChangeCheck()) LoadLevels();

        GUILayout.Space(16);
        GUILayout.Label("Search", subtitleStyle, GUILayout.Width(45));
        search = EditorGUILayout.TextField(search, GUILayout.Height(22));

        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(14);
    }

    // ------------------------------------------------------------
    // OVERVIEW CARDS
    // ------------------------------------------------------------

    private void DrawOverview()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);

        DrawStatCard("TOTAL LEVELS", levels.Count.ToString(), palette.accentPrimary);
        GUILayout.Space(10);

        string mostCommon = GetMostCommonDifficulty();
        DrawStatCard("MOST COMMON", string.IsNullOrEmpty(mostCommon) ? "-" : $"{mostCommon}", palette.midDiff);
        GUILayout.Space(10);

        DrawStatCard("DIFFICULTIES", difficultyCounts.Count.ToString(), palette.lowDiff);

        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(16);
    }

    private void DrawStatCard(string title, string value, Color accent)
    {
        Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(CARD_HEIGHT));
        rect.width = (position.width - 60f) / 3f;

        EditorGUI.DrawRect(rect, palette.bgCard);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4, rect.height), accent);

        GUI.Label(new Rect(rect.x + 14, rect.y + 12, rect.width - 28, 16), title, cardTitleStyle);
        GUI.Label(new Rect(rect.x + 14, rect.y + 32, rect.width - 28, 36), value, cardNumberStyle);
    }

    // ------------------------------------------------------------
    // DIFFICULTY DISTRIBUTION
    // ------------------------------------------------------------

    private void DrawDifficultyDistribution()
    {
        if (difficultyCounts.Count == 0) return;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("DIFFICULTY DISTRIBUTION", cardTitleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label($"{levels.Count} levels", subtitleStyle);
        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);
        EditorGUILayout.BeginVertical();

        foreach (var pair in difficultyCounts.OrderBy(x => GetDifficultyValue(x.Key)))
            DrawDifficultyRow(pair.Key, pair.Value, levels.Count);

        EditorGUILayout.EndVertical();
        GUILayout.Space(14);
    }

    private void DrawDifficultyRow(string difficulty, int count, int total)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);

        GUILayout.Label($"{difficulty}", subtitleStyle, GUILayout.Width(120));
        Rect barRect = GUILayoutUtility.GetRect(100, 16, GUILayout.ExpandWidth(true));
        float percentage = total > 0 ? (float)count / total : 0;

        EditorGUI.DrawRect(barRect, palette.bgHeader);
        EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * percentage, barRect.height), GetDifficultyColor(difficulty));

        GUILayout.Space(10);
        GUILayout.Label($"{count} ({percentage:P0})", subtitleStyle, GUILayout.Width(90));

        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(4);
    }

    // ------------------------------------------------------------
    // LEVEL LIST & EXPORT
    // ------------------------------------------------------------

    private void DrawLevelList()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("LEVELS", cardTitleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label($"{GetFilteredLevels().Count} shown", subtitleStyle);
        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);
        Rect headerRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(28));
        EditorGUI.DrawRect(headerRect, palette.bgHeader);

        GUI.Label(new Rect(headerRect.x + 20, headerRect.y, 60, 28), "#", tableHeaderStyle);
        GUI.Label(new Rect(headerRect.x + 60, headerRect.y, 400, 28), "LEVEL", tableHeaderStyle);
        GUI.Label(new Rect(headerRect.x + position.width - 170, headerRect.y, 130, 28), "DIFFICULTY", tableHeaderStyle);

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
        List<LevelData> filtered = GetFilteredLevels();

        for (int i = 0; i < filtered.Count; i++)
            DrawLevelRow(filtered[i], i);

        EditorGUILayout.EndScrollView();
    }

    private void DrawLevelRow(LevelData level, int index)
    {
        Rect rowRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(32));

        if (index % 2 == 0)
            EditorGUI.DrawRect(rowRect, palette.bgRowAlt);

        GUI.Label(new Rect(rowRect.x + 20, rowRect.y, 40, rowRect.height), (index + 1).ToString(), subtitleStyle);

        if (GUI.Button(new Rect(rowRect.x + 60, rowRect.y, rowRect.width - 250, rowRect.height), level.name, tableRowStyle))
        {
            Selection.activeObject = level.asset;
            EditorGUIUtility.PingObject(level.asset);
        }

        Color color = GetDifficultyColor(level.difficulty);
        Rect pill = new Rect(rowRect.x + rowRect.width - 170, rowRect.y + 5, 130, 22);

        EditorGUI.DrawRect(pill, new Color(color.r, color.g, color.b, 0.20f));

        GUIStyle pillStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = color },
            fontStyle = FontStyle.Bold
        };

        GUI.Label(pill, $"{level.difficulty}", pillStyle);
    }

    private void ExportToCSV()
    {
        if (levels.Count == 0)
        {
            EditorUtility.DisplayDialog("Export CSV", "No level data available to export.", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Export Level Overview for Google Sheets", "", "LevelOverview.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Index,Level Name,Difficulty");

        for (int i = 0; i < levels.Count; i++)
        {
            LevelData level = levels[i];
            sb.AppendLine($"{i + 1},\"{EscapeCsvField(level.name)}\",\"{EscapeCsvField(level.difficulty)}\"");
        }

        try
        {
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            EditorUtility.DisplayDialog("Export Successful", $"CSV saved to:\n{path}\n\nReady to import into Google Sheets.", "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Export Failed", $"Error writing file:\n{ex.Message}", "OK");
        }
    }

    private static string EscapeCsvField(string field) => string.IsNullOrEmpty(field) ? "" : field.Replace("\"", "\"\"");

    // ------------------------------------------------------------
    // EMPTY STATE & DATA
    // ------------------------------------------------------------

    private void DrawEmptyState()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginVertical(GUILayout.Width(380));
        GUILayout.Label("No Levels Folder Selected", titleStyle, GUILayout.Height(30));
        GUILayout.Label("Select the folder containing your level prefabs above.", subtitleStyle);
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Each prefab should contain a LevelItem component with a serialized 'difficulty' field.", MessageType.Info);
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    private void LoadLevels()
    {
        levels.Clear();
        difficultyCounts.Clear();

        if (levelsFolder == null) return;
        string folderPath = AssetDatabase.GetAssetPath(levelsFolder);
        if (!AssetDatabase.IsValidFolder(folderPath)) return;

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            AntsFlow.LevelItem levelItem = prefab.transform.GetComponent<AntsFlow.LevelItem>();
            if (levelItem == null) continue;

            SerializedObject serializedObject = new SerializedObject(levelItem);
            SerializedProperty difficulty = serializedObject.FindProperty("difficulty");
            if (difficulty == null) continue;

            string difficultyName = $"{levelItem.difficulty}";
            levels.Add(new LevelData { name = prefab.name, asset = prefab, difficulty = difficultyName });

            if (!difficultyCounts.ContainsKey(difficultyName))
                difficultyCounts[difficultyName] = 0;

            difficultyCounts[difficultyName]++;
        }

        levels = levels.OrderBy(x => GetLevelNumber(x.name)).ThenBy(x => x.name).ToList();
        Repaint();
    }

    private static int GetLevelNumber(string levelName)
    {
        var match = System.Text.RegularExpressions.Regex.Match(levelName, @"(\d+)(?!.*\d)");
        return (match.Success && int.TryParse(match.Value, out int number)) ? number : int.MaxValue;
    }

    private static int GetDifficultyValue(string difficulty)
    {
        return int.TryParse(difficulty, out int val) ? val : 0;
    }

    private List<LevelData> GetFilteredLevels()
    {
        if (string.IsNullOrWhiteSpace(search)) return levels;
        return levels.Where(x => x.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || x.difficulty.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
    }

    private string GetMostCommonDifficulty() => difficultyCounts.Count == 0 ? "" : difficultyCounts.OrderByDescending(x => x.Value).First().Key;

    private Color GetDifficultyColor(string difficulty)
    {
        float t = Mathf.Clamp01(GetDifficultyValue(difficulty) / 10f);

        if (t <= 0.5f)
            return Color.Lerp(palette.lowDiff, palette.midDiff, t * 2f);
        else
            return Color.Lerp(palette.midDiff, palette.highDiff, (t - 0.5f) * 2f);
    }

    private void DrawBackground() => EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), palette.bgDark);

    private class LevelData
    {
        public string name;
        public string difficulty;
        public UnityEngine.Object asset;
    }
}

#endif

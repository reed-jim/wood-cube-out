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
    private const float CARD_HEIGHT = 80f;

    private DefaultAsset levelsFolder;
    private string search = "";
    private Vector2 scroll;

    private List<LevelData> levels = new List<LevelData>();
    private Dictionary<string, int> difficultyCounts = new Dictionary<string, int>();

    private GUIStyle titleStyle, subtitleStyle, cardTitleStyle, cardNumberStyle, tableHeaderStyle, tableRowStyle;

    // Modern Slate Palette
    private readonly Color bgDark = new Color(0.05f, 0.06f, 0.08f);
    private readonly Color bgCard = new Color(0.09f, 0.10f, 0.13f);
    private readonly Color bgHeader = new Color(0.14f, 0.16f, 0.20f);
    private readonly Color bgRowAlt = new Color(0.07f, 0.08f, 0.10f);
    private readonly Color accentPrimary = new Color(0.38f, 0.40f, 0.98f);
    private readonly Color textMain = new Color(0.96f, 0.97f, 0.98f);
    private readonly Color textMuted = new Color(0.55f, 0.58f, 0.65f);

    [MenuItem("Tools/Level Overview")]
    public static void ShowWindow()
    {
        var window = GetWindow<LevelOverviewWindow>("Level Overview");
        window.minSize = new Vector2(800, 500);
        window.Show();
    }

    private void OnEnable()
    {
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
    // HEADER & TOOLBAR
    // ------------------------------------------------------------

    private void DrawHeader()
    {
        GUILayout.Space(16);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);

        EditorGUILayout.BeginVertical();
        GUILayout.Label("LEVEL OVERVIEW", titleStyle);
        GUILayout.Label("A clear look at your level difficulty distribution", subtitleStyle);
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Export to CSV (Google Sheets)", GUILayout.Width(190), GUILayout.Height(28)))
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
        GUILayout.Label("Search", GUILayout.Width(45));
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

        DrawStatCard("TOTAL LEVELS", levels.Count.ToString(), accentPrimary);
        GUILayout.Space(10);

        string mostCommon = GetMostCommonDifficulty();
        DrawStatCard("MOST COMMON", string.IsNullOrEmpty(mostCommon) ? "-" : $"{mostCommon}", new Color(1.00f, 0.70f, 0.18f));
        GUILayout.Space(10);

        DrawStatCard("DIFFICULTIES", difficultyCounts.Count.ToString(), new Color(0.00f, 0.82f, 0.90f));

        GUILayout.Space(20);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(16);
    }

    private void DrawStatCard(string title, string value, Color accent)
    {
        Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(CARD_HEIGHT));
        rect.width = (position.width - 60f) / 3f;

        DrawRoundedRect(rect, bgCard);
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

        GUILayout.Label($"{difficulty}", GUILayout.Width(120));
        Rect barRect = GUILayoutUtility.GetRect(100, 16, GUILayout.ExpandWidth(true));
        float percentage = total > 0 ? (float)count / total : 0;

        EditorGUI.DrawRect(barRect, bgHeader);
        EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * percentage, barRect.height), GetDifficultyColor(difficulty));

        GUILayout.Space(10);
        GUILayout.Label($"{count} ({percentage:P0})", GUILayout.Width(90));

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
        EditorGUI.DrawRect(headerRect, bgHeader);

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
            EditorGUI.DrawRect(rowRect, bgRowAlt);

        GUI.Label(new Rect(rowRect.x + 20, rowRect.y, 40, rowRect.height), (index + 1).ToString(), subtitleStyle);

        if (GUI.Button(new Rect(rowRect.x + 60, rowRect.y, rowRect.width - 250, rowRect.height), level.name, tableRowStyle))
        {
            Selection.activeObject = level.asset;
            EditorGUIUtility.PingObject(level.asset);
        }

        Color color = GetDifficultyColor(level.difficulty);
        Rect pill = new Rect(rowRect.x + rowRect.width - 170, rowRect.y + 5, 130, 22);

        EditorGUI.DrawRect(pill, new Color(color.r * 0.20f, color.g * 0.20f, color.b * 0.20f, 1f));

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

    // ------------------------------------------------------------
    // COLORS & STYLES
    // ------------------------------------------------------------

    private Color GetDifficultyColor(string difficulty)
    {
        float t = Mathf.Clamp01(GetDifficultyValue(difficulty) / 10f);

        Color low = new Color(0.00f, 0.82f, 0.90f);   // Bright Cyan (0-3)
        Color mid = new Color(1.00f, 0.70f, 0.18f);   // Warm Amber (4-7)
        Color high = new Color(1.00f, 0.25f, 0.42f);  // Vivid Rose (8-10)

        if (t <= 0.5f)
            return Color.Lerp(low, mid, t * 2f);
        else
            return Color.Lerp(mid, high, (t - 0.5f) * 2f);
    }

    private void CreateStyles()
    {
        titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = textMain } };
        subtitleStyle = new GUIStyle(EditorStyles.label) { fontSize = 11, normal = { textColor = textMuted } };
        cardTitleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 10, fontStyle = FontStyle.Bold, normal = { textColor = textMuted } };
        cardNumberStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = textMain } };
        tableHeaderStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 10, normal = { textColor = textMuted } };
        tableRowStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 11, normal = { textColor = textMain }, hover = { textColor = accentPrimary } };
    }

    private void DrawBackground() => EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), bgDark);
    private void DrawRoundedRect(Rect rect, Color color) => EditorGUI.DrawRect(rect, color);

    private class LevelData
    {
        public string name;
        public string difficulty;
        public UnityEngine.Object asset;
    }
}

#endif

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AugmentTool : EditorWindow
{
    private string searchString = "";
    private AugmentQuality? qualityFilter = null;
    private SpaceObjectType? typeFilter = null;
    private Vector2 scrollPos;

    [SerializeField] private AugmentContainer augmentContainer;

    // Space-Themed Custom UI Styles Cache
    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle cardStyle;
    private GUIStyle countButtonStyle;
    private bool stylesInitialized = false;

    [MenuItem("Tools/Augment Dashboard &A")]
    public static void ShowWindow() => GetWindow<AugmentTool>("🌌 Augment Command Center").minSize = new Vector2(720, 450);

    private void InitStyles()
    {
        if (stylesInitialized) return;

        headerStyle = new GUIStyle(EditorStyles.boldLabel) 
        { 
            fontSize = 12, 
            richText = true, 
            normal = { textColor = new Color(0.85f, 0.92f, 1.0f) } 
        };
        
        subHeaderStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 10,
            richText = true,
            normal = { textColor = new Color(0.6f, 0.75f, 0.9f) }
        };

        cardStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 8),
            margin = new RectOffset(4, 4, 3, 3)
        };

        countButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
        {
            richText = true,
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter
        };

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        InitStyles();
        DrawSpaceBackground();
        DrawToolbar();

        if (augmentContainer == null)
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("⚠️ NO AUGMENT CONTAINER DETECTED.\nPlease assign your AugmentContainer asset above to initialize radar scanning.", MessageType.Warning);
            return;
        }

        DrawSpaceCountersBar();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        if (augmentContainer.augments != null)
        {
            var filteredList = augmentContainer.augments.Where(a => a != null &&
                (string.IsNullOrEmpty(searchString) || 
                 a.augmentName.Contains(searchString, System.StringComparison.OrdinalIgnoreCase) || 
                 a.name.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)) &&
                (!qualityFilter.HasValue || a.augmentQuality == qualityFilter.Value) &&
                (!typeFilter.HasValue || a.spaceObjectType == typeFilter.Value)
            ).ToList();

            if (filteredList.Count == 0)
            {
                EditorGUILayout.Space(20);
                EditorGUILayout.HelpBox("🛰️ No matching augments found in sector database.", MessageType.Info);
            }
            else
            {
                foreach (var aug in filteredList)
                {
                    DrawAugmentCard(aug);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSpaceBackground()
    {
        // Deep cosmic background tint for the entire window body
        Rect bgRect = new Rect(0, 0, position.width, position.height);
        EditorGUI.DrawRect(bgRect, new Color(0.12f, 0.14f, 0.18f, 1f));
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        augmentContainer = (AugmentContainer)EditorGUILayout.ObjectField(augmentContainer, typeof(AugmentContainer), false, GUILayout.Width(190));
        
        searchString = EditorGUILayout.TextField(searchString, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("", EditorStyles.toolbarSearchField)) { searchString = ""; GUI.FocusControl(null); }

        // Quality Filter Dropdown
        string qLabel = qualityFilter.HasValue ? qualityFilter.Value.ToString() : "All Qualities";
        if (EditorGUILayout.DropdownButton(new GUIContent($"Quality: {qLabel}"), FocusType.Passive, GUILayout.Width(130)))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("All Qualities"), !qualityFilter.HasValue, () => qualityFilter = null);
            foreach (AugmentQuality q in System.Enum.GetValues(typeof(AugmentQuality)))
                menu.AddItem(new GUIContent(q.ToString()), qualityFilter == q, () => qualityFilter = q);
            menu.ShowAsContext();
        }

        // SpaceObjectType Filter Dropdown
        string tLabel = typeFilter.HasValue ? typeFilter.Value.ToString() : "All Types";
        if (EditorGUILayout.DropdownButton(new GUIContent($"Type: {tLabel}"), FocusType.Passive, GUILayout.Width(115)))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("All Types"), !typeFilter.HasValue, () => typeFilter = null);
            foreach (SpaceObjectType t in System.Enum.GetValues(typeof(SpaceObjectType)))
                menu.AddItem(new GUIContent(t.ToString()), typeFilter == t, () => typeFilter = t);
            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSpaceCountersBar()
    {
        if (augmentContainer.augments == null || augmentContainer.augments.Count == 0) return;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(28));
        GUILayout.Label($"<b>⚡ TOTAL: {augmentContainer.augments.Count}</b>", headerStyle, GUILayout.Width(85));

        foreach (AugmentQuality q in System.Enum.GetValues(typeof(AugmentQuality)))
        {
            int count = augmentContainer.augments.Count(a => a != null && a.augmentQuality == q);
            if (count > 0)
            {
                Color c = GetQualityColor(q);
                string hexColor = ColorUtility.ToHtmlStringRGB(c);
                
                // Fixed: Pass countButtonStyle so richText = true is active and parses <color> correctly
                if (GUILayout.Button($"<color=#{hexColor}><b>{q}: {count}</b></color>", countButtonStyle))
                {
                    qualityFilter = (qualityFilter == q) ? null : q; 
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(3);
    }

    private void DrawAugmentCard(Augment aug)
    {
        Color originalBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.16f, 0.19f, 0.25f, 1f); // Holographic sci-fi card box tone

        EditorGUILayout.BeginHorizontal(cardStyle);
        {
            // Neon glowing quality indicator bar on the left edge
            Color qColor = GetQualityColor(aug.augmentQuality);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(5, 32, GUILayout.ExpandWidth(false)), qColor);

            EditorGUILayout.BeginVertical();
            {
                string displayName = string.IsNullOrEmpty(aug.augmentName) ? aug.name : aug.augmentName;
                EditorGUILayout.LabelField(displayName, headerStyle);

                string qHex = ColorUtility.ToHtmlStringRGB(qColor);
                EditorGUILayout.LabelField($"Quality: <color=#{qHex}><b>{aug.augmentQuality}</b></color>  |  Sector Type: <color=#00E5FF><b>{aug.spaceObjectType}</b></color>", subHeaderStyle);
            }
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Sci-fi Ping Button
            if (GUILayout.Button("📡 Ping", GUILayout.Width(60), GUILayout.Height(28)))
            {
                EditorGUIUtility.PingObject(aug);
                Selection.activeObject = aug;
            }
        }
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = originalBg;
    }

    private Color GetQualityColor(AugmentQuality q) => q switch
    {
        AugmentQuality.Common => new Color(0.65f, 0.65f, 0.65f), // Steel Gray
        AugmentQuality.Uncommon => new Color(0.15f, 0.9f, 0.4f),  // Holographic Green
        AugmentQuality.Rare => new Color(0.1f, 0.6f, 1.0f),       // Plasma Blue
        AugmentQuality.Epic => new Color(0.7f, 0.3f, 1.0f),       // Neon Purple
        AugmentQuality.Legendary => new Color(1.0f, 0.6f, 0.1f),  // Solar Amber / Gold
        AugmentQuality.Mythic => new Color(1.0f, 0.2f, 0.3f),     // Supernova Red
        _ => Color.white
    };
}
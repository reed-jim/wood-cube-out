#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor window for clearing sprite references from TileIconContainer categories
/// that have already been uploaded to GplayCDN.
///
/// Use this AFTER uploading sprites to CDN (via "Upload Tile Icons to CDN" window).
/// Clearing sprite refs reduces build size since assets are served from CDN at runtime.
///
/// Open via: Tools → CDN → Clear CDN Sprites
/// </summary>
public class TileIconCdnClearSpritesWindow : EditorWindow
{
    // ─── State ───────────────────────────────────────────────────────────────
    private TileIconContainer _container;
    private Vector2 _scrollPos;
    private bool _showOnlyUploaded = true;
    private bool _showOnlyWithSprites = false;
    private int _startIndex = 0;        // show/act on categories from this index onwards

    // Per-row selection
    private readonly HashSet<int> _selectedIndices = new HashSet<int>();

    // ─── Menu entry ──────────────────────────────────────────────────────────
    [MenuItem("Tools/CDN/Clear CDN Sprites")]
    public static void ShowWindow()
    {
        var window = GetWindow<TileIconCdnClearSpritesWindow>("CDN – Clear Sprites");
        window.minSize = new Vector2(600, 400);
    }

    // ─── GUI ─────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        DrawHeader();
        GUILayout.Space(6);

        _container = (TileIconContainer)EditorGUILayout.ObjectField(
            "TileIconContainer", _container, typeof(TileIconContainer), false);

        if (_container == null || _container.categoryIconSets == null)
        {
            EditorGUILayout.HelpBox("Assign a TileIconContainer to begin.", MessageType.Info);
            return;
        }

        GUILayout.Space(4);
        DrawFilterBar();
        GUILayout.Space(4);
        DrawSummaryBar();
        GUILayout.Space(6);
        DrawCategoryList();
        GUILayout.Space(8);
        DrawActionButtons();
    }

    // ─── Header ──────────────────────────────────────────────────────────────
    private void DrawHeader()
    {
        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            normal = { textColor = new Color(0.9f, 0.6f, 0.2f) }
        };
        GUILayout.Label("🗑  CDN Sprite Cleaner", style);
        EditorGUILayout.HelpBox(
            "Clears sprite references from categories already uploaded to GplayCDN.\n" +
            "Only categories with isOnServer = true AND iconCdnKeys filled are safe to clear.",
            MessageType.Info);
    }

    // ─── Filter bar ──────────────────────────────────────────────────────────
    private void DrawFilterBar()
    {
        // Start index
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Start Index:", GUILayout.Width(80));
        int newStart = EditorGUILayout.IntField(_startIndex, GUILayout.Width(50));
        if (newStart != _startIndex)
        {
            _startIndex = Mathf.Max(0, newStart);
            _selectedIndices.Clear();   // reset selection when range changes
        }
        GUILayout.Label("(hiển thị category từ index này trở đi)",
            EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        // Toggles
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Filter:", GUILayout.Width(45));
        _showOnlyUploaded    = GUILayout.Toggle(_showOnlyUploaded,    " Only uploaded (isOnServer)", GUILayout.Width(190));
        _showOnlyWithSprites = GUILayout.Toggle(_showOnlyWithSprites, " Only with sprites still assigned", GUILayout.Width(220));
        EditorGUILayout.EndHorizontal();
    }

    // ─── Summary bar ─────────────────────────────────────────────────────────
    private void DrawSummaryBar()
    {
        int total        = Mathf.Max(0, _container.categoryIconSets.Length - _startIndex);
        int uploaded     = CountWhere(s => s.isOnServer);
        int withSprites  = CountWhere(s => s.isOnServer && HasSprites(s));
        int safeToClear  = CountWhere(IsSafeToClear);

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label($"Showing from [{_startIndex}]: {total} cats", GUILayout.Width(155));
        GUILayout.Label($"Uploaded: {uploaded}",        GUILayout.Width(90));
        GUILayout.Label($"Has sprites: {withSprites}",  GUILayout.Width(105));
        GUILayout.Label($"Safe to clear: {safeToClear}", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
    }

    // ─── Category list ────────────────────────────────────────────────────────
    private void DrawCategoryList()
    {
        // Column headers
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Toggle(false, "", GUILayout.Width(18));   // checkbox placeholder
        GUILayout.Label("#",           EditorStyles.toolbarButton, GUILayout.Width(32));
        GUILayout.Label("Category",    EditorStyles.toolbarButton, GUILayout.Width(160));
        GUILayout.Label("isOnServer",  EditorStyles.toolbarButton, GUILayout.Width(80));
        GUILayout.Label("CDN Keys",    EditorStyles.toolbarButton, GUILayout.Width(75));
        GUILayout.Label("icons[]",     EditorStyles.toolbarButton, GUILayout.Width(70));
        GUILayout.Label("spriteOrder", EditorStyles.toolbarButton, GUILayout.Width(80));
        GUILayout.Label("Safe?",       EditorStyles.toolbarButton, GUILayout.Width(50));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));

        var sets = _container.categoryIconSets;
        for (int i = _startIndex; i < sets.Length; i++)
        {
            var s = sets[i];

            if (_showOnlyUploaded  && !s.isOnServer)   continue;
            if (_showOnlyWithSprites && !HasSprites(s)) continue;

            bool safe = IsSafeToClear(s);
            bool selected = _selectedIndices.Contains(i);

            // Row background
            Color rowBg = safe
                ? (HasSprites(s) ? new Color(0.2f, 0.4f, 0.2f, 0.25f) : new Color(0.15f, 0.15f, 0.15f, 0.1f))
                : new Color(0.4f, 0.1f, 0.1f, 0.2f);

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = rowBg;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.backgroundColor = prevBg;

            // Checkbox (only for safe-to-clear rows)
            GUI.enabled = safe && HasSprites(s);
            bool newSelected = GUILayout.Toggle(selected, "", GUILayout.Width(18));
            GUI.enabled = true;
            if (newSelected != selected)
            {
                if (newSelected) _selectedIndices.Add(i);
                else             _selectedIndices.Remove(i);
            }

            // Index
            GUILayout.Label(i.ToString(), GUILayout.Width(32));

            // Category name
            GUILayout.Label(s.itemCategory.ToString(), GUILayout.Width(160));

            // isOnServer
            DrawBoolLabel(s.isOnServer, width: 80);

            // CDN keys count
            int keyCount = (s.iconCdnKeys != null ? s.iconCdnKeys.Length : 0)
                         + (string.IsNullOrEmpty(s.spriteOrderCdnKey) ? 0 : 1);
            GUILayout.Label(keyCount > 0 ? $"✓ {keyCount}" : "✗ 0",
                keyCount > 0 ? EditorStyles.boldLabel : EditorStyles.label,
                GUILayout.Width(75));

            // icons[] status
            bool hasIcons = s.icons != null && s.icons.Length > 0;
            DrawStatusLabel(hasIcons, hasIcons ? $"{s.icons.Length} refs" : "cleared", 70);

            // spriteOrder status
            bool hasOrder = s.spriteOrder != null;
            DrawStatusLabel(hasOrder, hasOrder ? "ref" : "cleared", 80);

            // Safe indicator
            GUILayout.Label(safe ? "✓" : "✗",
                safe ? EditorStyles.boldLabel : EditorStyles.label,
                GUILayout.Width(50));

            // Per-row clear button
            GUI.enabled = safe && HasSprites(s);
            if (GUILayout.Button("Clear", GUILayout.Width(52)))
            {
                if (ConfirmClear($"Clear sprites for [{i}] {s.itemCategory}?"))
                {
                    ClearIconSet(s);
                    MarkDirtyAndSave();
                    _selectedIndices.Remove(i);
                }
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    // ─── Action buttons ───────────────────────────────────────────────────────
    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        // Select all safe-with-sprites (within range)
        if (GUILayout.Button("Select All Safe", GUILayout.Height(28)))
        {
            _selectedIndices.Clear();
            var sets = _container.categoryIconSets;
            for (int i = _startIndex; i < sets.Length; i++)
                if (IsSafeToClear(sets[i]) && HasSprites(sets[i]))
                    _selectedIndices.Add(i);
        }

        // Deselect all
        if (GUILayout.Button("Deselect All", GUILayout.Height(28)))
            _selectedIndices.Clear();

        GUILayout.FlexibleSpace();

        // Clear selected
        GUI.enabled = _selectedIndices.Count > 0;
        var btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal    = { textColor = Color.white }
        };
        var prevBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.8f, 0.4f, 0.1f);
        if (GUILayout.Button($"Clear Selected ({_selectedIndices.Count})", btnStyle, GUILayout.Height(30), GUILayout.Width(200)))
        {
            if (ConfirmClear($"Clear sprites for {_selectedIndices.Count} selected categories?"))
            {
                foreach (int idx in _selectedIndices)
                    ClearIconSet(_container.categoryIconSets[idx]);
                _selectedIndices.Clear();
                MarkDirtyAndSave();
            }
        }
        GUI.backgroundColor = prevBg;
        GUI.enabled = true;

        GUILayout.Space(6);

        // Clear ALL safe (within range)
        int safeClearable = CountWhere(s => IsSafeToClear(s) && HasSprites(s));
        GUI.enabled = safeClearable > 0;
        GUI.backgroundColor = new Color(0.7f, 0.1f, 0.1f);
        if (GUILayout.Button($"Clear ALL Safe ({safeClearable})", btnStyle, GUILayout.Height(30), GUILayout.Width(180)))
        {
            if (ConfirmClear($"Clear sprites for ALL {safeClearable} safe categories (from index {_startIndex})?"))
            {
                var sets = _container.categoryIconSets;
                for (int i = _startIndex; i < sets.Length; i++)
                    if (IsSafeToClear(sets[i]) && HasSprites(sets[i]))
                        ClearIconSet(sets[i]);
                _selectedIndices.Clear();
                MarkDirtyAndSave();
            }
        }
        GUI.backgroundColor = prevBg;
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// A category is safe to clear when:
    ///   1. isOnServer = true (uploaded to CDN)
    ///   2. iconCdnKeys[] is filled with the same count as icons[] (or there are no icons)
    ///   3. spriteOrderCdnKey is filled if spriteOrder was uploaded
    /// </summary>
    private static bool IsSafeToClear(CategoryIconSet s)
    {
        if (!s.isOnServer) return false;

        // Validate icon CDN keys
        bool iconsOk;
        if (s.icons == null || s.icons.Length == 0)
        {
            // Already cleared or no icons → safe
            iconsOk = true;
        }
        else
        {
            // Must have a CDN key for every icon
            iconsOk = s.iconCdnKeys != null && s.iconCdnKeys.Length == s.icons.Length
                      && System.Array.TrueForAll(s.iconCdnKeys, k => !string.IsNullOrEmpty(k));
        }

        // Validate spriteOrder CDN key
        bool orderOk = s.spriteOrder == null || !string.IsNullOrEmpty(s.spriteOrderCdnKey);

        return iconsOk && orderOk;
    }

    private static bool HasSprites(CategoryIconSet s)
        => (s.icons != null && s.icons.Length > 0) || s.spriteOrder != null;

    private static void ClearIconSet(CategoryIconSet s)
    {
        s.icons       = null;
        s.spriteOrder = null;
        Debug.Log($"[CDN Clear] Cleared sprites for: {s.itemCategory}");
    }

    private void MarkDirtyAndSave()
    {
        EditorUtility.SetDirty(_container);
        AssetDatabase.SaveAssets();
        Debug.Log("[CDN Clear] Saved TileIconContainer.");
        Repaint();
    }

    private int CountWhere(System.Func<CategoryIconSet, bool> predicate)
    {
        int count = 0;
        var sets = _container.categoryIconSets;
        for (int i = _startIndex; i < sets.Length; i++)
            if (predicate(sets[i])) count++;
        return count;
    }

    private static bool ConfirmClear(string message)
        => EditorUtility.DisplayDialog("Confirm Clear Sprites", message, "Yes, Clear", "Cancel");

    private static void DrawBoolLabel(bool value, int width)
    {
        var style = value ? EditorStyles.boldLabel : EditorStyles.label;
        GUILayout.Label(value ? "✓ Yes" : "✗ No", style, GUILayout.Width(width));
    }

    private static void DrawStatusLabel(bool hasValue, string text, int width)
    {
        var style = hasValue ? EditorStyles.boldLabel : EditorStyles.label;
        var color = hasValue
            ? new Color(0.3f, 0.9f, 0.3f)   // green = has ref
            : new Color(0.6f, 0.6f, 0.6f);   // grey  = cleared
        var prev = GUI.contentColor;
        GUI.contentColor = color;
        GUILayout.Label(text, style, GUILayout.Width(width));
        GUI.contentColor = prev;
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace NamPhuThuy
{
    public class LevelAddressablesManagerWindow : EditorWindow
    {
        [Serializable]
        private class Item
        {
            public string guid;
            public string assetPath;
            public string fileName;
            public string currentAddress;
            public string newAddress;
            public bool hasEntry;
            public bool isValid;
            public int parsedLevelId;
            public bool isSub;
        }

        private DefaultAsset _folderAsset;
        private string _folderPath = "Assets/Prefabs/Levels";
        private string _groupName = "Levels";
        private bool _renamePrefabFiles = false;

        private readonly List<Item> _items = new List<Item>();
        private Vector2 _scroll;
        private bool _hasDuplicates;

        private static readonly Regex LevelRegex =
            new Regex(@"\bLevel\b\s*[-_ ]*\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #region Callbacks

        [MenuItem("Tools/TrinhNam/Level Addressables Manager")]
        public static void ShowWindow()
        {
            var win = GetWindow<LevelAddressablesManagerWindow>("Level Addressables");
            win.minSize = new Vector2(640, 400);
            win.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Level Addressables Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Rule:\n- Address = \"Level <id>\"\n- If filename contains \"SUB\" (anywhere, case-insensitive), append \" sub\".", MessageType.Info);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginVertical("box");
            {
                var newFolder = (DefaultAsset)EditorGUILayout.ObjectField("Level Prefabs Folder", _folderAsset, typeof(DefaultAsset), false);
                if (newFolder != _folderAsset)
                {
                    _folderAsset = newFolder;
                    _folderPath = _folderAsset != null ? AssetDatabase.GetAssetPath(_folderAsset) : _folderPath;
                }

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField("Folder Path", _folderPath);
                }

                _groupName = EditorGUILayout.TextField("Addressables Group", _groupName);
                _renamePrefabFiles = EditorGUILayout.Toggle("Also Rename Prefab Files", _renamePrefabFiles);

                EditorGUILayout.Space(2);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Scan Folder", GUILayout.Height(24)))
                    ScanFolder();

                using (new EditorGUI.DisabledScope(_items.Count == 0))
                {
                    if (GUILayout.Button("Apply Addresses", GUILayout.Height(24)))
                        ApplyAddresses();
                }

                using (new EditorGUI.DisabledScope(_items.Count == 0))
                {
                    if (GUILayout.Button("Validate", GUILayout.Height(24)))
                        ValidatePreview();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);
            DrawPreview();
        }

        #endregion

        private void ScanFolder()
        {
            _items.Clear();
            _hasDuplicates = false;

            if (string.IsNullOrEmpty(_folderPath) || !AssetDatabase.IsValidFolder(_folderPath))
            {
                ShowNotification(new GUIContent("Folder is invalid or missing."));
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { _folderPath });
            var assets = guids
                .Select(g =>
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    var name = System.IO.Path.GetFileNameWithoutExtension(path);
                    return new { guid = g, path, name, sortKey = ExtractNumber(name) };
                })
                .OrderBy(a => a.sortKey)
                .ThenBy(a => a.name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                ShowNotification(new GUIContent("Addressables settings not found. Please install/initialize Addressables."));
                return;
            }

            foreach (var a in assets)
            {
                var entry = settings.FindAssetEntry(a.guid);
                var parsed = TryBuildAddressFromFileName(a.name, out string address, out int levelId, out bool isSub);

                _items.Add(new Item
                {
                    guid = a.guid,
                    assetPath = a.path,
                    fileName = a.name,
                    currentAddress = entry != null ? entry.address : string.Empty,
                    newAddress = parsed ? address : string.Empty,
                    hasEntry = entry != null,
                    isValid = parsed,
                    parsedLevelId = levelId,
                    isSub = isSub
                });
            }

            ValidatePreview();
            Repaint();
        }

        private void ApplyAddresses()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                ShowNotification(new GUIContent("Addressables settings not found."));
                return;
            }

            var group = settings.FindGroup(_groupName);
            if (group == null)
            {
                group = settings.CreateGroup(_groupName, false, false, false, null);
                if (group.GetSchema<BundledAssetGroupSchema>() == null) group.AddSchema<BundledAssetGroupSchema>();
                if (group.GetSchema<ContentUpdateGroupSchema>() == null) group.AddSchema<ContentUpdateGroupSchema>();
            }

            settings.AddLabel("Level", false);

            var addressSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in _items.Where(i => i.isValid))
            {
                if (addressSet.Contains(item.newAddress))
                {
                    Debug.LogError($"Duplicate address detected: {item.newAddress}. Aborting.");
                    return;
                }
                addressSet.Add(item.newAddress);
            }

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var item in _items)
                {
                    if (!item.isValid)
                    {
                        Debug.LogWarning($"Skipped (unparsable): {item.assetPath}");
                        continue;
                    }

                    var entry = settings.FindAssetEntry(item.guid);
                    if (entry == null || entry.parentGroup != group)
                        entry = settings.CreateOrMoveEntry(item.guid, group, false, false);

                    if (entry.address != item.newAddress)
                        entry.SetAddress(item.newAddress);

                    if (!entry.labels.Contains("Level"))
                        entry.SetLabel("Level", true, true);

                    if (_renamePrefabFiles)
                    {
                        var desiredName = item.newAddress + ".prefab";
                        var currentName = System.IO.Path.GetFileName(item.assetPath);
                        if (!string.Equals(desiredName, currentName, StringComparison.OrdinalIgnoreCase))
                        {
                            var res = AssetDatabase.RenameAsset(item.assetPath, item.newAddress);
                            if (!string.IsNullOrEmpty(res))
                                Debug.LogError($"Rename failed for {item.assetPath}: {res}");
                            else
                                item.assetPath = AssetDatabase.GUIDToAssetPath(item.guid);
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
                EditorUtility.SetDirty(group);
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"Applied addresses for {_items.Count(i => i.isValid)} valid prefabs to group '{_groupName}'.");
            ScanFolder();
        }

        private void ValidatePreview()
        {
            _hasDuplicates = false;
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var i in _items.Where(i => i.isValid))
            {
                if (!set.Add(i.newAddress))
                {
                    _hasDuplicates = true;
                    break;
                }
            }
        }

        private void DrawPreview()
        {
            if (_items.Count == 0)
            {
                EditorGUILayout.HelpBox("No prefabs scanned. Choose a folder and click Scan.", MessageType.Info);
                return;
            }

            if (_hasDuplicates)
                EditorGUILayout.HelpBox("Duplicate new addresses detected.", MessageType.Error);

            int valid = _items.Count(i => i.isValid);
            int invalid = _items.Count - valid;
            EditorGUILayout.LabelField($"Found {_items.Count} prefabs. Valid: {valid}, Skipped: {invalid}. Preview (first 100):");
            EditorGUILayout.Space(2);

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scroll.scrollPosition;

                int shown = 0;
                foreach (var item in _items)
                {
                    if (shown++ >= 100) break;

                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUILayout.LabelField(item.fileName, EditorStyles.boldLabel);
                        EditorGUILayout.LabelField("Path", item.assetPath);
                        EditorGUILayout.LabelField("Current", string.IsNullOrEmpty(item.currentAddress) ? "(none)" : item.currentAddress);

                        if (item.isValid)
                        {
                            using (new EditorGUI.DisabledScope(true))
                            {
                                EditorGUILayout.TextField("New", item.newAddress);
                            }
                            EditorGUILayout.LabelField("Status", "Will update", EditorStyles.miniBoldLabel);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("New", "(unparsable)");
                            EditorGUILayout.LabelField("Status", "Skipped", EditorStyles.miniLabel);
                        }
                    }
                }
            }
        }

        private static int ExtractNumber(string name)
        {
            var m = Regex.Match(name, @"\d+");
            if (m.Success && int.TryParse(m.Value, out int v)) return v;
            return int.MaxValue;
        }

        private static bool TryBuildAddressFromFileName(string fileNameWithoutExt, out string address, out int levelId, out bool isSub)
        {
            // Parse "Level <id>" where spaces/dashes/underscores are tolerated between "Level" and digits.
            var m = LevelRegex.Match(fileNameWithoutExt);
            if (!m.Success || !int.TryParse(m.Groups[1].Value, out levelId))
            {
                address = string.Empty;
                isSub = false;
                levelId = 0; 
                return false;
            }

            isSub = fileNameWithoutExt.IndexOf("SUB", StringComparison.OrdinalIgnoreCase) >= 0;
            address = $"Level {levelId}" + (isSub ? " sub" : "");
            return true;
        }
    }
}
#endif

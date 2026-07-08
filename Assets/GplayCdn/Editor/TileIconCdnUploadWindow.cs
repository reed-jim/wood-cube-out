#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using GplayCdn;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor window for batch uploading TileIconContainer sprites to GplayCDN.
/// Uploads icons[] and spriteOrder for each CategoryIconSet from index 21 onwards.
/// After upload, stores CDN keys and optionally clears sprite references to reduce build size.
/// </summary>
public class TileIconCdnUploadWindow : EditorWindow
{
    private TileIconContainer _targetContainer;
    private int _startIndex = 21;
    private string _excludeIndicesStr = "";
    private bool _isUploading;
    private int _currentUploadIndex;
    private int _totalToUpload;
    private string _statusMessage = "";
    private Vector2 _scrollPos;

    [MenuItem("Tools/CDN/Upload Tile Icons to CDN")]
    public static void ShowWindow()
    {
        GetWindow<TileIconCdnUploadWindow>("CDN Upload Tile Icons");
    }

    private void OnGUI()
    {
        GUILayout.Label("Upload Category Icons to GplayCDN", EditorStyles.boldLabel);
        GUILayout.Space(5);

        _targetContainer = (TileIconContainer)EditorGUILayout.ObjectField(
            "TileIconContainer", _targetContainer, typeof(TileIconContainer), false);

        _startIndex = EditorGUILayout.IntField("Start Index (0-based)", _startIndex);
        _excludeIndicesStr = EditorGUILayout.TextField("Exclude Indices (e.g. 23,24)", _excludeIndicesStr);

        EditorGUILayout.HelpBox(
            "Tip: To clear sprite references after upload, use\n" +
            "Tools → CDN → Clear CDN Sprites",
            MessageType.None);

        GUILayout.Space(10);

        if (_targetContainer != null && _targetContainer.categoryIconSets != null)
        {
            int total = _targetContainer.categoryIconSets.Length;
            var excluded = GetExcludeIndices();
            
            int toUpload = 0;
            for (int i = _startIndex; i < total; i++)
            {
                if (!excluded.Contains(i))
                {
                    toUpload++;
                }
            }

            EditorGUILayout.HelpBox(
                $"Total categories: {total}\n" +
                $"Categories to upload (from index {_startIndex}, excluding {excluded.Count} items): {toUpload}",
                MessageType.Info);

            // Show preview of what will be uploaded
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MaxHeight(300));
            for (int i = _startIndex; i < total; i++)
            {
                var set = _targetContainer.categoryIconSets[i];
                int iconCount = set.icons != null ? set.icons.Length : 0;
                bool hasSpriteOrder = set.spriteOrder != null;
                
                bool isExcluded = excluded.Contains(i);
                string status;
                if (isExcluded)
                {
                    status = "🚫 Excluded";
                }
                else
                {
                    status = set.isOnServer ? "✓ On Server" : "✗ Not uploaded";
                }

                EditorGUILayout.BeginHorizontal();
                if (isExcluded)
                {
                    GUI.enabled = false;
                }
                GUILayout.Label($"[{i}] {set.itemCategory}", GUILayout.Width(200));
                GUILayout.Label($"Icons: {iconCount}", GUILayout.Width(80));
                GUILayout.Label($"Order: {(hasSpriteOrder ? "Yes" : "No")}", GUILayout.Width(80));
                GUILayout.Label(status, (set.isOnServer && !isExcluded) ? EditorStyles.boldLabel : EditorStyles.label);
                if (isExcluded)
                {
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        GUILayout.Space(10);

        // Upload progress
        if (_isUploading)
        {
            EditorGUI.ProgressBar(
                EditorGUILayout.GetControlRect(GUILayout.Height(20)),
                (float)_currentUploadIndex / _totalToUpload,
                $"Uploading {_currentUploadIndex}/{_totalToUpload}...");

            GUILayout.Label(_statusMessage, EditorStyles.miniLabel);

            if (GUILayout.Button("Cancel"))
            {
                _isUploading = false;
            }
        }
        else
        {
            GUI.enabled = _targetContainer != null && !_isUploading;

            if (GUILayout.Button("Upload All (from index " + _startIndex + ")", GUILayout.Height(35)))
            {
                StartBatchUpload();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Upload Only NOT uploaded", GUILayout.Height(30)))
            {
                StartBatchUpload(skipAlreadyUploaded: true);
            }

            GUI.enabled = true;
        }

        GUILayout.Space(5);
        if (!string.IsNullOrEmpty(_statusMessage) && !_isUploading)
        {
            EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
        }
    }

    private HashSet<int> GetExcludeIndices()
    {
        var result = new HashSet<int>();
        if (string.IsNullOrEmpty(_excludeIndicesStr))
            return result;

        string[] parts = _excludeIndicesStr.Split(',');
        foreach (var part in parts)
        {
            if (int.TryParse(part.Trim(), out int idx))
            {
                result.Add(idx);
            }
        }
        return result;
    }

    private void StartBatchUpload(bool skipAlreadyUploaded = false)
    {
        if (_targetContainer == null || _targetContainer.categoryIconSets == null)
        {
            _statusMessage = "No TileIconContainer assigned!";
            return;
        }

        _isUploading = true;
        _currentUploadIndex = 0;

        // Collect all sprites to upload
        var uploadTasks = new List<UploadTask>();
        var excluded = GetExcludeIndices();

        for (int i = _startIndex; i < _targetContainer.categoryIconSets.Length; i++)
        {
            if (excluded.Contains(i))
                continue;

            var set = _targetContainer.categoryIconSets[i];

            if (skipAlreadyUploaded && set.isOnServer)
                continue;

            uploadTasks.Add(new UploadTask { categoryIndex = i, iconSet = set });
        }

        _totalToUpload = uploadTasks.Count;

        if (_totalToUpload == 0)
        {
            _isUploading = false;
            _statusMessage = "Nothing to upload!";
            return;
        }

        UploadNextCategory(uploadTasks, 0);
    }

    private void UploadNextCategory(List<UploadTask> tasks, int taskIndex)
    {
        if (!_isUploading || taskIndex >= tasks.Count)
        {
            _isUploading = false;
            _statusMessage = $"Upload complete! {taskIndex} categories processed.";
            EditorUtility.SetDirty(_targetContainer);
            AssetDatabase.SaveAssets();
            Repaint();
            return;
        }

        var task = tasks[taskIndex];
        _currentUploadIndex = taskIndex + 1;
        _statusMessage = $"Uploading category: {task.iconSet.itemCategory}...";
        Repaint();

        UploadCategoryIconSet(task.iconSet, () =>
        {
            task.iconSet.isOnServer = true;
            EditorUtility.SetDirty(_targetContainer);

            // Continue to next
            UploadNextCategory(tasks, taskIndex + 1);
        });
    }

    private void UploadCategoryIconSet(CategoryIconSet iconSet, System.Action onComplete)
    {
        // Collect all sprites to upload for this category
        var sprites = new List<Sprite>();
        var spriteTypes = new List<SpriteType>(); // track which sprite goes where

        // Add icons
        if (iconSet.icons != null)
        {
            for (int i = 0; i < iconSet.icons.Length; i++)
            {
                if (iconSet.icons[i] != null)
                {
                    sprites.Add(iconSet.icons[i]);
                    spriteTypes.Add(new SpriteType { isIcon = true, iconIndex = i });
                }
            }
        }

        // Add spriteOrder
        if (iconSet.spriteOrder != null)
        {
            sprites.Add(iconSet.spriteOrder);
            spriteTypes.Add(new SpriteType { isIcon = false });
        }

        if (sprites.Count == 0)
        {
            Debug.LogWarning($"[CdnUpload] No sprites to upload for category: {iconSet.itemCategory}");
            onComplete?.Invoke();
            return;
        }

        // Initialize CDN key arrays
        if (iconSet.icons != null)
        {
            iconSet.iconCdnKeys = new string[iconSet.icons.Length];
        }

        CdnUploader.UploadSprites(
            sprites.ToArray(),
            onEachComplete: (index, fileId) =>
            {
                var type = spriteTypes[index];
                if (type.isIcon)
                {
                    iconSet.iconCdnKeys[type.iconIndex] = fileId;
                    _statusMessage = $"Uploaded icon [{type.iconIndex}] of {iconSet.itemCategory}: {fileId}";
                }
                else
                {
                    iconSet.spriteOrderCdnKey = fileId;
                    _statusMessage = $"Uploaded spriteOrder of {iconSet.itemCategory}: {fileId}";
                }
                Repaint();
            },
            onError: (error) =>
            {
                Debug.LogError($"[CdnUpload] Error: {error}");
            },
            onAllComplete: () =>
            {
                Debug.Log($"[CdnUpload] Category {iconSet.itemCategory} upload complete!" +
                          " Use 'Tools → CDN → Clear CDN Sprites' to clear sprite refs.");

                onComplete?.Invoke();
            });
    }

    private class UploadTask
    {
        public int categoryIndex;
        public CategoryIconSet iconSet;
    }

    private struct SpriteType
    {
        public bool isIcon;
        public int iconIndex;
    }
}
#endif

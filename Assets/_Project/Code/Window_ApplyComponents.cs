#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NamPhuThuy.Common
{
    public class Window_ApplyComponents : EditorWindow
    {
        #region Private Fields
        [SerializeField] private GameSetting gameSetting;

        private Vector2 _scrollPos;
        private GUIStyle _centeredLabelStyle;
        private GUIStyle _centeredButtonStyle;
        #endregion

        #region Menu Item
        [MenuItem("NamPhuThuy/Common/Apply GameSetting To Selected BasePieces")]
        private static void OpenWindow()
        {
            var window = GetWindow<Window_ApplyComponents>("Apply GameSetting");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }
        #endregion

        #region Unity Callbacks

        private void OnGUI()
        {
            InitializeStyles();

            float padding = 20f;
            Rect areaRect = new Rect(
                padding,
                padding,
                position.width - 2 * padding,
                position.height - 2 * padding
            );

            GUILayout.BeginArea(areaRect);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawHeader();
            GUILayout.Space(10);
            DrawContent();
            GUILayout.Space(10);
            DrawButtons();

            EditorGUILayout.EndScrollView();

            GUILayout.EndArea();
        }
        #endregion

        #region Private Methods
        private void InitializeStyles()
        {
            if (_centeredLabelStyle == null)
            {
                _centeredLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14
                };
            }

            if (_centeredButtonStyle == null)
            {
                _centeredButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 13,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        private void DrawHeader()
        {
            GUILayout.Label("Apply GameSetting", _centeredLabelStyle);
            EditorGUILayout.HelpBox(
                "Apply the selected GameSetting to all BasePiece components\n" +
                "in the children of the currently selected GameObject(s).",
                MessageType.Info
            );
        }

        private void DrawContent()
        {
            EditorGUILayout.LabelField("GameSetting Asset", EditorStyles.boldLabel);

            gameSetting = (GameSetting)EditorGUILayout.ObjectField(
                "GameSetting",
                gameSetting,
                typeof(GameSetting),
                false
            );

            GUILayout.Space(8);

            int selectedCount = Selection.gameObjects.Length;
            EditorGUILayout.LabelField(
                "Selection Info",
                $"{selectedCount} root object(s) selected in Hierarchy."
            );
        }

        private void DrawButtons()
        {
            GUILayout.BeginHorizontal();

            GUI.enabled = gameSetting != null && Selection.gameObjects.Length > 0;

            if (GUILayout.Button(
                    $"Apply to {Selection.gameObjects.Length} selected root object(s)",
                    _centeredButtonStyle,
                    GUILayout.Height(30)))
            {
                ApplyToSelection();
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        private void ApplyToSelection()
        {
            if (gameSetting == null)
            {
                Debug.LogError("GameSetting is null. Please assign a GameSetting asset.");
                return;
            }

            var selectedObjects = Selection.gameObjects;
            int totalPieces = 0;

            Undo.RecordObjects(selectedObjects, "Apply GameSetting To BasePieces");

            foreach (var go in selectedObjects)
            {
                if (go == null) continue;

                var basePieces = go.GetComponentsInChildren<BasePiece>(true);
                foreach (var piece in basePieces)
                {
                    if (piece == null) continue;

                    Undo.RecordObject(piece, "Set GameSetting on BasePiece");
                    var so = new SerializedObject(piece);
                    var prop = so.FindProperty("gameSetting");
                    if (prop != null)
                    {
                        prop.objectReferenceValue = gameSetting;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(piece);
                        totalPieces++;
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find `gameSetting` field on {piece.name}", piece);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Applied GameSetting `{gameSetting.name}` to {totalPieces} BasePiece component(s) in selection.");
        }
        #endregion
    }
}
#endif

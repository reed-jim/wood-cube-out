// Assets/Editor/PictureDatasEditor.cs
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NamPhuThuy.Data
{
#if UNITY_EDITOR
    [CustomEditor(typeof(PictureDatas))]
    public class PictureDatasEditor : Editor
    {
        // Editor-only field for folder selection
        SerializedProperty allPictureDatasProp;
        DefaultAsset mainImageSpriteFolder;
        DefaultAsset subImageSpriteFolder;

        int indexOffset = 0;
        static readonly Regex trailingNumber = new Regex(@"(\d+)$", RegexOptions.Compiled);

        void OnEnable()
        {
            allPictureDatasProp = serializedObject.FindProperty("allPictureDatas");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Auto Assign Tools", EditorStyles.boldLabel);

            mainImageSpriteFolder = (DefaultAsset)EditorGUILayout.ObjectField("Main Image Folder", mainImageSpriteFolder, typeof(DefaultAsset), false);
            subImageSpriteFolder = (DefaultAsset)EditorGUILayout.ObjectField("Sub Image Folder", subImageSpriteFolder, typeof(DefaultAsset), false);

            indexOffset = EditorGUILayout.IntField(new GUIContent("Index Offset",
                "0: suffix 25 -> element index 25\n-1: suffix 25 -> element index 24\n+1: suffix 25 -> element index 26"), indexOffset);

            using (new EditorGUI.DisabledScope(allPictureDatasProp == null || allPictureDatasProp.arraySize == 0))
            {
                if (GUILayout.Button("Auto-assign Main n Sub Images"))
                {
                    string mainImagefolderPath = mainImageSpriteFolder != null ? AssetDatabase.GetAssetPath(mainImageSpriteFolder) : "Assets";
                    string subImagefolderPath = subImageSpriteFolder != null ? AssetDatabase.GetAssetPath(subImageSpriteFolder) : "Assets";


                    AutoAssignMainImages((PictureDatas)target, mainImagefolderPath, indexOffset);
                    AutoAssignSubImages((PictureDatas)target, subImagefolderPath, indexOffset);
                }

                if (GUILayout.Button("CheckNullReference"))
                {
                    CheckNullReference((PictureDatas)target);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        static void AutoAssignMainImages(PictureDatas data, string folderPath, int indexOffset)
        {
            if (data == null || data.allPictureDatas == null || data.allPictureDatas.Count == 0)
            {
                Debug.LogWarning("[PictureDatas] No entries to assign.");
                return;
            }

            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("[PictureDatas] Invalid folder path.");
                return;
            }

            // Map: suffix -> Sprite
            var suffixToSprite = new Dictionary<int, Sprite>();
            string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

            foreach (var guid in texGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in assets)
                {
                    if (a is Sprite s)
                    {
                        var m = trailingNumber.Match(s.name);
                        if (m.Success && int.TryParse(m.Groups[1].Value, out int suffix))
                        {
                            suffixToSprite[suffix] = s; // last one wins
                        }
                    }
                }
            }

            Undo.RecordObject(data, "Auto-assign Main Images");

            int assigned = 0, notFound = 0;
            for (int i = 0; i < data.allPictureDatas.Count; i++)
            {
                int expectedSuffix = i - indexOffset;
                if (expectedSuffix < 0) continue;

                if (suffixToSprite.TryGetValue(expectedSuffix, out var sprite))
                {
                    data.allPictureDatas[i].mainImage = sprite;
                    assigned++;
                }
                else
                {
                    notFound++;
                }
            }

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PictureDatas] Assigned {assigned} sprites from folder '{folderPath}'. Not found: {notFound}");
        }

        static void AutoAssignSubImages(PictureDatas data, string folderPath, int indexOffset)
        {
            if (data == null || data.allPictureDatas == null || data.allPictureDatas.Count == 0)
            {
                Debug.LogWarning("[PictureDatas] No entries to assign.");
                return;
            }

            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("[PictureDatas] Invalid folder path.");
                return;
            }

            // Map: suffix -> Sprite
            var suffixToSprite = new Dictionary<int, Sprite>();
            string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

            foreach (var guid in texGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in assets)
                {
                    if (a is Sprite s)
                    {
                        var m = trailingNumber.Match(s.name);
                        if (m.Success && int.TryParse(m.Groups[1].Value, out int suffix))
                        {
                            suffixToSprite[suffix] = s; // last one wins
                        }
                    }
                }
            }

            Undo.RecordObject(data, "Auto-assign Main Images");

            int assigned = 0, notFound = 0;
            for (int i = 0; i < data.allPictureDatas.Count; i++)
            {
                int expectedSuffix = i - indexOffset;
                if (expectedSuffix < 0) continue;

                if (suffixToSprite.TryGetValue(expectedSuffix, out var sprite))
                {
                    // data.allPictureDatas[i].subImage = sprite;
                    data.allPictureDatas[i].picId = i;
                    assigned++;
                }
                else
                {
                    notFound++;
                }
            }

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PictureDatas] Assigned {assigned} sprites from folder '{folderPath}'. Not found: {notFound}");
        }

        static void CheckNullReference(PictureDatas data)
        {
            for (int i = 0; i < data.allPictureDatas.Count; i++)
            {
                Debug.Log("CheckNullReference " + data.allPictureDatas[i].mainImage);
            }
        }
    }

#endif
}

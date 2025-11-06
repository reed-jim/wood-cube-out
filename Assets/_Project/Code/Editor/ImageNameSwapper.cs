using System.IO;
using NamPhuThuy.Data;
using UnityEngine;
using UnityEditor;

namespace NamPhuThuy.EditorTools
{
    public class ImageNameSwapper : EditorWindow
    {
        private Texture2D texA;
        private Texture2D texB;
        
        // Fields for swapping inside ScriptableObject
        private PictureDatas pictureDatas;
        private int indexA;
        private int indexB;

        [MenuItem("Tools/TrinhNam/Swap Texture Names")]
        public static void ShowWindow()
        {
            GetWindow<ImageNameSwapper>("Swap Texture Names");
        }

        #region Callbacks

        private void OnGUI()
        {
            GUILayout.Label("Swap the names of 2 Textures", EditorStyles.boldLabel);

            texA = (Texture2D)EditorGUILayout.ObjectField("Texture A", texA, typeof(Texture2D), false);
            texB = (Texture2D)EditorGUILayout.ObjectField("Texture B", texB, typeof(Texture2D), false);

            EditorGUILayout.Space();

            // 🔘 Button to auto-fill from selection (accepts Texture2D or Sprite)
            if (GUILayout.Button("Use Selected (Textures or Sprites)"))
            {
                AssignSelectedTextures();
            }

            if (GUILayout.Button("Clear Fields"))
            {
                texA = null;
                texB = null;
            }
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Swap Names"))
            {
                if (texA == null || texB == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please assign both textures!", "OK");
                    return;
                }

                SwapTextureNames(texA, texB);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            // =============================
            // ScriptableObject Section
            // =============================
            GUILayout.Label("Swap Elements in PictureDatas", EditorStyles.boldLabel);

            pictureDatas = (PictureDatas)EditorGUILayout.ObjectField("PictureDatas SO", pictureDatas, typeof(PictureDatas), false);
            indexA = EditorGUILayout.IntField("Index A", indexA);
            indexB = EditorGUILayout.IntField("Index B", indexB);

            if (GUILayout.Button("Swap Elements"))
            {
                if (pictureDatas == null)
                {
                    EditorUtility.DisplayDialog("Error", "Assign a PictureDatas ScriptableObject!", "OK");
                    return;
                }

                SwapPictureDatasElements(pictureDatas, indexA, indexB);
            }
        }

        #endregion

        #region Functionalities

        private void SwapTextureNames(Texture2D a, Texture2D b)
        {
            string pathA = AssetDatabase.GetAssetPath(a);
            string pathB = AssetDatabase.GetAssetPath(b);

            if (string.IsNullOrEmpty(pathA) || string.IsNullOrEmpty(pathB))
            {
                EditorUtility.DisplayDialog("Error", "Invalid asset paths.", "OK");
                return;
            }

            if (pathA == pathB)
            {
                EditorUtility.DisplayDialog("Error", "You selected the same texture twice.", "OK");
                return;
            }

            string nameA = Path.GetFileNameWithoutExtension(pathA);
            string nameB = Path.GetFileNameWithoutExtension(pathB);

            string dirA = Path.GetDirectoryName(pathA);
            string dirB = Path.GetDirectoryName(pathB);

            string extA = Path.GetExtension(pathA);
            string extB = Path.GetExtension(pathB);

            // Temporary name to avoid conflict in dirA
            string tempName = nameA + "_temp_swap";
            string tempPath = Path.Combine(dirA, tempName + extA);

            try
            {
                // If a leftover temp exists, clean it up first
                if (AssetDatabase.LoadAssetAtPath<Object>(tempPath) != null)
                {
                    AssetDatabase.DeleteAsset(tempPath);
                }

                // Rename sequence:
                // A -> temp (in its own folder)
                var err1 = AssetDatabase.RenameAsset(pathA, tempName);
                if (!string.IsNullOrEmpty(err1)) throw new System.Exception(err1);

                // B -> A's old name (but in B's folder)
                var err2 = AssetDatabase.RenameAsset(pathB, nameA);
                if (!string.IsNullOrEmpty(err2)) throw new System.Exception(err2);

                // temp (A) -> B's old name (in A's folder)
                var err3 = AssetDatabase.RenameAsset(tempPath, nameB);
                if (!string.IsNullOrEmpty(err3)) throw new System.Exception(err3);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Success", $"Swapped '{nameA}' and '{nameB}'", "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", "Failed to swap: " + ex.Message, "OK");
            }
        }

        /// <summary>
        /// Fills texA/texB from current selection.
        /// Accepts either Texture2D or Sprite assets.
        /// If a Sprite is selected, we use sprite.texture (and its image asset path).
        /// </summary>
        private void AssignSelectedTextures()
        {
            // Only assets from Project view (no scene objects)
            Object[] selection = Selection.GetFiltered<Object>(SelectionMode.Assets);

            if (selection.Length != 2)
            {
                EditorUtility.DisplayDialog("Invalid Selection", "Please select exactly 2 assets (Texture2D or Sprite) in the Project window.", "OK");
                return;
            }

            Texture2D a = ToTexture(selection[0]);
            Texture2D b = ToTexture(selection[1]);

            if (a == null || b == null)
            {
                EditorUtility.DisplayDialog("Error", "Both selected assets must be Texture2D or Sprite (not atlases or non-image assets).", "OK");
                texA = null;
                texB = null;
                return;
            }

            // Prevent choosing two sprites from the same texture file
            string pathA = AssetDatabase.GetAssetPath(a);
            string pathB = AssetDatabase.GetAssetPath(b);
            if (pathA == pathB)
            {
                EditorUtility.DisplayDialog("Error",
                    "The two selections point to the SAME image file (e.g., two sprites from one texture). Pick two different textures.",
                    "OK");
                texA = null;
                texB = null;
                return;
            }

            texA = a;
            texB = b;
        }

        private Texture2D ToTexture(Object obj)
        {
            if (obj is Texture2D t) return t;
            if (obj is Sprite s)
            {
                // For sprites, resolve their source texture asset
                return s.texture;
            }
            return null;
        }

        #endregion

        #region SO Functions

        private void SwapPictureDatasElements(PictureDatas so, int a, int b)
        {
            if (so.allPictureDatas == null || so.allPictureDatas.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "allPictureDatas is empty!", "OK");
                return;
            }

            if (a < 0 || b < 0 || a >= so.allPictureDatas.Count || b >= so.allPictureDatas.Count)
            {
                EditorUtility.DisplayDialog("Error", "Indexes out of range!", "OK");
                return;
            }

            Undo.RecordObject(so, "Swap PictureDatas Elements");

            
            (so.allPictureDatas[a].picId, so.allPictureDatas[b].picId) = (so.allPictureDatas[b].picId, so.allPictureDatas[a].picId);
            (so.allPictureDatas[a], so.allPictureDatas[b]) = (so.allPictureDatas[b], so.allPictureDatas[a]);

            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", $"Swapped elements at index {a} and {b}", "OK");
        }

        #endregion
    }
}

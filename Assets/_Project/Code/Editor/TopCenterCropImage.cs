// Assets/Editor/TopCenterCropperWindow.cs
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NamPhuThuy.EditorTools
{
    public class TopCenterCropperWindow : EditorWindow
    {
        [SerializeField] private List<Texture2D> textures = new List<Texture2D>();

        #region Stats

        [SerializeField] private int targetXSize = 590;
        [SerializeField] private int targetYSize = 750;
        
        // NEW: size multipliers
        [SerializeField] private bool useSeparateMultipliers = false;
        [SerializeField] private float sizeMultiplier = 1f;
        [SerializeField] private float sizeMultiplierX = 1f;
        [SerializeField] private float sizeMultiplierY = 1f;
        
        // NEW: Offsets
        [SerializeField] private bool usePercentOffset = false;
        [SerializeField] private Vector2Int offsetPx = Vector2Int.zero;   // (+X right, +Y down)
        [SerializeField] private Vector2 offsetPercent = Vector2.zero;    // (+X right, +Y down), e.g., 0.1 = 10%

        #endregion

        #region Flags

        private bool _showTextures = true; // biến foldout

        #endregion
        
        [SerializeField] private DefaultAsset outputFolder; // pick an Assets folder
        [SerializeField] private bool overwriteExisting = false;

        [MenuItem("Tools/TrinhNam/Top-Center Cropper")]
        public static void ShowWindow()
        {
            GetWindow<TopCenterCropperWindow>("Top-Center Cropper");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Batch crop top-center square", EditorStyles.boldLabel);
            targetXSize = EditorGUILayout.IntField("Target Size (px)", Mathf.Max(1, targetXSize));
            targetYSize = EditorGUILayout.IntField("Target Size (px)", Mathf.Max(1, targetYSize));
            outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder (Assets)", outputFolder, typeof(DefaultAsset), false);

            EditorGUILayout.Space(4);
            useSeparateMultipliers = EditorGUILayout.ToggleLeft("Use separate X/Y multipliers", useSeparateMultipliers);
            using (new EditorGUI.IndentLevelScope())
            {
                if (!useSeparateMultipliers)
                {
                    sizeMultiplier = EditorGUILayout.FloatField("Size Multiplier", Mathf.Max(0.01f, sizeMultiplier));
                }
                else
                {
                    sizeMultiplierX = EditorGUILayout.FloatField("Width Multiplier (X)", Mathf.Max(0.01f, sizeMultiplierX));
                    sizeMultiplierY = EditorGUILayout.FloatField("Height Multiplier (Y)", Mathf.Max(0.01f, sizeMultiplierY));
                }
            }
            
            EditorGUILayout.Space(4);
            usePercentOffset = EditorGUILayout.ToggleLeft("Use % Offset (relative to crop size)", usePercentOffset);
            using (new EditorGUI.IndentLevelScope())
            {
                if (!usePercentOffset)
                {
                    offsetPx = EditorGUILayout.Vector2IntField("Offset (px)  [+X right, +Y down]", offsetPx);
                }
                else
                {
                    offsetPercent = EditorGUILayout.Vector2Field("Offset (%)  [+X right, +Y down]", offsetPercent);
                }
            }
            
            EditorGUILayout.Space(6);
            if (GUILayout.Button("Add Selected Textures"))
            {
                foreach (var obj in Selection.objects)
                {
                    if (obj is Texture2D tex && !textures.Contains(tex))
                        textures.Add(tex);
                }
            }

            EditorGUILayout.Space(6);
            overwriteExisting = EditorGUILayout.ToggleLeft("Overwrite existing files", overwriteExisting);

            EditorGUILayout.Space(6);
            _showTextures = EditorGUILayout.Foldout(
                _showTextures,
                $"Texture List ({textures.Count})", // thêm count
                true
            );
            
            EditorGUILayout.LabelField("Textures to process:", EditorStyles.boldLabel);
            if (_showTextures)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    int removeAt = -1;
                    for (int i = 0; i < textures.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        textures[i] = (Texture2D)EditorGUILayout.ObjectField(textures[i], typeof(Texture2D), false);
                        if (GUILayout.Button("X", GUILayout.Width(24))) removeAt = i;
                        EditorGUILayout.EndHorizontal();
                    }
                    if (removeAt >= 0) textures.RemoveAt(removeAt);
                }
            }

            EditorGUILayout.Space(10);
            using (new EditorGUI.DisabledScope(textures.Count == 0))
            {
                if (GUILayout.Button($"Process {textures.Count} Texture(s)"))
                {
                    ProcessAll();
                }
            }
            
            if (GUILayout.Button("Clear All"))
            {
                textures.Clear();
            }
        }

        void ProcessAll()
        {
            if (outputFolder == null)
            {
                EditorUtility.DisplayDialog("Output Folder Needed", "Please set an output folder under Assets.", "OK");
                return;
            }

            string outFolderPath = AssetDatabase.GetAssetPath(outputFolder);
            if (string.IsNullOrEmpty(outFolderPath) || !AssetDatabase.IsValidFolder(outFolderPath))
            {
                EditorUtility.DisplayDialog("Invalid Folder", "Output must be a folder under Assets/.", "OK");
                return;
            }

            int success = 0, skipped = 0;
            foreach (var tex in textures)
            {
                if (tex == null) { skipped++; continue; }

                string srcPath = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(srcPath)) { skipped++; continue; }

                // Ensure texture is readable (temporarily flip importer flag if needed)
                var importer = (TextureImporter)TextureImporter.GetAtPath(srcPath);
                bool restoreReadable = false;
                TextureImporterCompression oldCompression = TextureImporterCompression.Uncompressed;
                TextureImporterNPOTScale oldNpot = TextureImporterNPOTScale.None;
                bool oldReadable = false;

                if (importer != null)
                {
                    oldReadable = importer.isReadable;
                    oldCompression = importer.textureCompression;
                    oldNpot = importer.npotScale;

                    if (!importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.isReadable = true;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.npotScale = TextureImporterNPOTScale.None;
                        restoreReadable = true;
                        importer.SaveAndReimport();
                    }
                }

                try
                {
                    Texture2D src = AssetDatabase.LoadAssetAtPath<Texture2D>(srcPath);
                    if (src == null) { skipped++; continue; }
                    
                    // Compute effective sizes with multiplier(s)
                    float mX = useSeparateMultipliers ? sizeMultiplierX : sizeMultiplier;
                    float mY = useSeparateMultipliers ? sizeMultiplierY : sizeMultiplier;
                    
                    int effX = Mathf.Max(1, Mathf.RoundToInt(targetXSize * Mathf.Max(0.01f, mX)));
                    int effY = Mathf.Max(1, Mathf.RoundToInt(targetYSize * Mathf.Max(0.01f, mY)));
                    
                    // Effective offsets (px)
                    Vector2Int effOffsetPx;
                    if (!usePercentOffset)
                    {
                        effOffsetPx = offsetPx;
                    }
                    else
                    {
                        // Percent is relative to crop size (before clamping)
                        int ox = Mathf.RoundToInt(effX * offsetPercent.x);
                        int oy = Mathf.RoundToInt(effY * offsetPercent.y);
                        effOffsetPx = new Vector2Int(ox, oy);
                    }

                    // var cropped = CropTopCenter(src, effX, effY);
                    var cropped = CropTopCenterWithOffset(src, effX, effY, effOffsetPx);
                    // var cropped = CropTopCenter(src, targetXSize, targetYSize);

                    // Build output filename
                    string baseName = Path.GetFileNameWithoutExtension(srcPath);
                    // string outName = $"{baseName}_topcenter_{cropped.width}x{cropped.height}.png";
                    string outName = $"{baseName}.png";
                    string outPath = Path.Combine(outFolderPath, outName).Replace("\\", "/");

                    if (!overwriteExisting && File.Exists(outPath))
                        outPath = AssetDatabase.GenerateUniqueAssetPath(outPath);

                    byte[] png = cropped.EncodeToPNG();
                    File.WriteAllBytes(outPath, png);
                    AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceSynchronousImport);

                    success++;
                }
                catch (System.SystemException ex)
                {
                    Debug.LogError($"Crop failed for {tex.name}: {ex.Message}");
                    skipped++;
                }
                finally
                {
                    // Restore importer settings if we modified them
                    if (importer != null && restoreReadable)
                    {
                        importer.isReadable = oldReadable;
                        importer.textureCompression = oldCompression;
                        importer.npotScale = oldNpot;
                        importer.SaveAndReimport();
                    }
                }
            }

            AssetDatabase.Refresh();
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"Top-Center Cropper: Success={success}, Skipped/Failed={skipped}");
            EditorUtility.DisplayDialog("Done", $"Cropped {success} texture(s). Skipped/Failed: {skipped}", "OK");
        }

        /// <summary>
        /// Crops a square from the top-center of the source texture.
        /// If the source is smaller than targetSize, it will use the largest possible square that fits.
        /// </summary>
        static Texture2D CropTopCenter(Texture2D src, int targetXSize, int targetYSize)
        {
            int w = src.width;
            int h = src.height;

            int sizeX = Mathf.Min(targetXSize, w, h); // clamp if image is smaller
            int sizeY = Mathf.Min(targetYSize, w, h); // clamp if image is smaller
            
            
            
            int x = Mathf.Max(0, (w - sizeX) / 2);
            int y = Mathf.Max(0, h - sizeY); // top edge (Unity textures have origin at bottom-left)

            // Read pixels
            Color[] pixels = src.GetPixels(x, y, sizeX, sizeY);
            var outTex = new Texture2D(sizeX, sizeY, TextureFormat.RGBA32, false, false);
            outTex.SetPixels(pixels);
            outTex.Apply(false, false);
            return outTex;
        }
        
        /// <summary>
        /// Crops a rectangle from the top-center of the source texture, then applies an offset.
        /// Offset convention: (+X right, +Y down). Stays clamped inside the source.
        /// </summary>
        static Texture2D CropTopCenterWithOffset(Texture2D src, int targetXSize, int targetYSize, Vector2Int offsetPx)
        {
            int w = src.width;
            int h = src.height;

            // Clamp crop size to source
            int sizeX = Mathf.Min(targetXSize, w);
            int sizeY = Mathf.Min(targetYSize, h);

            // Base top-center rect
            int baseX = Mathf.Max(0, (w - sizeX) / 2);
            int baseYTopAligned = Mathf.Max(0, h - sizeY); // top-aligned (origin at bottom-left)

            // Apply offsets: +X moves right; +Y moves down (so subtract from baseY)
            int x = baseX + offsetPx.x;
            int y = baseYTopAligned - offsetPx.y;

            // Clamp inside image
            x = Mathf.Clamp(x, 0, w - sizeX);
            y = Mathf.Clamp(y, 0, h - sizeY);

            Color[] pixels = src.GetPixels(x, y, sizeX, sizeY);
            var outTex = new Texture2D(sizeX, sizeY, TextureFormat.RGBA32, false, false);
            outTex.SetPixels(pixels);
            outTex.Apply(false, false);
            return outTex;
        }
    }
}

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class BadgeGeneratorEditor : EditorWindow
{
    private Vector2Int badgeSize = new Vector2Int(600, 100);
    private float cornerRadius = 16f;
    private string saveFolderPath = "Assets/RankBadges";

    [MenuItem("Tools/Rank Badge Generator")]
    public static void ShowWindow()
    {
        GetWindow<BadgeGeneratorEditor>("Badge Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Horizontal Rank Badge Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        badgeSize = EditorGUILayout.Vector2IntField("Badge Dimensions (PX)", badgeSize);
        cornerRadius = EditorGUILayout.FloatField("Corner Radius", cornerRadius);
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate All 10 Rank Badges", GUILayout.Height(40)))
        {
            GenerateAndSaveAllBadges();
        }
    }

    private void GenerateAndSaveAllBadges()
    {
        // 1. Ensure target folder exists
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            AssetDatabase.Refresh();
        }

        // Generate procedural rounded corner sprite texture for building components
        Sprite roundedBaseSprite = CreateRoundedSquareSprite(128, cornerRadius);

        for (int level = 1; level <= 10; level++)
        {
            byte[] pngData = BakeBadgeToPNG(level, badgeSize, roundedBaseSprite);
            string filePath = Path.Combine(saveFolderPath, $"RankBadge_Lv{level}.png");

            File.WriteAllBytes(filePath, pngData);
        }

        // Clean up temporary base sprite
        DestroyImmediate(roundedBaseSprite.texture);
        DestroyImmediate(roundedBaseSprite);

        // 2. Refresh Asset Database so Unity detects the PNGs
        AssetDatabase.Refresh();

        // 3. Automatically import PNGs as 2D UI Sprites
        for (int level = 1; level <= 10; level++)
        {
            string filePath = Path.Combine(saveFolderPath, $"RankBadge_Lv{level}.png");
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        EditorUtility.DisplayDialog("Success", $"Generated 10 Rank Badge Sprites inside:\n{saveFolderPath}", "OK");
    }

    private byte[] BakeBadgeToPNG(int level, Vector2Int size, Sprite baseSprite)
    {
        // 1. Create root container for WorldSpace rendering
        GameObject bakerRoot = new GameObject("TempBadgeBaker", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas bakerCanvas = bakerRoot.GetComponent<Canvas>();
        bakerCanvas.renderMode = RenderMode.WorldSpace; // Must be WorldSpace for off-screen camera capturing

        RectTransform rootRect = bakerRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = size;
        rootRect.position = Vector3.zero;

        // 2. Setup camera aimed at World Space Canvas
        GameObject cameraObj = new GameObject("TempBakerCamera", typeof(Camera));
        Camera bakerCamera = cameraObj.GetComponent<Camera>();
        bakerCamera.clearFlags = CameraClearFlags.SolidColor;
        bakerCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent background
        bakerCamera.cullingMask = ~0; // Render all layers
        bakerCamera.orthographic = true;
        bakerCamera.orthographicSize = size.y / 2f;
        bakerCamera.nearClipPlane = 0.01f;
        bakerCamera.farClipPlane = 1000f;

        // Frame camera directly in front of canvas
        cameraObj.transform.position = new Vector3(0, 0, -100f);
        cameraObj.transform.LookAt(Vector3.zero);

        bakerCanvas.worldCamera = bakerCamera;

        // 3. Build procedural badge elements
        BuildBadgeHierarchy(bakerRoot.transform, level, size, baseSprite);
        Canvas.ForceUpdateCanvases();

        // 4. Create RenderTexture and bake
        RenderTexture renderTex = RenderTexture.GetTemporary(size.x, size.y, 24, RenderTextureFormat.ARGB32);
        bakerCamera.targetTexture = renderTex;

        // Force frame render into texture
        bakerCamera.Render();

        RenderTexture.active = renderTex;
        Texture2D resultTex = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
        resultTex.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
        resultTex.Apply();

        // 5. Cleanup memory and temporary objects
        RenderTexture.active = null;
        bakerCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTex);

        byte[] pngData = resultTex.EncodeToPNG();

        DestroyImmediate(bakerRoot);
        DestroyImmediate(cameraObj);
        DestroyImmediate(resultTex);

        return pngData;
    }

    #region Procedural Builders

    private void BuildBadgeHierarchy(Transform root, int level, Vector2Int size, Sprite baseSprite)
    {
        Color primary, secondary, plate, glow, gem;
        SetupTierColors(level, out primary, out secondary, out plate, out glow, out gem);

        float w = size.x;
        float h = size.y;
        int order = 0;

        // 1. Halo Aura (Level 7+)
        if (level >= 7)
        {
            float extraSize = (level >= 9) ? 40f : 20f;
            CreatePart("HaloAura", root, baseSprite, glow, new Vector2(w + extraSize, h + extraSize), Vector2.zero, order++);
        }

        // 2. Side Wings (Level 3+)
        if (level >= 3)
        {
            float wingW = (level >= 8) ? 60f : 40f;
            float wingH = h * 0.7f;
            GameObject leftWing = CreatePart("LeftWing", root, baseSprite, secondary, new Vector2(wingW, wingH), new Vector2(-(w / 2f + wingW / 3f), 0), order++);
            GameObject rightWing = CreatePart("RightWing", root, baseSprite, secondary, new Vector2(wingW, wingH), new Vector2((w / 2f + wingW / 3f), 0), order++);

            leftWing.transform.localRotation = Quaternion.Euler(0, 0, 15f);
            rightWing.transform.localRotation = Quaternion.Euler(0, 0, -15f);
        }

        // 3. Base Frame & Inner Plate
        GameObject outerFrame = CreatePart("OuterFrame", root, baseSprite, primary, size, Vector2.zero, order++);
        GameObject innerPlate = CreatePart("InnerPlate", outerFrame.transform, baseSprite, plate, new Vector2(w - 12f, h - 12f), Vector2.zero, order++);

        // 4. Slanted Progress Chevrons
        float crestSize = h * 0.75f;
        float crestX = -(w / 2f) + (crestSize / 2f) + 16f;
        float chevronsStartX = crestX + (crestSize / 2f) + 24f;
        int chevronCount = Mathf.Clamp((level - 1) % 4 + 1, 1, 4);

        for (int i = 0; i < chevronCount; i++)
        {
            float posX = chevronsStartX + (i * 22f);
            GameObject chevron = CreatePart($"Chevron_{i}", innerPlate.transform, baseSprite, secondary, new Vector2(12f, h * 0.5f), new Vector2(posX, 0), order++);
            chevron.transform.localRotation = Quaternion.Euler(0, 0, 15f);
        }

        // 5. Embedded Icon Crest
        GameObject rankIconFrame = CreatePart("RankIconFrame", innerPlate.transform, baseSprite, primary, new Vector2(crestSize, crestSize), new Vector2(crestX, 0), order++);
        rankIconFrame.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        if (level >= 5)
        {
            float gemSize = crestSize * 0.55f;
            CreatePart("CoreGem", rankIconFrame.transform, baseSprite, gem, new Vector2(gemSize, gemSize), Vector2.zero, order++);
        }

        // 6. Crown Accent (Level 9+)
        if (level >= 9)
        {
            CreatePart("CrownAccent", outerFrame.transform, baseSprite, primary, new Vector2(crestSize * 0.7f, 16f), new Vector2(crestX, h / 2f + 8f), order++);
        }

        // 7. Right Side Stars
        int starCount = (level <= 2) ? 1 : (level <= 4) ? 2 : (level <= 6) ? 3 : (level <= 8) ? 4 : 5;
        float starsStartX = (w / 2f) - 40f;

        for (int i = 0; i < starCount; i++)
        {
            float posX = starsStartX - (i * 28f);
            GameObject star = CreatePart($"StarInsignia_{i}", innerPlate.transform, baseSprite, primary, new Vector2(18f, 18f), new Vector2(posX, 0), order++);
            star.transform.localRotation = Quaternion.Euler(0, 0, 45f);
        }
    }

    private void SetupTierColors(int level, out Color primary, out Color secondary, out Color plate, out Color glow, out Color gem)
    {
        if (level <= 2) // Tier 1: Cadet
        {
            primary = new Color(0.68f, 0.44f, 0.25f); secondary = new Color(0.45f, 0.28f, 0.15f);
            plate = new Color(0.16f, 0.14f, 0.14f); glow = new Color(0.80f, 0.50f, 0.20f, 0.20f); gem = new Color(0.75f, 0.38f, 0.18f);
        }
        else if (level <= 4) // Tier 2: Officer
        {
            primary = new Color(0.78f, 0.82f, 0.88f); secondary = new Color(0.42f, 0.48f, 0.56f);
            plate = new Color(0.12f, 0.15f, 0.18f); glow = new Color(0.40f, 0.70f, 1.00f, 0.35f); gem = new Color(0.20f, 0.65f, 0.95f);
        }
        else if (level <= 6) // Tier 3: Elite
        {
            primary = new Color(0.98f, 0.78f, 0.18f); secondary = new Color(0.68f, 0.48f, 0.08f);
            plate = new Color(0.18f, 0.14f, 0.08f); glow = new Color(1.00f, 0.80f, 0.20f, 0.50f); gem = new Color(0.95f, 0.20f, 0.15f);
        }
        else if (level <= 8) // Tier 4: Master
        {
            primary = new Color(0.85f, 0.95f, 0.98f); secondary = new Color(0.20f, 0.58f, 0.68f);
            plate = new Color(0.08f, 0.12f, 0.16f); glow = new Color(0.10f, 0.95f, 0.98f, 0.65f); gem = new Color(0.00f, 0.90f, 0.90f);
        }
        else // Tier 5: Legend
        {
            primary = new Color(1.00f, 0.84f, 0.00f); secondary = new Color(0.88f, 0.12f, 0.22f);
            plate = new Color(0.08f, 0.05f, 0.09f); glow = new Color(0.95f, 0.25f, 0.95f, 0.80f); gem = new Color(0.80f, 0.10f, 1.00f);
        }
    }

    private GameObject CreatePart(string name, Transform parent, Sprite sprite, Color color, Vector2 size, Vector2 pos, int order)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;

        go.transform.SetSiblingIndex(order);
        return go;
    }

    private Sprite CreateRoundedSquareSprite(int size, float cornerR)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float halfSize = size / 2f;
        Vector2 boxHalfSize = new Vector2(halfSize - cornerR, halfSize - cornerR);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x - halfSize + 0.5f, y - halfSize + 0.5f);
                Vector2 d = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - boxHalfSize;
                float dist = Vector2.Max(d, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0f) - cornerR;

                float alpha = Mathf.Clamp01(0.5f - dist);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    #endregion
}
#endif

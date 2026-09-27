#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class BadgeGeneratorEditor : EditorWindow
{
    private Vector2Int badgeSize = new Vector2Int(600, 100);
    private string saveFolderPath = "Assets/RankBadges";

    [MenuItem("Tools/Rank Badge Generator")]
    public static void ShowWindow()
    {
        GetWindow<BadgeGeneratorEditor>("Badge Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("High-Quality Sci-Fi Badge Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        badgeSize = EditorGUILayout.Vector2IntField("Badge Dimensions (PX)", badgeSize);
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate 10 High-Quality Sci-Fi Badges", GUILayout.Height(40)))
        {
            GenerateAndSaveAllBadges();
        }
    }

    private void GenerateAndSaveAllBadges()
    {
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            AssetDatabase.Refresh();
        }

        // Generate procedural quality texture assets
        Sprite flatRect = CreateFlatSquareSprite();
        Sprite metallicPlate = CreateMetallicGradientSprite(256, 256);
        Sprite glowTexture = CreateSoftGlowSprite(256, 256);
        Sprite bevelFrame = CreateBevelBorderSprite(256, 256, 12);

        for (int level = 1; level <= 10; level++)
        {
            byte[] pngData = BakeBadgeToPNG(level, badgeSize, flatRect, metallicPlate, glowTexture, bevelFrame);
            string filePath = Path.Combine(saveFolderPath, $"RankBadge_Lv{level}.png");
            File.WriteAllBytes(filePath, pngData);
        }

        // Cleanup temporary sprites
        DestroyImmediate(flatRect.texture); DestroyImmediate(flatRect);
        DestroyImmediate(metallicPlate.texture); DestroyImmediate(metallicPlate);
        DestroyImmediate(glowTexture.texture); DestroyImmediate(glowTexture);
        DestroyImmediate(bevelFrame.texture); DestroyImmediate(bevelFrame);

        AssetDatabase.Refresh();

        // Reimport as single sprites with full quality
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

        EditorUtility.DisplayDialog("Success", $"Generated 10 HQ Sci-Fi Badges inside:\n{saveFolderPath}", "OK");
    }

    private byte[] BakeBadgeToPNG(int level, Vector2Int size, Sprite flatSprite, Sprite metallicSprite, Sprite glowSprite, Sprite bevelSprite)
    {
        GameObject bakerRoot = new GameObject("TempBadgeBaker", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas bakerCanvas = bakerRoot.GetComponent<Canvas>();
        bakerCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform rootRect = bakerRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = size;
        rootRect.position = Vector3.zero;

        GameObject cameraObj = new GameObject("TempBakerCamera", typeof(Camera));
        Camera bakerCamera = cameraObj.GetComponent<Camera>();
        bakerCamera.clearFlags = CameraClearFlags.SolidColor;
        bakerCamera.backgroundColor = new Color(0, 0, 0, 0);
        bakerCamera.cullingMask = ~0;
        bakerCamera.orthographic = true;
        bakerCamera.orthographicSize = size.y / 2f;
        bakerCamera.nearClipPlane = 0.01f;
        bakerCamera.farClipPlane = 1000f;

        cameraObj.transform.position = new Vector3(0, 0, -100f);
        cameraObj.transform.LookAt(Vector3.zero);

        bakerCanvas.worldCamera = bakerCamera;

        BuildBadgeHierarchy(bakerRoot.transform, level, size, flatSprite, metallicSprite, glowSprite, bevelSprite);
        Canvas.ForceUpdateCanvases();

        RenderTexture renderTex = RenderTexture.GetTemporary(size.x, size.y, 24, RenderTextureFormat.ARGB32);
        bakerCamera.targetTexture = renderTex;
        bakerCamera.Render();

        RenderTexture.active = renderTex;
        Texture2D resultTex = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
        resultTex.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
        resultTex.Apply();

        RenderTexture.active = null;
        bakerCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTex);

        byte[] pngData = resultTex.EncodeToPNG();

        DestroyImmediate(bakerRoot);
        DestroyImmediate(cameraObj);
        DestroyImmediate(resultTex);

        return pngData;
    }

    #region HQ Sci-Fi Builders

    private void BuildBadgeHierarchy(Transform root, int level, Vector2Int size, Sprite flat, Sprite metallic, Sprite glow, Sprite bevel)
    {
        Color primary, secondary, plate, glowColor, gem;
        SetupSciFiTierColors(level, out primary, out secondary, out plate, out glowColor, out gem);

        float w = size.x;
        float h = size.y;
        int order = 0;

        // 1. Core Back Ambient Reactor Glow (Level 5+)
        if (level >= 5)
        {
            float extraGlow = (level >= 8) ? 48f : 24f;
            CreatePart("ReactorGlow", root, glow, glowColor, new Vector2(w + extraGlow, h + extraGlow), Vector2.zero, order++);
        }

        // 2. Angular Side Wing Thruster Fins (Level 3+)
        if (level >= 3)
        {
            float wingW = (level >= 8) ? 38f : 26f;
            float wingH = h * 0.85f;
            
            GameObject leftWing = CreatePart("LeftWing", root, metallic, secondary * 0.8f, new Vector2(wingW, wingH), new Vector2(-(w / 2f + wingW / 2f - 4f), 0), order++);
            GameObject rightWing = CreatePart("RightWing", root, metallic, secondary * 0.8f, new Vector2(wingW, wingH), new Vector2((w / 2f + wingW / 2f - 4f), 0), order++);

            leftWing.transform.localRotation = Quaternion.Euler(0, 0, 12f);
            rightWing.transform.localRotation = Quaternion.Euler(0, 0, -12f);

            // Wing edge energy strip
            CreatePart("LeftWingGlow", leftWing.transform, glow, glowColor, new Vector2(wingW * 0.5f, wingH), Vector2.zero, order++);
            CreatePart("RightWingGlow", rightWing.transform, glow, glowColor, new Vector2(wingW * 0.5f, wingH), Vector2.zero, order++);
        }

        // 3. Main Outer Chassis (Metallic Frame + Metallic Bevel Edge)
        GameObject outerFrame = CreatePart("OuterFrame", root, metallic, primary, size, Vector2.zero, order++);
        CreatePart("OuterFrameBevel", outerFrame.transform, bevel, primary * 1.3f, size, Vector2.zero, order++);

        // 4. Dark Tech Inner Hull Plate
        GameObject innerPlate = CreatePart("InnerPlate", outerFrame.transform, metallic, plate, new Vector2(w - 10f, h - 10f), Vector2.zero, order++);
        CreatePart("InnerPlateBevel", innerPlate.transform, bevel, new Color(0, 0, 0, 0.6f), new Vector2(w - 10f, h - 10f), Vector2.zero, order++);

        // 5. Tech Corner Brackets (High-Tech Rectangular Trim)
        float bracketSize = 14f;
        float halfW = (w - 10f) / 2f - (bracketSize / 2f);
        float halfH = (h - 10f) / 2f - (bracketSize / 2f);

        CreatePart("BracketTL", innerPlate.transform, flat, primary, new Vector2(bracketSize, 3f), new Vector2(-halfW, halfH + 4f), order++);
        CreatePart("BracketTR", innerPlate.transform, flat, primary, new Vector2(bracketSize, 3f), new Vector2(halfW, halfH + 4f), order++);
        CreatePart("BracketBL", innerPlate.transform, flat, primary, new Vector2(bracketSize, 3f), new Vector2(-halfW, -halfH - 4f), order++);
        CreatePart("BracketBR", innerPlate.transform, flat, primary, new Vector2(bracketSize, 3f), new Vector2(halfW, -halfH - 4f), order++);

        // 6. Top/Bottom Metallic Hull Energy Rails
        float railThickness = 4f;
        GameObject topRail = CreatePart("TopRail", innerPlate.transform, metallic, primary, new Vector2(w - 32f, railThickness), new Vector2(0, (h / 2f) - 10f), order++);
        GameObject bottomRail = CreatePart("BottomRail", innerPlate.transform, metallic, primary, new Vector2(w - 32f, railThickness), new Vector2(0, -(h / 2f) + 10f), order++);
        
        // Add rail specular edge lines
        CreatePart("TopRailGlow", topRail.transform, glow, primary * 1.5f, new Vector2(w - 32f, railThickness * 2f), Vector2.zero, order++);
        CreatePart("BottomRailGlow", bottomRail.transform, glow, primary * 1.5f, new Vector2(w - 32f, railThickness * 2f), Vector2.zero, order++);

        // 7. Slanted Power Indicators (Chevrons with glow effect)
        float crestSize = h * 0.68f;
        float crestX = -(w / 2f) + (crestSize / 2f) + 24f;
        float chevronsStartX = crestX + (crestSize / 2f) + 28f;
        int chevronCount = Mathf.Clamp((level - 1) % 4 + 1, 1, 4);

        for (int i = 0; i < chevronCount; i++)
        {
            float posX = chevronsStartX + (i * 22f);
            GameObject chevron = CreatePart($"Chevron_{i}", innerPlate.transform, metallic, secondary, new Vector2(9f, h * 0.52f), new Vector2(posX, 0), order++);
            chevron.transform.localRotation = Quaternion.Euler(0, 0, 20f);
            
            // Glowing core for chevron
            GameObject chevronGlow = CreatePart($"ChevronGlow_{i}", chevron.transform, glow, primary * 1.4f, new Vector2(5f, h * 0.45f), Vector2.zero, order++);
        }

        // 8. Central Crest Node (Diamond Engine Core)
        GameObject rankIconFrame = CreatePart("RankIconFrame", innerPlate.transform, metallic, primary, new Vector2(crestSize, crestSize), new Vector2(crestX, 0), order++);
        rankIconFrame.transform.localRotation = Quaternion.Euler(0, 0, 45f);
        CreatePart("RankIconBevel", rankIconFrame.transform, bevel, primary * 1.5f, new Vector2(crestSize, crestSize), Vector2.zero, order++);

        if (level >= 5)
        {
            float gemSize = crestSize * 0.55f;
            GameObject coreGem = CreatePart("CoreGem", rankIconFrame.transform, metallic, gem, new Vector2(gemSize, gemSize), Vector2.zero, order++);
            CreatePart("CoreGemGlow", coreGem.transform, glow, gem * 1.6f, new Vector2(gemSize * 1.4f, gemSize * 1.4f), Vector2.zero, order++);
        }

        // 9. Crown Mod / Commander Crest (Level 9+)
        if (level >= 9)
        {
            GameObject crown = CreatePart("CrownAccent", outerFrame.transform, metallic, primary, new Vector2(crestSize * 0.85f, 10f), new Vector2(crestX, h / 2f + 5f), order++);
            CreatePart("CrownGlow", crown.transform, glow, glowColor, new Vector2(crestSize * 1.1f, 18f), Vector2.zero, order++);
        }

        // 10. Star Badges / Tech Level Insignias
        int starCount = (level <= 2) ? 1 : (level <= 4) ? 2 : (level <= 6) ? 3 : (level <= 8) ? 4 : 5;
        float starsStartX = (w / 2f) - 38f;

        for (int i = 0; i < starCount; i++)
        {
            float posX = starsStartX - (i * 26f);
            GameObject star = CreatePart($"TechInsignia_{i}", innerPlate.transform, metallic, primary, new Vector2(16f, 16f), new Vector2(posX, 0), order++);
            star.transform.localRotation = Quaternion.Euler(0, 0, 45f);
            CreatePart($"TechInsigniaGlow_{i}", star.transform, glow, glowColor * 1.2f, new Vector2(24f, 24f), Vector2.zero, order++);
        }
    }

    private void SetupSciFiTierColors(int level, out Color primary, out Color secondary, out Color plate, out Color glow, out Color gem)
    {
        if (level <= 2) // Tier 1: Cadet / Steel Blue
        {
            primary = new Color(0.55f, 0.65f, 0.75f); secondary = new Color(0.35f, 0.45f, 0.55f);
            plate = new Color(0.06f, 0.08f, 0.10f); glow = new Color(0.30f, 0.60f, 0.90f, 0.35f); gem = new Color(0.20f, 0.70f, 0.90f);
        }
        else if (level <= 4) // Tier 2: Officer / Neon Plasma
        {
            primary = new Color(0.20f, 0.88f, 0.98f); secondary = new Color(0.10f, 0.50f, 0.70f);
            plate = new Color(0.04f, 0.07f, 0.11f); glow = new Color(0.00f, 0.95f, 1.00f, 0.50f); gem = new Color(0.00f, 1.00f, 0.85f);
        }
        else if (level <= 6) // Tier 3: Commander / Solar Amber
        {
            primary = new Color(1.00f, 0.70f, 0.10f); secondary = new Color(0.70f, 0.38f, 0.00f);
            plate = new Color(0.10f, 0.07f, 0.03f); glow = new Color(1.00f, 0.65f, 0.00f, 0.60f); gem = new Color(1.00f, 0.20f, 0.00f);
        }
        else if (level <= 8) // Tier 4: Dreadnought / Warp Violet
        {
            primary = new Color(0.85f, 0.25f, 1.00f); secondary = new Color(0.50f, 0.08f, 0.70f);
            plate = new Color(0.08f, 0.03f, 0.10f); glow = new Color(0.90f, 0.15f, 1.00f, 0.70f); gem = new Color(1.00f, 0.00f, 0.60f);
        }
        else // Tier 5: Singularity / Hyper Gold
        {
            primary = new Color(1.00f, 0.92f, 0.30f); secondary = new Color(0.95f, 0.25f, 0.15f);
            plate = new Color(0.05f, 0.04f, 0.04f); glow = new Color(1.00f, 0.85f, 0.15f, 0.85f); gem = new Color(0.00f, 0.98f, 1.00f);
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

    #endregion

    #region Procedural Texture Generators

    private Sprite CreateFlatSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CreateMetallicGradientSprite(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            float v = y / (float)height;
            // High-contrast metallic reflection curve
            float metallic = Mathf.Sin(v * Mathf.PI * 2f) * 0.18f + 0.82f;
            if (v > 0.85f) metallic += (v - 0.85f) * 1.2f; // Top specular highlight

            Color col = new Color(metallic, metallic, metallic, 1f);
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CreateSoftGlowSprite(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(width / 2f, height / 2f);
        float maxDist = width / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float normDist = Mathf.Clamp01(dist / maxDist);
                float alpha = Mathf.Pow(1f - normDist, 2f); // Smooth radial falloff

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CreateBevelBorderSprite(int width, int height, int borderWidth)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBorder = (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth);
                if (isBorder)
                {
                    // Light from top, dark on bottom for realistic beveling
                    float val = (y > height / 2f) ? 0.9f : 0.3f;
                    tex.SetPixel(x, y, new Color(val, val, val, 1f));
                }
                else
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    #endregion
}
#endif
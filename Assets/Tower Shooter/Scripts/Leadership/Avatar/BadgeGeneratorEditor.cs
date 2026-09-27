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
        GUILayout.Label("Immersive Progressive Rank Badge Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        badgeSize = EditorGUILayout.Vector2IntField("Badge Dimensions (PX)", badgeSize);
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Progressive 10 Rank Badges", GUILayout.Height(40)))
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

        Sprite solidSprite = CreateFlatSquareSprite();
        Sprite softGlowSprite = CreateRadialGlowSprite(128);

        for (int level = 1; level <= 10; level++)
        {
            byte[] pngData = BakeBadgeToPNG(level, badgeSize, solidSprite, softGlowSprite);
            string filePath = Path.Combine(saveFolderPath, $"RankBadge_Lv{level}.png");
            File.WriteAllBytes(filePath, pngData);
        }

        DestroyImmediate(solidSprite.texture); DestroyImmediate(solidSprite);
        DestroyImmediate(softGlowSprite.texture); DestroyImmediate(softGlowSprite);

        AssetDatabase.Refresh();

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

        EditorUtility.DisplayDialog("Success", $"Generated 10 Progressive Rank Badges inside:\n{saveFolderPath}", "OK");
    }

    private byte[] BakeBadgeToPNG(int level, Vector2Int size, Sprite solidSprite, Sprite glowSprite)
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

        BuildImmersiveBadgeHierarchy(bakerRoot.transform, level, size, solidSprite, glowSprite);
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

    #region Immersive Procedural Builders

    private void BuildImmersiveBadgeHierarchy(Transform root, int level, Vector2Int size, Sprite solid, Sprite glow)
    {
        Color primaryAccent, secondaryAccent, plateBg, outerBorderColor;
        SetupHighContrastColors(level, out primaryAccent, out secondaryAccent, out plateBg, out outerBorderColor);

        float w = size.x;
        float h = size.y;
        int order = 0;

        Color voidDark = new Color(0.03f, 0.04f, 0.07f);
        Color crispWhite = new Color(0.98f, 0.98f, 1.00f);

        // 1. DYNAMIC AURA BACKDROP (Scales dramatically with rank level)
        if (level >= 4)
        {
            float extraGlow = (level >= 9) ? 50f : (level >= 7) ? 35f : 20f;
            float glowAlpha = (level >= 9) ? 0.75f : (level >= 7) ? 0.55f : 0.35f;
            CreatePart("OuterGlowField", root, glow, outerBorderColor * new Color(1, 1, 1, glowAlpha), new Vector2(w + extraGlow, h + extraGlow), Vector2.zero, order++);
        }

        // 2. WING STABILIZERS (Progressively larger & more detailed)
        if (level >= 3)
        {
            float wingW = (level >= 9) ? 38f : (level >= 7) ? 30f : (level >= 5) ? 24f : 18f;
            float wingH = h * ((level >= 7) ? 0.85f : 0.70f);

            GameObject leftWingOutline = CreatePart("LeftWingOutline", root, solid, voidDark, new Vector2(wingW + 6f, wingH + 6f), new Vector2(-(w / 2f + wingW / 2f), 0), order++);
            GameObject rightWingOutline = CreatePart("RightWingOutline", root, solid, voidDark, new Vector2(wingW + 6f, wingH + 6f), new Vector2((w / 2f + wingW / 2f), 0), order++);

            leftWingOutline.transform.localRotation = Quaternion.Euler(0, 0, 12f);
            rightWingOutline.transform.localRotation = Quaternion.Euler(0, 0, -12f);

            CreatePart("LeftWingFill", leftWingOutline.transform, solid, secondaryAccent, new Vector2(wingW, wingH), Vector2.zero, order++);
            CreatePart("RightWingFill", rightWingOutline.transform, solid, secondaryAccent, new Vector2(wingW, wingH), Vector2.zero, order++);

            // Thruster Energy Accents on Wing Tips (Rank 7+)
            if (level >= 7)
            {
                CreatePart("LeftWingEnergy", leftWingOutline.transform, solid, primaryAccent, new Vector2(4f, wingH * 0.6f), new Vector2(-wingW / 3f, 0), order++);
                CreatePart("RightWingEnergy", rightWingOutline.transform, solid, primaryAccent, new Vector2(4f, wingH * 0.6f), new Vector2(wingW / 3f, 0), order++);
            }
        }

        // 3. MAIN RECTANGULAR CONTAINER CHASSIS
        GameObject outerStroke = CreatePart("OuterStrokeFrame", root, solid, outerBorderColor, size, Vector2.zero, order++);

        float borderThickness = 4f;
        GameObject darkBorder = CreatePart("DarkBorder", outerStroke.transform, solid, voidDark, new Vector2(w - (borderThickness * 2), h - (borderThickness * 2)), Vector2.zero, order++);

        GameObject innerPlate = CreatePart("InnerPlate", darkBorder.transform, solid, plateBg, new Vector2(w - (borderThickness * 4), h - (borderThickness * 4)), Vector2.zero, order++);

        // 4. TOP / BOTTOM COMMAND RAIL ACCENTS (Rank 5+)
        if (level >= 5)
        {
            float railH = (level >= 8) ? 4f : 3f;
            CreatePart("TopAccentRail", innerPlate.transform, solid, primaryAccent, new Vector2(w - 24f, railH), new Vector2(0, (h / 2f) - 8f), order++);
            CreatePart("BottomAccentRail", innerPlate.transform, solid, primaryAccent, new Vector2(w - 24f, railH), new Vector2(0, -(h / 2f) + 8f), order++);
        }

        // 5. CHEVRONS & PROGRESSIVE HULL GRID LINES
        float crestSize = h * ((level >= 8) ? 0.72f : 0.62f);
        float crestX = -(w / 2f) + (crestSize / 2f) + 24f;
        float chevronsStartX = crestX + (crestSize / 2f) + 24f;
        int chevronCount = Mathf.Clamp((level - 1) % 4 + 1, 1, 4);

        for (int i = 0; i < chevronCount; i++)
        {
            float posX = chevronsStartX + (i * 20f);

            GameObject chevronBorder = CreatePart($"ChevronBorder_{i}", innerPlate.transform, solid, voidDark, new Vector2(12f, h * 0.54f), new Vector2(posX, 0), order++);
            chevronBorder.transform.localRotation = Quaternion.Euler(0, 0, 18f);

            CreatePart($"ChevronFill_{i}", chevronBorder.transform, solid, primaryAccent, new Vector2(6f, h * 0.46f), Vector2.zero, order++);
            
            // Higher ranks get inner core chevron highlights
            if (level >= 6)
            {
                CreatePart($"ChevronCore_{i}", chevronBorder.transform, solid, crispWhite, new Vector2(2f, h * 0.35f), Vector2.zero, order++);
            }
        }

        // 6. CENTRAL CREST & REACTOR NODE (Progressively richer layers)
        GameObject diamondDarkBase = CreatePart("DiamondDarkBase", innerPlate.transform, solid, voidDark, new Vector2(crestSize + 8f, crestSize + 8f), new Vector2(crestX, 0), order++);
        diamondDarkBase.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        GameObject diamondBorder = CreatePart("DiamondBorder", diamondDarkBase.transform, solid, outerBorderColor, new Vector2(crestSize + 4f, crestSize + 4f), Vector2.zero, order++);
        GameObject diamondFill = CreatePart("DiamondFill", diamondBorder.transform, solid, primaryAccent, new Vector2(crestSize, crestSize), Vector2.zero, order++);

        if (level >= 4)
        {
            float coreSize = crestSize * 0.5f;
            CreatePart("CoreNodeBase", diamondFill.transform, solid, voidDark, new Vector2(coreSize + 4f, coreSize + 4f), Vector2.zero, order++);
            CreatePart("CoreNode", diamondFill.transform, solid, crispWhite, new Vector2(coreSize, coreSize), Vector2.zero, order++);
        }

        if (level >= 8)
        {
            // Inner Starburst Core for High Tiers
            float innerStarSize = crestSize * 0.28f;
            GameObject starburst = CreatePart("CoreStarburst", diamondFill.transform, solid, secondaryAccent, new Vector2(innerStarSize, innerStarSize), Vector2.zero, order++);
            starburst.transform.localRotation = Quaternion.Euler(0, 0, 45f);
        }

        // 7. UPPER COMMAND CROWN MOD (Rank 9+)
        if (level >= 9)
        {
            GameObject crownDark = CreatePart("CrownDark", outerStroke.transform, solid, voidDark, new Vector2(crestSize + 12f, 10f), new Vector2(crestX, h / 2f + 4f), order++);
            CreatePart("CrownFill", crownDark.transform, solid, primaryAccent, new Vector2(crestSize + 8f, 6f), Vector2.zero, order++);
        }

        // 8. RANK INSIGNIA STARS
        int starCount = (level <= 2) ? 1 : (level <= 4) ? 2 : (level <= 6) ? 3 : (level <= 8) ? 4 : 5;
        float starsStartX = (w / 2f) - 36f;

        for (int i = 0; i < starCount; i++)
        {
            float posX = starsStartX - (i * 26f);

            GameObject starBorder = CreatePart($"StarBorder_{i}", innerPlate.transform, solid, voidDark, new Vector2(20f, 20f), new Vector2(posX, 0), order++);
            starBorder.transform.localRotation = Quaternion.Euler(0, 0, 45f);

            GameObject starOuter = CreatePart($"StarOuter_{i}", starBorder.transform, solid, outerBorderColor, new Vector2(16f, 16f), Vector2.zero, order++);
            CreatePart($"StarCore_{i}", starOuter.transform, solid, primaryAccent, new Vector2(10f, 10f), Vector2.zero, order++);

            if (level >= 7)
            {
                // Crisp white star core dot for high ranks
                CreatePart($"StarPoint_{i}", starOuter.transform, solid, crispWhite, new Vector2(4f, 4f), Vector2.zero, order++);
            }
        }
    }

    private void SetupHighContrastColors(int level, out Color primaryAccent, out Color secondaryAccent, out Color plateBg, out Color outerBorderColor)
    {
        plateBg = new Color(0.04f, 0.05f, 0.10f);

        if (level <= 2) // Tier 1: Recruit (Electric Cyan)
        {
            primaryAccent = new Color(0.00f, 0.95f, 1.00f);
            secondaryAccent = new Color(0.15f, 0.40f, 0.65f);
            outerBorderColor = new Color(0.00f, 0.85f, 1.00f);
        }
        else if (level <= 4) // Tier 2: Officer (Vivid Violet)
        {
            primaryAccent = new Color(0.68f, 0.38f, 1.00f);
            secondaryAccent = new Color(0.38f, 0.18f, 0.68f);
            outerBorderColor = new Color(0.60f, 0.32f, 0.98f);
        }
        else if (level <= 6) // Tier 3: Commander (Solar Orange)
        {
            primaryAccent = new Color(1.00f, 0.40f, 0.00f);
            secondaryAccent = new Color(0.65f, 0.20f, 0.00f);
            outerBorderColor = new Color(1.00f, 0.50f, 0.10f);
        }
        else if (level <= 8) // Tier 4: Dreadnought (Electric Lime)
        {
            primaryAccent = new Color(0.00f, 1.00f, 0.40f);
            secondaryAccent = new Color(0.00f, 0.50f, 0.25f);
            outerBorderColor = new Color(0.20f, 0.95f, 0.45f);
        }
        else // Tier 5: Singularity / Overlord (Hyperdrive Gold & Magenta)
        {
            primaryAccent = new Color(1.00f, 0.85f, 0.00f);
            secondaryAccent = new Color(1.00f, 0.15f, 0.52f);
            outerBorderColor = new Color(1.00f, 0.80f, 0.10f);
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

    private Sprite CreateFlatSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CreateRadialGlowSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDist = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float normDist = Mathf.Clamp01(dist / maxDist);
                float alpha = Mathf.Pow(1f - normDist, 2.5f);

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    #endregion
}
#endif

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class MinimalResearchIconGeneratorEditor : EditorWindow
{
    private Vector2Int iconSize = new Vector2Int(512, 512);
    private string saveFolderPath = "Assets/ResearchIcons_Minimal";

    [MenuItem("Tools/2D Flat Research Icon Generator")]
    public static void ShowWindow()
    {
        GetWindow<MinimalResearchIconGeneratorEditor>("Flat Icon Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Flat Vector Research Icon Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        iconSize = EditorGUILayout.Vector2IntField("Sprite Dimensions (PX)", iconSize);
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate 5 Flat UI Research Icons", GUILayout.Height(40)))
        {
            GenerateAndSaveAllIcons();
        }
    }

    private void GenerateAndSaveAllIcons()
    {
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            AssetDatabase.Refresh();
        }

        Sprite solidSprite = CreateFlatSquareSprite();
        Sprite circleSprite = CreateCircleSprite(256);
        Sprite ringSprite = CreateRingSprite(256, 0.88f);

        string[] iconNames = new string[]
        {
            "Research_01_ErlenmeyerFlask",
            "Research_02_SpaceshipEngine",
            "Research_03_ShieldTech",
            "Research_04_EnergyLightning",
            "Research_05_AIBrainChip"
        };

        for (int i = 0; i < iconNames.Length; i++)
        {
            byte[] pngData = BakeIconToPNG(i + 1, iconSize, solidSprite, circleSprite, ringSprite);
            string filePath = Path.Combine(saveFolderPath, $"{iconNames[i]}.png");
            File.WriteAllBytes(filePath, pngData);
        }

        // Cleanup
        DestroyImmediate(solidSprite.texture); DestroyImmediate(solidSprite);
        DestroyImmediate(circleSprite.texture); DestroyImmediate(circleSprite);
        DestroyImmediate(ringSprite.texture); DestroyImmediate(ringSprite);

        AssetDatabase.Refresh();

        // Reimport Sprites
        for (int i = 0; i < iconNames.Length; i++)
        {
            string filePath = Path.Combine(saveFolderPath, $"{iconNames[i]}.png");
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

        EditorUtility.DisplayDialog("Success", $"Generated 5 Flat UI Research Icons inside:\n{saveFolderPath}", "OK");
    }

    private byte[] BakeIconToPNG(int iconId, Vector2Int size, Sprite solid, Sprite circle, Sprite ring)
    {
        GameObject bakerRoot = new GameObject("TempIconBaker", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
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

        BuildFlatIconHierarchy(bakerRoot.transform, iconId, size, solid, circle, ring);
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

    private void BuildFlatIconHierarchy(Transform root, int iconId, Vector2Int size, Sprite solid, Sprite circle, Sprite ring)
    {
        // Color Palette matching your UI screenshot
        Color uiCyan = new Color(0.00f, 0.82f, 1.00f, 1.00f);      // Primary Accent (#00D2FF)
        Color uiWhite = new Color(1.00f, 1.00f, 1.00f, 1.00f);     // Pure White Icons
        Color uiNavyBg = new Color(0.08f, 0.12f, 0.20f, 1.00f);    // Panel Navy Background
        Color uiBorderNavy = new Color(0.18f, 0.25f, 0.38f, 1.00f);// Subtle Border Outline

        int order = 0;
        float baseSize = Mathf.Min(size.x, size.y) * 0.72f;

        // 1. Outer Container Badge (Rounded Frame matching UI containers)
        CreatePart("Badge_Backing", root, circle, uiNavyBg, new Vector2(baseSize, baseSize), Vector2.zero, order++);
        CreatePart("Badge_Border", root, ring, uiBorderNavy, new Vector2(baseSize, baseSize), Vector2.zero, order++);
        CreatePart("Badge_CyanRing", root, ring, uiCyan * new Color(1, 1, 1, 0.4f), new Vector2(baseSize * 0.96f, baseSize * 0.96f), Vector2.zero, order++);

        // 2. Minimalist Flat Vector Icons
        switch (iconId)
        {
            case 1: // Flat Science Flask (Matches top bar Research Icon)
                BuildFlaskIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 2: // Flat Rocket Engine (Voyage / Propulsion)
                BuildRocketIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 3: // Flat Defense Shield
                BuildShieldIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 4: // Energy Lightning Bolt (Matches UI power stat)
                BuildBoltIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 5: // Flat AI Microchip
                BuildChipIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
        }
    }

    #region Flat Icon Drawings

    private void BuildFlaskIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        // Neck & Rim
        CreatePart("Flask_Rim", root, solid, white, new Vector2(baseSize * 0.16f, baseSize * 0.04f), new Vector2(0, baseSize * 0.22f), order++);
        CreatePart("Flask_Neck", root, solid, white, new Vector2(baseSize * 0.08f, baseSize * 0.14f), new Vector2(0, baseSize * 0.13f), order++);

        // Flask Triangular Base
        GameObject body = CreatePart("Flask_Base", root, solid, white, new Vector2(baseSize * 0.32f, baseSize * 0.30f), new Vector2(0, -baseSize * 0.08f), order++);
        body.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        // Liquid Line (Cyan Accent)
        CreatePart("Liquid_Level", root, solid, cyan, new Vector2(baseSize * 0.22f, baseSize * 0.06f), new Vector2(0, -baseSize * 0.08f), order++);
        CreatePart("Bubble_1", root, circle, cyan, new Vector2(baseSize * 0.04f, baseSize * 0.04f), new Vector2(-baseSize * 0.04f, 0f), order++);
        CreatePart("Bubble_2", root, circle, cyan, new Vector2(baseSize * 0.03f, baseSize * 0.03f), new Vector2(baseSize * 0.05f, baseSize * 0.06f), order++);
    }

    private void BuildRocketIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        // Main Body
        CreatePart("Rocket_Body", root, solid, white, new Vector2(baseSize * 0.12f, baseSize * 0.32f), new Vector2(0, baseSize * 0.02f), order++);

        // Nose Cone
        GameObject nose = CreatePart("Rocket_Nose", root, solid, white, new Vector2(baseSize * 0.12f, baseSize * 0.12f), new Vector2(0, baseSize * 0.20f), order++);
        nose.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        // Wings
        GameObject wingL = CreatePart("Wing_L", root, solid, white, new Vector2(baseSize * 0.08f, baseSize * 0.16f), new Vector2(-baseSize * 0.10f, -baseSize * 0.05f), order++);
        wingL.transform.localRotation = Quaternion.Euler(0, 0, 30f);

        GameObject wingR = CreatePart("Wing_R", root, solid, white, new Vector2(baseSize * 0.08f, baseSize * 0.16f), new Vector2(baseSize * 0.10f, -baseSize * 0.05f), order++);
        wingR.transform.localRotation = Quaternion.Euler(0, 0, -30f);

        // Cyan Flame Thrust
        CreatePart("Flame_Base", root, solid, cyan, new Vector2(baseSize * 0.08f, baseSize * 0.12f), new Vector2(0, -baseSize * 0.20f), order++);
    }

    private void BuildShieldIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        // Flat Minimal Shield
        CreatePart("Shield_Top", root, solid, white, new Vector2(baseSize * 0.32f, baseSize * 0.18f), new Vector2(0, baseSize * 0.08f), order++);

        GameObject shieldBottom = CreatePart("Shield_Tip", root, solid, white, new Vector2(baseSize * 0.22f, baseSize * 0.22f), new Vector2(0, -baseSize * 0.07f), order++);
        shieldBottom.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        // Inner Cyan Core Symbol
        CreatePart("Shield_Core", root, solid, cyan, new Vector2(baseSize * 0.08f, baseSize * 0.18f), new Vector2(0, 0), order++);
        CreatePart("Shield_Cross", root, solid, cyan, new Vector2(baseSize * 0.18f, baseSize * 0.08f), new Vector2(0, baseSize * 0.02f), order++);
    }

    private void BuildBoltIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        // Sharp Flat Lightning Bolt (Matches Energy icon)
        GameObject topSegment = CreatePart("Bolt_Top", root, solid, cyan, new Vector2(baseSize * 0.08f, baseSize * 0.28f), new Vector2(baseSize * 0.03f, baseSize * 0.08f), order++);
        topSegment.transform.localRotation = Quaternion.Euler(0, 0, -25f);

        GameObject botSegment = CreatePart("Bolt_Bottom", root, solid, cyan, new Vector2(baseSize * 0.08f, baseSize * 0.28f), new Vector2(-baseSize * 0.03f, -baseSize * 0.08f), order++);
        botSegment.transform.localRotation = Quaternion.Euler(0, 0, -25f);

        CreatePart("Bolt_Center_Fill", root, solid, cyan, new Vector2(baseSize * 0.14f, baseSize * 0.06f), Vector2.zero, order++);
    }

    private void BuildChipIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        // Central Microchip Square
        CreatePart("Chip_Center", root, solid, white, new Vector2(baseSize * 0.24f, baseSize * 0.24f), Vector2.zero, order++);
        CreatePart("Chip_Core_Accent", root, solid, cyan, new Vector2(baseSize * 0.12f, baseSize * 0.12f), Vector2.zero, order++);

        // Chip Connector Pins
        float offset = baseSize * 0.18f;
        for (int i = -1; i <= 1; i += 2)
        {
            float shift = i * (baseSize * 0.06f);
            CreatePart($"Pin_T_{i}", root, solid, white, new Vector2(baseSize * 0.03f, baseSize * 0.08f), new Vector2(shift, offset), order++);
            CreatePart($"Pin_B_{i}", root, solid, white, new Vector2(baseSize * 0.03f, baseSize * 0.08f), new Vector2(shift, -offset), order++);
            CreatePart($"Pin_L_{i}", root, solid, white, new Vector2(baseSize * 0.08f, baseSize * 0.03f), new Vector2(-offset, shift), order++);
            CreatePart($"Pin_R_{i}", root, solid, white, new Vector2(baseSize * 0.08f, baseSize * 0.03f), new Vector2(offset, shift), order++);
        }
    }

    #endregion

    #region Helpers

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

    private Sprite CreateCircleSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius - dist);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CreateRingSprite(int size, float innerRadiusRatio)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float outerRadius = size / 2f;
        float innerRadius = outerRadius * innerRadiusRatio;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float outerAlpha = Mathf.Clamp01(outerRadius - dist);
                float innerAlpha = Mathf.Clamp01(dist - innerRadius);
                float alpha = Mathf.Min(outerAlpha, innerAlpha);

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    #endregion
}
#endif
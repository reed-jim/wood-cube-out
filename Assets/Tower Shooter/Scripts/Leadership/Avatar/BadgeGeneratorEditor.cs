#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class BadgeGeneratorEditor : EditorWindow
{
    private Vector2Int nodeSize = new Vector2Int(512, 512);
    private string saveFolderPath = "Assets/NodeSprites";

    [MenuItem("Tools/2D Node Sprite Generator")]
    public static void ShowWindow()
    {
        GetWindow<BadgeGeneratorEditor>("Node Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Progressive 2D Circular Power Node Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        nodeSize = EditorGUILayout.Vector2IntField("Sprite Dimensions (PX)", nodeSize);
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Progressive 10 Node Levels", GUILayout.Height(40)))
        {
            GenerateAndSaveAllNodes();
        }
    }

    private void GenerateAndSaveAllNodes()
    {
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            AssetDatabase.Refresh();
        }

        // Generate base procedural textures
        Sprite solidSprite = CreateFlatSquareSprite();
        Sprite softGlowSprite = CreateRadialGlowSprite(256);
        Sprite circleSprite = CreateCircleSprite(256);
        Sprite ringSprite = CreateRingSprite(256, 0.82f);

        for (int level = 1; level <= 10; level++)
        {
            byte[] pngData = BakeNodeToPNG(level, nodeSize, solidSprite, softGlowSprite, circleSprite, ringSprite);
            string filePath = Path.Combine(saveFolderPath, $"PowerNode_Lv{level}.png");
            File.WriteAllBytes(filePath, pngData);
        }

        // Clean up memory
        DestroyImmediate(solidSprite.texture); DestroyImmediate(solidSprite);
        DestroyImmediate(softGlowSprite.texture); DestroyImmediate(softGlowSprite);
        DestroyImmediate(circleSprite.texture); DestroyImmediate(circleSprite);
        DestroyImmediate(ringSprite.texture); DestroyImmediate(ringSprite);

        AssetDatabase.Refresh();

        // Reimport assets as 2D Sprites
        for (int level = 1; level <= 10; level++)
        {
            string filePath = Path.Combine(saveFolderPath, $"PowerNode_Lv{level}.png");
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

        EditorUtility.DisplayDialog("Success", $"Generated 10 Level Power Nodes inside:\n{saveFolderPath}", "OK");
    }

    private byte[] BakeNodeToPNG(int level, Vector2Int size, Sprite solidSprite, Sprite glowSprite, Sprite circleSprite, Sprite ringSprite)
    {
        GameObject bakerRoot = new GameObject("TempNodeBaker", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
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

        BuildNodeHierarchy(bakerRoot.transform, level, size, solidSprite, glowSprite, circleSprite, ringSprite);
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

    #region Procedural Builders

    private void BuildNodeHierarchy(Transform root, int level, Vector2Int size, Sprite solid, Sprite glow, Sprite circle, Sprite ring)
    {
        Color primaryAccent, secondaryAccent, plateBg, outerBorderColor;
        SetupHighContrastColors(level, out primaryAccent, out secondaryAccent, out plateBg, out outerBorderColor);

        Color voidDark = new Color(0.03f, 0.04f, 0.07f);
        Color crispWhite = new Color(0.98f, 0.98f, 1.00f);

        float baseDiameter = Mathf.Min(size.x, size.y) * 0.72f;
        int order = 0;

        // 1. POWER AURA GLOW (Unlocks Lv3+)
        if (level >= 3)
        {
            float glowScale = 1.25f + (level * 0.05f);
            float glowAlpha = 0.25f + (level * 0.06f);
            CreatePart("AuraGlow", root, glow, outerBorderColor * new Color(1, 1, 1, glowAlpha), new Vector2(baseDiameter * glowScale, baseDiameter * glowScale), Vector2.zero, order++);
        }

        // 2. ORBITAL RUNIC RINGS (Unlocks Lv5+ and Lv8+)
        if (level >= 5)
        {
            float ring1Size = baseDiameter * 1.15f;
            GameObject outerRing = CreatePart("OuterOrbitalRing", root, ring, primaryAccent * 0.85f, new Vector2(ring1Size, ring1Size), Vector2.zero, order++);

            if (level >= 8)
            {
                float ring2Size = baseDiameter * 1.30f;
                GameObject secondaryRing = CreatePart("SecondaryOrbitalRing", root, ring, secondaryAccent, new Vector2(ring2Size, ring2Size), Vector2.zero, order++);
                secondaryRing.transform.localRotation = Quaternion.Euler(0, 0, 45f);
            }
        }

        // 3. MAIN CIRCULAR CHASSIS
        GameObject darkBorder = CreatePart("NodeBaseDark", root, circle, voidDark, new Vector2(baseDiameter, baseDiameter), Vector2.zero, order++);

        float outerThickness = baseDiameter * 0.92f;
        GameObject outerFrame = CreatePart("NodeOuterFrame", darkBorder.transform, circle, outerBorderColor, new Vector2(outerThickness, outerThickness), Vector2.zero, order++);

        float innerFillSize = baseDiameter * 0.84f;
        GameObject nodePlate = CreatePart("NodePlate", outerFrame.transform, circle, plateBg, new Vector2(innerFillSize, innerFillSize), Vector2.zero, order++);

        // 4. INNER ACCENT RING (Unlocks Lv2+)
        if (level >= 2)
        {
            float accentRingSize = innerFillSize * 0.88f;
            CreatePart("InnerAccentRing", nodePlate.transform, ring, primaryAccent, new Vector2(accentRingSize, accentRingSize), Vector2.zero, order++);
        }

        // 5. ORBITING POWER CAPACITOR SUB-NODES (Scales 2 to 8 nodes based on level)
        int orbCount = Mathf.Clamp(level, 2, 8);
        float orbRadius = (innerFillSize / 2f) * 0.70f;

        for (int i = 0; i < orbCount; i++)
        {
            float angle = i * (360f / orbCount) * Mathf.Deg2Rad;
            Vector2 orbPos = new Vector2(Mathf.Cos(angle) * orbRadius, Mathf.Sin(angle) * orbRadius);
            float orbSize = (level >= 7) ? 16f : 12f;

            GameObject orbDark = CreatePart($"OrbDark_{i}", nodePlate.transform, circle, voidDark, new Vector2(orbSize + 4f, orbSize + 4f), orbPos, order++);
            CreatePart($"OrbCore_{i}", orbDark.transform, circle, primaryAccent, new Vector2(orbSize, orbSize), Vector2.zero, order++);

            if (level >= 6)
            {
                CreatePart($"OrbHighlight_{i}", orbDark.transform, circle, crispWhite, new Vector2(orbSize * 0.4f, orbSize * 0.4f), Vector2.zero, order++);
            }
        }

        // 6. CENTRAL POWER CORE REACTOR
        float coreRatio = (level >= 9) ? 0.48f : (level >= 6) ? 0.40f : 0.32f;
        float coreSize = innerFillSize * coreRatio;

        GameObject coreBase = CreatePart("CoreBaseDark", nodePlate.transform, circle, voidDark, new Vector2(coreSize + 6f, coreSize + 6f), Vector2.zero, order++);
        GameObject coreGlow = CreatePart("CoreGlow", coreBase.transform, circle, primaryAccent, new Vector2(coreSize, coreSize), Vector2.zero, order++);
        CreatePart("CoreCenter", coreGlow.transform, circle, crispWhite, new Vector2(coreSize * 0.5f, coreSize * 0.5f), Vector2.zero, order++);

        // 7. HIGH-TIER STARBURST ACCENTS (Unlocks Lv7+)
        if (level >= 7)
        {
            float starSize = coreSize * 0.70f;
            GameObject star1 = CreatePart("StarCore1", coreGlow.transform, solid, secondaryAccent, new Vector2(starSize, starSize), Vector2.zero, order++);
            star1.transform.localRotation = Quaternion.Euler(0, 0, 45f);

            if (level >= 9)
            {
                GameObject star2 = CreatePart("StarCore2", coreGlow.transform, solid, crispWhite, new Vector2(starSize * 0.55f, starSize * 0.55f), Vector2.zero, order++);
                star2.transform.localRotation = Quaternion.Euler(0, 0, 0f);
            }
        }
    }

    private void SetupHighContrastColors(int level, out Color primaryAccent, out Color secondaryAccent, out Color plateBg, out Color outerBorderColor)
    {
        plateBg = new Color(0.04f, 0.05f, 0.10f);

        if (level <= 2) // Tier 1: Electric Cyan
        {
            primaryAccent = new Color(0.00f, 0.95f, 1.00f);
            secondaryAccent = new Color(0.15f, 0.40f, 0.65f);
            outerBorderColor = new Color(0.00f, 0.85f, 1.00f);
        }
        else if (level <= 4) // Tier 2: Vivid Violet
        {
            primaryAccent = new Color(0.68f, 0.38f, 1.00f);
            secondaryAccent = new Color(0.38f, 0.18f, 0.68f);
            outerBorderColor = new Color(0.60f, 0.32f, 0.98f);
        }
        else if (level <= 6) // Tier 3: Solar Orange
        {
            primaryAccent = new Color(1.00f, 0.40f, 0.00f);
            secondaryAccent = new Color(0.65f, 0.20f, 0.00f);
            outerBorderColor = new Color(1.00f, 0.50f, 0.10f);
        }
        else if (level <= 8) // Tier 4: Electric Lime
        {
            primaryAccent = new Color(0.00f, 1.00f, 0.40f);
            secondaryAccent = new Color(0.00f, 0.50f, 0.25f);
            outerBorderColor = new Color(0.20f, 0.95f, 0.45f);
        }
        else // Tier 5: Overlord Gold & Magenta
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
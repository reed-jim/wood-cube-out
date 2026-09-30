#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class BorderAuraGeneratorEditor : EditorWindow
{
    private Vector2Int nodeSize = new Vector2Int(512, 512);
    private string saveFolderPath = "Assets/NodeAuraBorders";

    [MenuItem("Tools/2D Circle Aura Generator")]
    public static void ShowWindow()
    {
        GetWindow<BorderAuraGeneratorEditor>("Aura Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("10-Rank Circular Border Aura Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        nodeSize = EditorGUILayout.Vector2IntField("Sprite Dimensions (PX)", nodeSize);
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate 10 Border Aura Ranks", GUILayout.Height(40)))
        {
            GenerateAndSaveAllAuras();
        }
    }

    private void GenerateAndSaveAllAuras()
    {
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            AssetDatabase.Refresh();
        }

        // Generate procedural base textures
        Sprite solidSprite = CreateFlatSquareSprite();
        Sprite softGlowSprite = CreateRadialGlowSprite(256);
        Sprite circleSprite = CreateCircleSprite(256);
        Sprite ringSprite = CreateRingSprite(256, 0.85f);
        Sprite thinRingSprite = CreateRingSprite(256, 0.94f);

        for (int rank = 1; rank <= 10; rank++)
        {
            byte[] pngData = BakeAuraToPNG(rank, nodeSize, solidSprite, softGlowSprite, circleSprite, ringSprite, thinRingSprite);
            string filePath = Path.Combine(saveFolderPath, $"BorderAura_Rank{rank}.png");
            File.WriteAllBytes(filePath, pngData);
        }

        // Clean up temporary textures
        DestroyImmediate(solidSprite.texture); DestroyImmediate(solidSprite);
        DestroyImmediate(softGlowSprite.texture); DestroyImmediate(softGlowSprite);
        DestroyImmediate(circleSprite.texture); DestroyImmediate(circleSprite);
        DestroyImmediate(ringSprite.texture); DestroyImmediate(ringSprite);
        DestroyImmediate(thinRingSprite.texture); DestroyImmediate(thinRingSprite);

        AssetDatabase.Refresh();

        // Reimport as 2D Sprites
        for (int rank = 1; rank <= 10; rank++)
        {
            string filePath = Path.Combine(saveFolderPath, $"BorderAura_Rank{rank}.png");
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

        EditorUtility.DisplayDialog("Success", $"Generated 10 Border Aura Ranks inside:\n{saveFolderPath}", "OK");
    }

    private byte[] BakeAuraToPNG(int rank, Vector2Int size, Sprite solidSprite, Sprite glowSprite, Sprite circleSprite, Sprite ringSprite, Sprite thinRingSprite)
    {
        GameObject bakerRoot = new GameObject("TempAuraBaker", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
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

        BuildAuraHierarchy(bakerRoot.transform, rank, size, solidSprite, glowSprite, circleSprite, ringSprite, thinRingSprite);
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

    private void BuildAuraHierarchy(Transform root, int rank, Vector2Int size, Sprite solid, Sprite glow, Sprite circle, Sprite ring, Sprite thinRing)
    {
        Color primaryAccent, secondaryAccent, glowColor, rimColor;
        GetRankColors(rank, out primaryAccent, out secondaryAccent, out glowColor, out rimColor);

        Color crispWhite = new Color(0.98f, 0.98f, 1.00f, 0.95f);
        Color voidDark = new Color(0.02f, 0.02f, 0.05f, 0.85f);

        // Core dimensions (Center remains hollow)
        float baseBorderDiameter = Mathf.Min(size.x, size.y) * 0.60f;
        int order = 0;

        // 1. DISTANT OUTER GLOW (Scales intensity and reach per rank)
        float outerGlowSize = baseBorderDiameter * (1.30f + (rank * 0.06f));
        float glowAlpha = Mathf.Min(0.3f + (rank * 0.06f), 0.85f);
        Color finalOuterGlow = glowColor;
        finalOuterGlow.a = glowAlpha;
        CreatePart("OuterAuraGlow", root, glow, finalOuterGlow, new Vector2(outerGlowSize, outerGlowSize), Vector2.zero, order++);

        // 2. RAY BURSTS / STARBURST BEAMS (Unlocks Rank 7+)
        if (rank >= 7)
        {
            int rayCount = (rank >= 9) ? 8 : 4;
            float rayLength = baseBorderDiameter * (0.8f + (rank * 0.05f));
            float rayThickness = 6f + rank;

            for (int i = 0; i < rayCount; i++)
            {
                float angle = i * (180f / rayCount);
                GameObject ray = CreatePart($"AuraRay_{i}", root, solid, primaryAccent * new Color(1, 1, 1, 0.5f), new Vector2(rayThickness, rayLength), Vector2.zero, order++);
                ray.transform.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // 3. SECONDARY OUTER ORBITAL RINGS (Unlocks Rank 5+)
        if (rank >= 5)
        {
            float ring1Size = baseBorderDiameter * 1.18f;
            CreatePart("OuterEnergyRing", root, thinRing, secondaryAccent * 0.8f, new Vector2(ring1Size, ring1Size), Vector2.zero, order++);

            if (rank >= 9)
            {
                float ring2Size = baseBorderDiameter * 1.32f;
                GameObject secondaryRing = CreatePart("FarEnergyRing", root, thinRing, primaryAccent * 0.6f, new Vector2(ring2Size, ring2Size), Vector2.zero, order++);
                secondaryRing.transform.localRotation = Quaternion.Euler(0, 0, 30f);
            }
        }

        // 4. MAIN BORDER FRAME & DARK BACKING RIM
        float darkBackingSize = baseBorderDiameter + 8f + (rank * 0.8f);
        CreatePart("RimBacking", root, ring, voidDark, new Vector2(darkBackingSize, darkBackingSize), Vector2.zero, order++);

        // Main colored rim frame
        CreatePart("MainBorderRim", root, ring, rimColor, new Vector2(baseBorderDiameter, baseBorderDiameter), Vector2.zero, order++);

        // 5. INNER & OUTER RIM HIGHLIGHTS (Unlocks Rank 2+)
        if (rank >= 2)
        {
            float outerHighlightSize = baseBorderDiameter * 1.05f;
            CreatePart("OuterRimEdge", root, thinRing, primaryAccent, new Vector2(outerHighlightSize, outerHighlightSize), Vector2.zero, order++);
        }

        if (rank >= 4)
        {
            float innerHighlightSize = baseBorderDiameter * 0.88f;
            CreatePart("InnerRimEdge", root, thinRing, crispWhite, new Vector2(innerHighlightSize, innerHighlightSize), Vector2.zero, order++);
        }

        // 6. ORBITING AURA NODES / CAPACITORS ALONG THE BORDER (Unlocks Rank 3+)
        if (rank >= 3)
        {
            int nodeCount = Mathf.Clamp(rank, 3, 12);
            float orbitRadius = baseBorderDiameter / 2f;

            for (int i = 0; i < nodeCount; i++)
            {
                float angle = i * (360f / nodeCount) * Mathf.Deg2Rad;
                Vector2 nodePos = new Vector2(Mathf.Cos(angle) * orbitRadius, Mathf.Sin(angle) * orbitRadius);
                float orbSize = 8f + (rank * 1.2f);

                GameObject orbDark = CreatePart($"AuraNodeDark_{i}", root, circle, voidDark, new Vector2(orbSize + 4f, orbSize + 4f), nodePos, order++);
                CreatePart($"AuraNodeCore_{i}", orbDark.transform, circle, primaryAccent, new Vector2(orbSize, orbSize), Vector2.zero, order++);

                if (rank >= 6)
                {
                    CreatePart($"AuraNodeGlow_{i}", orbDark.transform, glow, secondaryAccent, new Vector2(orbSize * 2f, orbSize * 2f), Vector2.zero, order++);
                    CreatePart($"AuraNodeHighlight_{i}", orbDark.transform, circle, crispWhite, new Vector2(orbSize * 0.4f, orbSize * 0.4f), Vector2.zero, order++);
                }
            }
        }
    }

    private void GetRankColors(int rank, out Color primary, out Color secondary, out Color glow, out Color rim)
    {
        if (rank <= 2) // Tier 1: Soft Cyan / Azure
        {
            primary = new Color(0.00f, 0.85f, 1.00f);
            secondary = new Color(0.10f, 0.45f, 0.75f);
            glow = new Color(0.00f, 0.70f, 1.00f);
            rim = new Color(0.15f, 0.75f, 0.95f);
        }
        else if (rank <= 4) // Tier 2: Amethyst Purple
        {
            primary = new Color(0.70f, 0.35f, 1.00f);
            secondary = new Color(0.40f, 0.15f, 0.70f);
            glow = new Color(0.60f, 0.20f, 0.95f);
            rim = new Color(0.65f, 0.30f, 0.90f);
        }
        else if (rank <= 6) // Tier 3: Crimson Ember
        {
            primary = new Color(1.00f, 0.30f, 0.15f);
            secondary = new Color(0.65f, 0.10f, 0.05f);
            glow = new Color(0.95f, 0.20f, 0.00f);
            rim = new Color(0.90f, 0.35f, 0.10f);
        }
        else if (rank <= 8) // Tier 4: Radiance Emerald
        {
            primary = new Color(0.10f, 1.00f, 0.50f);
            secondary = new Color(0.00f, 0.55f, 0.30f);
            glow = new Color(0.00f, 0.90f, 0.40f);
            rim = new Color(0.20f, 0.95f, 0.55f);
        }
        else // Tier 5 (Ranks 9–10): Celestial Mythic Gold & Magenta
        {
            primary = new Color(1.00f, 0.84f, 0.00f);
            secondary = new Color(1.00f, 0.20f, 0.60f);
            glow = new Color(1.00f, 0.60f, 0.00f);
            rim = new Color(1.00f, 0.80f, 0.20f);
        }
    }

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
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class ResearchIconGeneratorEditor : EditorWindow
{
    private Vector2Int iconSize = new Vector2Int(512, 512);
    private string saveFolderPath = "Assets/ResearchIcons";

    [MenuItem("Tools/2D Sci-Fi Research Icon Generator")]
    public static void ShowWindow()
    {
        GetWindow<ResearchIconGeneratorEditor>("Research Icon Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("2D Idle Spaceship Game - Research Icons Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        iconSize = EditorGUILayout.Vector2IntField("Sprite Dimensions (PX)", iconSize);
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate 5 Sci-Fi Research Icons", GUILayout.Height(40)))
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

        // Procedural primitives
        Sprite solidSprite = CreateFlatSquareSprite();
        Sprite glowSprite = CreateRadialGlowSprite(256);
        Sprite circleSprite = CreateCircleSprite(256);
        Sprite ringSprite = CreateRingSprite(256, 0.82f);
        Sprite thinRingSprite = CreateRingSprite(256, 0.94f);

        string[] iconNames = new string[]
        {
            "Research_01_QuantumComputing",
            "Research_02_PlasmaPropulsion",
            "Research_03_ShieldTech",
            "Research_04_AIMatrix",
            "Research_05_WarpDrive"
        };

        for (int i = 0; i < iconNames.Length; i++)
        {
            int iconId = i + 1;
            byte[] pngData = BakeIconToPNG(iconId, iconSize, solidSprite, glowSprite, circleSprite, ringSprite, thinRingSprite);
            string filePath = Path.Combine(saveFolderPath, $"{iconNames[i]}.png");
            File.WriteAllBytes(filePath, pngData);
        }

        // Clean up temporary procedural textures
        DestroyImmediate(solidSprite.texture); DestroyImmediate(solidSprite);
        DestroyImmediate(glowSprite.texture); DestroyImmediate(glowSprite);
        DestroyImmediate(circleSprite.texture); DestroyImmediate(circleSprite);
        DestroyImmediate(ringSprite.texture); DestroyImmediate(ringSprite);
        DestroyImmediate(thinRingSprite.texture); DestroyImmediate(thinRingSprite);

        AssetDatabase.Refresh();

        // Reimport as Sprites
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

        EditorUtility.DisplayDialog("Success", $"Generated 5 Research Icons inside:\n{saveFolderPath}", "OK");
    }

    private byte[] BakeIconToPNG(int iconId, Vector2Int size, Sprite solid, Sprite glow, Sprite circle, Sprite ring, Sprite thinRing)
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

        BuildResearchIconHierarchy(bakerRoot.transform, iconId, size, solid, glow, circle, ring, thinRing);
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

    private void BuildResearchIconHierarchy(Transform root, int iconId, Vector2Int size, Sprite solid, Sprite glow, Sprite circle, Sprite ring, Sprite thinRing)
    {
        Color primary, secondary, highlight, darkBg;
        GetIconColorScheme(iconId, out primary, out secondary, out highlight, out darkBg);

        int order = 0;
        float baseSize = Mathf.Min(size.x, size.y) * 0.75f;

        // 1. Tech Hex / Circle Background Frame
        CreatePart("Glow_Background", root, glow, primary * new Color(1, 1, 1, 0.4f), new Vector2(baseSize * 1.3f, baseSize * 1.3f), Vector2.zero, order++);
        CreatePart("Backing_Plate", root, circle, darkBg, new Vector2(baseSize, baseSize), Vector2.zero, order++);
        CreatePart("Outer_Tech_Ring", root, ring, secondary, new Vector2(baseSize, baseSize), Vector2.zero, order++);

        // 2. Icon Specific Graphics
        switch (iconId)
        {
            case 1: // Quantum Computing
                BuildQuantumComputingIcon(root, solid, glow, circle, thinRing, primary, highlight, ref order, baseSize);
                break;
            case 2: // Plasma Propulsion
                BuildPlasmaPropulsionIcon(root, solid, glow, circle, thinRing, primary, highlight, ref order, baseSize);
                break;
            case 3: // Shield Tech
                BuildShieldTechIcon(root, solid, glow, circle, thinRing, primary, highlight, ref order, baseSize);
                break;
            case 4: // AI Matrix
                BuildAIMatrixIcon(root, solid, glow, circle, thinRing, primary, highlight, ref order, baseSize);
                break;
            case 5: // Warp Drive
                BuildWarpDriveIcon(root, solid, glow, circle, thinRing, primary, highlight, ref order, baseSize);
                break;
        }

        // 3. Common Foreground Overlay Framing
        CreatePart("Inner_Tech_Rim", root, thinRing, highlight * new Color(1, 1, 1, 0.8f), new Vector2(baseSize * 0.92f, baseSize * 0.92f), Vector2.zero, order++);
    }

    #region Icon Implementations

    private void BuildQuantumComputingIcon(Transform root, Sprite solid, Sprite glow, Sprite circle, Sprite thinRing, Color primary, Color highlight, ref int order, float baseSize)
    {
        // Orbital Atomic Loops
        for (int i = 0; i < 3; i++)
        {
            GameObject ringObj = CreatePart($"Orbit_Ring_{i}", root, thinRing, primary, new Vector2(baseSize * 0.65f, baseSize * 0.22f), Vector2.zero, order++);
            ringObj.transform.localRotation = Quaternion.Euler(0, 0, i * 60f);
        }

        // Center Atom Core
        CreatePart("Core_Glow", root, glow, primary, new Vector2(baseSize * 0.4f, baseSize * 0.4f), Vector2.zero, order++);
        CreatePart("Core_Node", root, circle, highlight, new Vector2(baseSize * 0.15f, baseSize * 0.15f), Vector2.zero, order++);

        // Floating Quantum Particles
        float orbitR = baseSize * 0.28f;
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(angle) * orbitR, Mathf.Sin(angle) * orbitR);
            CreatePart($"Particle_{i}", root, circle, highlight, new Vector2(baseSize * 0.05f, baseSize * 0.05f), pos, order++);
        }
    }

    private void BuildPlasmaPropulsionIcon(Transform root, Sprite solid, Sprite glow, Sprite circle, Sprite thinRing, Color primary, Color highlight, ref int order, float baseSize)
    {
        // Thrust Nozzle Frame
        GameObject nozzle = CreatePart("Nozzle_Base", root, solid, primary, new Vector2(baseSize * 0.35f, baseSize * 0.18f), new Vector2(0, baseSize * 0.2f), order++);
        nozzle.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // Exhaust Beams / Plume
        for (int i = 0; i < 3; i++)
        {
            float width = baseSize * (0.12f - (i * 0.03f));
            float length = baseSize * (0.55f + (i * 0.1f));
            Vector2 pos = new Vector2(0, -baseSize * 0.1f - (i * 10f));

            CreatePart($"Flame_Layer_{i}", root, solid, (i == 0) ? highlight : primary, new Vector2(width, length), pos, order++);
        }

        // Energy Shock Diamond Core
        CreatePart("Flame_Glow", root, glow, primary, new Vector2(baseSize * 0.5f, baseSize * 0.7f), new Vector2(0, -baseSize * 0.15f), order++);
    }

    private void BuildShieldTechIcon(Transform root, Sprite solid, Sprite glow, Sprite circle, Sprite thinRing, Color primary, Color highlight, ref int order, float baseSize)
    {
        // Central Emblem Shield Core
        GameObject shieldTop = CreatePart("Shield_Top", root, solid, primary, new Vector2(baseSize * 0.4f, baseSize * 0.35f), new Vector2(0, baseSize * 0.05f), order++);
        GameObject shieldBottom = CreatePart("Shield_Tip", root, solid, primary, new Vector2(baseSize * 0.3f, baseSize * 0.3f), new Vector2(0, -baseSize * 0.15f), order++);
        shieldBottom.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        // Protective Hex Core Glow
        CreatePart("Shield_Glow", root, glow, primary, new Vector2(baseSize * 0.6f, baseSize * 0.6f), Vector2.zero, order++);

        // Emblem Cross Highlight
        CreatePart("Emblem_V", root, solid, highlight, new Vector2(baseSize * 0.06f, baseSize * 0.3f), Vector2.zero, order++);
        CreatePart("Emblem_H", root, solid, highlight, new Vector2(baseSize * 0.25f, baseSize * 0.06f), new Vector2(0, baseSize * 0.05f), order++);
    }

    private void BuildAIMatrixIcon(Transform root, Sprite solid, Sprite glow, Sprite circle, Sprite thinRing, Color primary, Color highlight, ref int order, float baseSize)
    {
        // Microchip Core Shape
        CreatePart("Chip_Base", root, solid, primary, new Vector2(baseSize * 0.35f, baseSize * 0.35f), Vector2.zero, order++);
        CreatePart("Chip_Inner", root, solid, highlight, new Vector2(baseSize * 0.2f, baseSize * 0.2f), Vector2.zero, order++);

        // Bus Lines / Connectors
        float offset = baseSize * 0.28f;
        int lineCount = 4;
        for (int i = 0; i < lineCount; i++)
        {
            float pos = -baseSize * 0.12f + (i * (baseSize * 0.08f));

            // Top / Bottom Pins
            CreatePart($"Pin_T_{i}", root, solid, primary, new Vector2(baseSize * 0.03f, baseSize * 0.12f), new Vector2(pos, offset), order++);
            CreatePart($"Pin_B_{i}", root, solid, primary, new Vector2(baseSize * 0.03f, baseSize * 0.12f), new Vector2(pos, -offset), order++);

            // Left / Right Pins
            CreatePart($"Pin_L_{i}", root, solid, primary, new Vector2(baseSize * 0.12f, baseSize * 0.03f), new Vector2(-offset, pos), order++);
            CreatePart($"Pin_R_{i}", root, solid, primary, new Vector2(baseSize * 0.12f, baseSize * 0.03f), new Vector2(offset, pos), order++);
        }

        CreatePart("Chip_Glow", root, glow, primary, new Vector2(baseSize * 0.5f, baseSize * 0.5f), Vector2.zero, order++);
    }

    private void BuildWarpDriveIcon(Transform root, Sprite solid, Sprite glow, Sprite circle, Sprite thinRing, Color primary, Color highlight, ref int order, float baseSize)
    {
        // Portal Singularity Center
        CreatePart("Singularity_Core", root, circle, new Color(0.02f, 0.02f, 0.05f, 1f), new Vector2(baseSize * 0.22f, baseSize * 0.22f), Vector2.zero, order++);
        CreatePart("Singularity_Glow", root, glow, primary, new Vector2(baseSize * 0.65f, baseSize * 0.65f), Vector2.zero, order++);

        // Accelerating Vortex Rings
        for (int i = 1; i <= 3; i++)
        {
            float ringScale = baseSize * (0.2f + (i * 0.12f));
            GameObject vRing = CreatePart($"Vortex_Ring_{i}", root, thinRing, highlight * new Color(1, 1, 1, 1f - (i * 0.2f)), new Vector2(ringScale, ringScale), Vector2.zero, order++);
            vRing.transform.localRotation = Quaternion.Euler(0, 0, i * 25f);
        }

        // Warp Ray Bursts
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 45f;
            GameObject ray = CreatePart($"Warp_Ray_{i}", root, solid, highlight, new Vector2(baseSize * 0.03f, baseSize * 0.7f), Vector2.zero, order++);
            ray.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    #endregion

    private void GetIconColorScheme(int iconId, out Color primary, out Color secondary, out Color highlight, out Color darkBg)
    {
        darkBg = new Color(0.03f, 0.05f, 0.10f, 0.9f);
        highlight = new Color(0.98f, 0.98f, 1.0f, 0.95f);

        switch (iconId)
        {
            case 1: // Quantum - Electric Cyan & Violet
                primary = new Color(0.00f, 0.85f, 1.00f);
                secondary = new Color(0.45f, 0.20f, 0.90f);
                break;
            case 2: // Plasma - Fire Orange & Bright Yellow
                primary = new Color(1.00f, 0.40f, 0.00f);
                secondary = new Color(1.00f, 0.75f, 0.00f);
                break;
            case 3: // Shield - Emerald Tech Green
                primary = new Color(0.00f, 0.95f, 0.45f);
                secondary = new Color(0.00f, 0.50f, 0.35f);
                break;
            case 4: // AI Matrix - Crimson & Deep Magenta
                primary = new Color(1.00f, 0.15f, 0.35f);
                secondary = new Color(0.60f, 0.00f, 0.40f);
                break;
            case 5: // Warp Drive - Celestial Gold & Deep Blue
            default:
                primary = new Color(0.20f, 0.55f, 1.00f);
                secondary = new Color(1.00f, 0.80f, 0.10f);
                break;
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
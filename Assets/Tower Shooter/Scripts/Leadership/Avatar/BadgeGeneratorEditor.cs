#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class AugmentBehaviorIconGeneratorEditor : EditorWindow
{
    private Vector2Int iconSize = new Vector2Int(512, 512);
    private string saveFolderPath = "Assets/AugmentIcons_Behavioral";

    [MenuItem("Tools/Generate 20 Augment Behavior Icons")]
    public static void ShowWindow()
    {
        GetWindow<AugmentBehaviorIconGeneratorEditor>("Augment Behavior Icon Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Flat Vector Augment Behavior Icon Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        iconSize = EditorGUILayout.Vector2IntField("Sprite Dimensions (PX)", iconSize);
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate 20 Augment Behavior Icons", GUILayout.Height(40)))
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
            "Augment_01_CyclePulse",       // OnCycleComplete
            "Augment_02_CritSpark",        // OnCrit
            "Augment_03_ClockCadence",     // EveryXCycles
            "Augment_04_StaticLock",       // Permanently
            "Augment_05_IdleEngine",       // WhenIdle
            "Augment_06_ThresholdGate",    // StatCrossesThreshold / ThresholdGate
            "Augment_07_StatAdd",          // Add
            "Augment_08_MultiplierFlare",  // Multiply
            "Augment_09_TransmuteConvert", // Convert
            "Augment_10_StatSwap",         // Swap
            "Augment_11_RootExtractor",    // SquareRoot
            "Augment_12_LogCurve",         // Logarithm
            "Augment_13_EchoCopy",         // Copy
            "Augment_14_SelfCore",         // Self
            "Augment_15_AdjacentLink",     // AdjacentNodes
            "Augment_16_GlobalGrid",       // Global
            "Augment_17_TemporaryBurst",   // TemporaryBurst
            "Augment_18_StackingPyramid",  // Stacking
            "Augment_19_DecayingHalfLife", // Decaying
            "Augment_20_InverseFlip"       // Inverse Scaling
        };

        for (int i = 0; i < iconNames.Length; i++)
        {
            byte[] pngData = BakeIconToPNG(i + 1, iconSize, solidSprite, circleSprite, ringSprite);
            string filePath = Path.Combine(saveFolderPath, $"{iconNames[i]}.png");
            File.WriteAllBytes(filePath, pngData);
        }

        // Cleanup temporary procedural sprites
        DestroyImmediate(solidSprite.texture); DestroyImmediate(solidSprite);
        DestroyImmediate(circleSprite.texture); DestroyImmediate(circleSprite);
        DestroyImmediate(ringSprite.texture); DestroyImmediate(ringSprite);

        AssetDatabase.Refresh();

        // Reimport generated PNG files as 2D Sprites
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

        EditorUtility.DisplayDialog("Success", $"Generated 20 Augment Behavior Icons inside:\n{saveFolderPath}", "OK");
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
        bakerCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent background
        bakerCamera.cullingMask = ~0;
        bakerCamera.orthographic = true;
        bakerCamera.orthographicSize = size.y / 2f;
        bakerCamera.nearClipPlane = 0.01f;
        bakerCamera.farClipPlane = 1000f;

        cameraObj.transform.position = new Vector3(0, 0, -100f);
        cameraObj.transform.LookAt(Vector3.zero);

        bakerCanvas.worldCamera = bakerCamera;

        BuildBehaviorIconHierarchy(bakerRoot.transform, iconId, size, solid, circle, ring);
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

    private void BuildBehaviorIconHierarchy(Transform root, int iconId, Vector2Int size, Sprite solid, Sprite circle, Sprite ring)
    {
        // Color Palette matching UI dark-space theme
        Color uiCyan = new Color(0.00f, 0.85f, 1.00f, 1.00f);       // Primary Neon Cyan
        Color uiMagenta = new Color(0.85f, 0.20f, 0.85f, 1.00f);    // Secondary Neon Magenta
        Color uiWhite = new Color(1.00f, 1.00f, 1.00f, 1.00f);      // Clean White
        Color uiDarkNavy = new Color(0.07f, 0.09f, 0.16f, 1.00f);   // Card Panel Backing
        Color uiBorderNavy = new Color(0.15f, 0.22f, 0.35f, 1.00f); // Dark Rim Border

        int order = 0;
        float baseSize = Mathf.Min(size.x, size.y) * 0.72f;

        // Base Container Backplate & Border Rings
        CreatePart("Badge_Backing", root, circle, uiDarkNavy, new Vector2(baseSize, baseSize), Vector2.zero, order++);
        CreatePart("Badge_Border", root, ring, uiBorderNavy, new Vector2(baseSize, baseSize), Vector2.zero, order++);
        CreatePart("Badge_CyanGlowRing", root, ring, uiCyan * new Color(1, 1, 1, 0.35f), new Vector2(baseSize * 0.94f, baseSize * 0.94f), Vector2.zero, order++);

        switch (iconId)
        {
            case 1:  // OnCycleComplete
                BuildCyclePulseIcon(root, solid, circle, ring, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 2:  // OnCrit
                BuildCritSparkIcon(root, solid, circle, uiWhite, uiMagenta, ref order, baseSize);
                break;
            case 3:  // EveryXCycles
                BuildClockCadenceIcon(root, solid, circle, ring, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 4:  // Permanently
                BuildStaticLockIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 5:  // WhenIdle
                BuildIdleEngineIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 6:  // StatCrossesThreshold / ThresholdGate
                BuildThresholdGateIcon(root, solid, circle, uiWhite, uiMagenta, ref order, baseSize);
                break;
            case 7:  // Add
                BuildStatAddIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 8:  // Multiply
                BuildMultiplierFlareIcon(root, solid, circle, uiWhite, uiMagenta, ref order, baseSize);
                break;
            case 9:  // Convert
                BuildTransmuteConvertIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 10: // Swap
                BuildStatSwapIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 11: // SquareRoot
                BuildRootExtractorIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 12: // Logarithm
                BuildLogCurveIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 13: // Copy
                BuildEchoCopyIcon(root, solid, circle, uiWhite, uiMagenta, ref order, baseSize);
                break;
            case 14: // Scope: Self
                BuildSelfCoreIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 15: // Scope: AdjacentNodes
                BuildAdjacentLinkIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 16: // Scope: Global
                BuildGlobalGridIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 17: // TemporaryBurst
                BuildTemporaryBurstIcon(root, solid, circle, ring, uiWhite, uiMagenta, ref order, baseSize);
                break;
            case 18: // Stacking
                BuildStackingPyramidIcon(root, solid, circle, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 19: // Decaying
                BuildDecayingHalfLifeIcon(root, solid, circle, ring, uiWhite, uiCyan, ref order, baseSize);
                break;
            case 20: // Inverse
                BuildInverseFlipIcon(root, solid, circle, uiWhite, uiMagenta, ref order, baseSize);
                break;
        }
    }

    #region Icon Rendering Implementations

    private void BuildCyclePulseIcon(Transform root, Sprite solid, Sprite circle, Sprite ring, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Cycle_OuterRing", root, ring, cyan, new Vector2(baseSize * 0.45f, baseSize * 0.45f), Vector2.zero, order++);
        CreatePart("Cycle_CenterCore", root, circle, white, new Vector2(baseSize * 0.14f, baseSize * 0.14f), Vector2.zero, order++);
        
        for (int i = 0; i < 3; i++)
        {
            float ang = i * 120f;
            Vector2 pos = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)) * (baseSize * 0.18f);
            GameObject dot = CreatePart($"Cycle_Dot_{i}", root, circle, white, new Vector2(baseSize * 0.05f, baseSize * 0.05f), pos, order++);
        }
    }

    private void BuildCritSparkIcon(Transform root, Sprite solid, Sprite circle, Color white, Color magenta, ref int order, float baseSize)
    {
        GameObject diamond = CreatePart("Crit_Core", root, solid, white, new Vector2(baseSize * 0.16f, baseSize * 0.16f), Vector2.zero, order++);
        diamond.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        for (int i = 0; i < 4; i++)
        {
            GameObject spike = CreatePart($"Crit_Spike_{i}", root, solid, magenta, new Vector2(baseSize * 0.06f, baseSize * 0.42f), Vector2.zero, order++);
            spike.transform.localRotation = Quaternion.Euler(0, 0, i * 45f);
        }
    }

    private void BuildClockCadenceIcon(Transform root, Sprite solid, Sprite circle, Sprite ring, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Clock_Ring", root, ring, white, new Vector2(baseSize * 0.42f, baseSize * 0.42f), Vector2.zero, order++);
        CreatePart("Clock_Center", root, circle, cyan, new Vector2(baseSize * 0.08f, baseSize * 0.08f), Vector2.zero, order++);
        
        CreatePart("Hand_H", root, solid, cyan, new Vector2(baseSize * 0.04f, baseSize * 0.16f), new Vector2(0, baseSize * 0.06f), order++);
        CreatePart("Hand_M", root, solid, cyan, new Vector2(baseSize * 0.14f, baseSize * 0.04f), new Vector2(baseSize * 0.05f, 0), order++);
    }

    private void BuildStaticLockIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Lock_Shackle", root, solid, white, new Vector2(baseSize * 0.18f, baseSize * 0.18f), new Vector2(0, baseSize * 0.08f), order++);
        CreatePart("Lock_Shackle_Cut", root, solid, new Color(0.07f, 0.09f, 0.16f), new Vector2(baseSize * 0.10f, baseSize * 0.12f), new Vector2(0, baseSize * 0.08f), order++);
        CreatePart("Lock_Body", root, solid, cyan, new Vector2(baseSize * 0.26f, baseSize * 0.22f), new Vector2(0, -baseSize * 0.06f), order++);
    }

    private void BuildIdleEngineIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Idle_Core", root, circle, white, new Vector2(baseSize * 0.20f, baseSize * 0.20f), Vector2.zero, order++);
        CreatePart("Bar_Top", root, solid, cyan, new Vector2(baseSize * 0.24f, baseSize * 0.04f), new Vector2(0, baseSize * 0.18f), order++);
        CreatePart("Bar_Bot", root, solid, cyan, new Vector2(baseSize * 0.24f, baseSize * 0.04f), new Vector2(0, -baseSize * 0.18f), order++);
    }

    private void BuildThresholdGateIcon(Transform root, Sprite solid, Sprite circle, Color white, Color magenta, ref int order, float baseSize)
    {
        CreatePart("Gate_Line", root, solid, white, new Vector2(baseSize * 0.40f, baseSize * 0.04f), Vector2.zero, order++);
        
        GameObject arrow = CreatePart("Gate_Arrow", root, solid, magenta, new Vector2(baseSize * 0.12f, baseSize * 0.24f), new Vector2(0, baseSize * 0.10f), order++);
        arrow.transform.localRotation = Quaternion.Euler(0, 0, 0);

        CreatePart("Gate_BaseDot", root, circle, magenta, new Vector2(baseSize * 0.08f, baseSize * 0.08f), new Vector2(0, -baseSize * 0.12f), order++);
    }

    private void BuildStatAddIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Add_Core", root, circle, cyan, new Vector2(baseSize * 0.26f, baseSize * 0.26f), Vector2.zero, order++);
        CreatePart("Add_V", root, solid, white, new Vector2(baseSize * 0.06f, baseSize * 0.24f), Vector2.zero, order++);
        CreatePart("Add_H", root, solid, white, new Vector2(baseSize * 0.24f, baseSize * 0.06f), Vector2.zero, order++);
    }

    private void BuildMultiplierFlareIcon(Transform root, Sprite solid, Sprite circle, Color white, Color magenta, ref int order, float baseSize)
    {
        GameObject v = CreatePart("Mul_V", root, solid, magenta, new Vector2(baseSize * 0.06f, baseSize * 0.32f), Vector2.zero, order++);
        v.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        GameObject h = CreatePart("Mul_H", root, solid, magenta, new Vector2(baseSize * 0.06f, baseSize * 0.32f), Vector2.zero, order++);
        h.transform.localRotation = Quaternion.Euler(0, 0, -45f);

        CreatePart("Mul_Center", root, circle, white, new Vector2(baseSize * 0.10f, baseSize * 0.10f), Vector2.zero, order++);
    }

    private void BuildTransmuteConvertIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        GameObject arrow1 = CreatePart("Arrow_1", root, solid, cyan, new Vector2(baseSize * 0.14f, baseSize * 0.14f), new Vector2(-baseSize * 0.08f, baseSize * 0.08f), order++);
        arrow1.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        GameObject arrow2 = CreatePart("Arrow_2", root, solid, white, new Vector2(baseSize * 0.14f, baseSize * 0.14f), new Vector2(baseSize * 0.08f, -baseSize * 0.08f), order++);
        arrow2.transform.localRotation = Quaternion.Euler(0, 0, -135f);

        CreatePart("Link_Line", root, solid, cyan, new Vector2(baseSize * 0.28f, baseSize * 0.04f), Vector2.zero, order++).transform.localRotation = Quaternion.Euler(0, 0, -45f);
    }

    private void BuildStatSwapIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Node_Left", root, circle, white, new Vector2(baseSize * 0.14f, baseSize * 0.14f), new Vector2(-baseSize * 0.14f, 0), order++);
        CreatePart("Node_Right", root, circle, cyan, new Vector2(baseSize * 0.14f, baseSize * 0.14f), new Vector2(baseSize * 0.14f, 0), order++);

        CreatePart("Swap_TopLine", root, solid, cyan, new Vector2(baseSize * 0.24f, baseSize * 0.03f), new Vector2(0, baseSize * 0.10f), order++);
        CreatePart("Swap_BotLine", root, solid, cyan, new Vector2(baseSize * 0.24f, baseSize * 0.03f), new Vector2(0, -baseSize * 0.10f), order++);
    }

    private void BuildRootExtractorIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Root_V1", root, solid, white, new Vector2(baseSize * 0.04f, baseSize * 0.12f), new Vector2(-baseSize * 0.14f, -baseSize * 0.04f), order++).transform.localRotation = Quaternion.Euler(0, 0, -20f);
        CreatePart("Root_V2", root, solid, white, new Vector2(baseSize * 0.04f, baseSize * 0.32f), new Vector2(-baseSize * 0.06f, 0f), order++).transform.localRotation = Quaternion.Euler(0, 0, 20f);
        CreatePart("Root_H", root, solid, white, new Vector2(baseSize * 0.22f, baseSize * 0.04f), new Vector2(0.06f * baseSize, baseSize * 0.14f), order++);

        CreatePart("Root_Core", root, circle, cyan, new Vector2(baseSize * 0.14f, baseSize * 0.14f), new Vector2(baseSize * 0.06f, -baseSize * 0.04f), order++);
    }

    private void BuildLogCurveIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Axis_V", root, solid, white, new Vector2(baseSize * 0.03f, baseSize * 0.32f), new Vector2(-baseSize * 0.12f, 0), order++);
        CreatePart("Axis_H", root, solid, white, new Vector2(baseSize * 0.32f, baseSize * 0.03f), new Vector2(0, -baseSize * 0.12f), order++);

        for (int i = 0; i < 5; i++)
        {
            float t = i / 4f;
            float x = Mathf.Lerp(-baseSize * 0.10f, baseSize * 0.14f, t);
            float y = Mathf.Log10(1 + t * 9) * (baseSize * 0.22f) - (baseSize * 0.10f);
            CreatePart($"Curve_Dot_{i}", root, circle, cyan, new Vector2(baseSize * 0.06f, baseSize * 0.06f), new Vector2(x, y), order++);
        }
    }

    private void BuildEchoCopyIcon(Transform root, Sprite solid, Sprite circle, Color white, Color magenta, ref int order, float baseSize)
    {
        CreatePart("Copy_Back", root, solid, magenta * new Color(1, 1, 1, 0.5f), new Vector2(baseSize * 0.18f, baseSize * 0.18f), new Vector2(-baseSize * 0.06f, baseSize * 0.06f), order++).transform.localRotation = Quaternion.Euler(0, 0, 45f);
        CreatePart("Copy_Front", root, solid, white, new Vector2(baseSize * 0.18f, baseSize * 0.18f), new Vector2(baseSize * 0.04f, -baseSize * 0.04f), order++).transform.localRotation = Quaternion.Euler(0, 0, 45f);
    }

    private void BuildSelfCoreIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        GameObject diamond = CreatePart("Self_Node", root, solid, white, new Vector2(baseSize * 0.24f, baseSize * 0.24f), Vector2.zero, order++);
        diamond.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        CreatePart("Self_Core", root, circle, cyan, new Vector2(baseSize * 0.12f, baseSize * 0.12f), Vector2.zero, order++);
    }

    private void BuildAdjacentLinkIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        Vector2 left = new Vector2(-baseSize * 0.12f, 0);
        Vector2 right = new Vector2(baseSize * 0.12f, 0);

        CreatePart("Bridge", root, solid, cyan, new Vector2(baseSize * 0.24f, baseSize * 0.05f), Vector2.zero, order++);
        CreatePart("Node_L", root, circle, white, new Vector2(baseSize * 0.14f, baseSize * 0.14f), left, order++);
        CreatePart("Node_R", root, circle, white, new Vector2(baseSize * 0.14f, baseSize * 0.14f), right, order++);
    }

    private void BuildGlobalGridIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Grid_Cross_V", root, solid, cyan * new Color(1, 1, 1, 0.4f), new Vector2(baseSize * 0.03f, baseSize * 0.36f), Vector2.zero, order++);
        CreatePart("Grid_Cross_H", root, solid, cyan * new Color(1, 1, 1, 0.4f), new Vector2(baseSize * 0.36f, baseSize * 0.03f), Vector2.zero, order++);

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                Vector2 pos = new Vector2(x * baseSize * 0.14f, y * baseSize * 0.14f);
                CreatePart($"Grid_Node_{x}_{y}", root, circle, white, new Vector2(baseSize * 0.07f, baseSize * 0.07f), pos, order++);
            }
        }
    }

    private void BuildTemporaryBurstIcon(Transform root, Sprite solid, Sprite circle, Sprite ring, Color white, Color magenta, ref int order, float baseSize)
    {
        CreatePart("Burst_Ring", root, ring, magenta, new Vector2(baseSize * 0.40f, baseSize * 0.40f), Vector2.zero, order++);
        CreatePart("Burst_Center", root, circle, white, new Vector2(baseSize * 0.12f, baseSize * 0.12f), Vector2.zero, order++);

        for (int i = 0; i < 4; i++)
        {
            GameObject ray = CreatePart($"Ray_{i}", root, solid, magenta, new Vector2(baseSize * 0.04f, baseSize * 0.28f), Vector2.zero, order++);
            ray.transform.localRotation = Quaternion.Euler(0, 0, i * 45f);
        }
    }

    private void BuildStackingPyramidIcon(Transform root, Sprite solid, Sprite circle, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Stack_1", root, solid, cyan, new Vector2(baseSize * 0.30f, baseSize * 0.05f), new Vector2(0, -baseSize * 0.10f), order++);
        CreatePart("Stack_2", root, solid, cyan, new Vector2(baseSize * 0.22f, baseSize * 0.05f), new Vector2(0, -baseSize * 0.02f), order++);
        CreatePart("Stack_3", root, solid, white, new Vector2(baseSize * 0.14f, baseSize * 0.05f), new Vector2(0, baseSize * 0.06f), order++);
    }

    private void BuildDecayingHalfLifeIcon(Transform root, Sprite solid, Sprite circle, Sprite ring, Color white, Color cyan, ref int order, float baseSize)
    {
        CreatePart("Decay_Ring_Outer", root, ring, cyan * new Color(1, 1, 1, 0.3f), new Vector2(baseSize * 0.42f, baseSize * 0.42f), Vector2.zero, order++);
        CreatePart("Decay_Ring_Mid", root, ring, cyan * new Color(1, 1, 1, 0.6f), new Vector2(baseSize * 0.28f, baseSize * 0.28f), Vector2.zero, order++);
        CreatePart("Decay_Core", root, circle, white, new Vector2(baseSize * 0.10f, baseSize * 0.10f), Vector2.zero, order++);
    }

    private void BuildInverseFlipIcon(Transform root, Sprite solid, Sprite circle, Color white, Color magenta, ref int order, float baseSize)
    {
        GameObject top = CreatePart("Inv_Top", root, solid, magenta, new Vector2(baseSize * 0.12f, baseSize * 0.20f), new Vector2(-baseSize * 0.06f, baseSize * 0.06f), order++);
        top.transform.localRotation = Quaternion.Euler(0, 0, 0);

        GameObject bot = CreatePart("Inv_Bot", root, solid, white, new Vector2(baseSize * 0.12f, baseSize * 0.20f), new Vector2(baseSize * 0.06f, -baseSize * 0.06f), order++);
        bot.transform.localRotation = Quaternion.Euler(0, 0, 180f);
    }

    #endregion

    #region Texture Helpers

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
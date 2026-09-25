using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
public class ProceduralHorizontalRankBadgeUI_Cached : MonoBehaviour
{
    [Header("Rank Settings")]
    [Range(1, 10)]
    public int rankLevel = 1;

    [Header("Layout & Sizing")]
    public Vector2Int badgeTextureSize = new Vector2Int(600, 100);
    public float cornerRadius = 16f;

    [Header("Optimization")]
    [Tooltip("If true, reuses cached rank textures across multiple badge instances.")]
    public bool enableGlobalCaching = true;

    // Static runtime texture cache across all instances (Rank Level -> Baked Sprite)
    private static Dictionary<int, Sprite> spriteCache = new Dictionary<int, Sprite>();

    // Dynamic procedural sprite base
    private Sprite roundedBaseSprite;
    private Image targetImage;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    private void Start()
    {
        SetRankLevel(rankLevel);
    }

    private void OnValidate()
    {
        if (Application.isPlaying && targetImage != null)
        {
            SetRankLevel(rankLevel);
        }
    }

    public void SetRankLevel(int level)
    {
        rankLevel = Mathf.Clamp(level, 1, 10);

        // 1. Check runtime cache first (0 Draw Call / Performance overhead)
        if (enableGlobalCaching && spriteCache.TryGetValue(rankLevel, out Sprite cachedSprite))
        {
            if (cachedSprite != null)
            {
                ApplyCachedSprite(cachedSprite);
                return;
            }
        }

        // 2. Bake texture if not cached
        Sprite newSprite = BakeBadgeToSprite(rankLevel);

        if (enableGlobalCaching && newSprite != null)
        {
            spriteCache[rankLevel] = newSprite;
        }

        ApplyCachedSprite(newSprite);
    }

    /// <summary>
    /// Clears the global sprite cache (useful if resolution or visual settings change at runtime).
    /// </summary>
    public static void ClearCache()
    {
        foreach (var pair in spriteCache)
        {
            if (pair.Value != null && pair.Value.texture != null)
            {
                Destroy(pair.Value.texture);
                Destroy(pair.Value);
            }
        }
        spriteCache.Clear();
    }

    private void ApplyCachedSprite(Sprite sprite)
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        targetImage.sprite = sprite;
        targetImage.type = Image.Type.Simple;

        // Match RectTransform size to texture dimensions
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(badgeTextureSize.x, badgeTextureSize.y);
    }

    private Sprite BakeBadgeToSprite(int level)
    {
        if (roundedBaseSprite == null)
        {
            roundedBaseSprite = CreateRoundedSquareSprite();
        }

        // --- 1. Create Temporary off-screen Canvas Hierarchy ---
        GameObject bakerRoot = new GameObject("TempBadgeBaker", typeof(RectTransform), typeof(Canvas));
        Canvas bakerCanvas = bakerRoot.GetComponent<Canvas>();
        bakerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        RectTransform rootRect = bakerRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = badgeTextureSize;

        // Build procedural hierarchy inside baker root
        BuildBadgeHierarchy(bakerRoot.transform, level);

        // Force UI layout rebuild
        Canvas.ForceUpdateCanvases();

        // --- 2. Setup Render Camera & RenderTexture ---
        GameObject cameraObj = new GameObject("TempBakerCamera", typeof(Camera));
        Camera bakerCamera = cameraObj.GetComponent<Camera>();
        bakerCamera.clearFlags = CameraClearFlags.SolidColor;
        bakerCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent background
        bakerCamera.cullingMask = 1 << bakerRoot.layer;
        bakerCamera.orthographic = true;
        bakerCamera.orthographicSize = badgeTextureSize.y / 2f;
        bakerCamera.nearClipPlane = -100f;
        bakerCamera.farClipPlane = 100f;

        RenderTexture renderTex = RenderTexture.GetTemporary(badgeTextureSize.x, badgeTextureSize.y, 24, RenderTextureFormat.ARGB32);
        bakerCamera.targetTexture = renderTex;

        // --- 3. Render and Extract Texture ---
        bakerCamera.Render();

        RenderTexture.active = renderTex;
        Texture2D resultTex = new Texture2D(badgeTextureSize.x, badgeTextureSize.y, TextureFormat.RGBA32, false);
        resultTex.ReadPixels(new Rect(0, 0, badgeTextureSize.x, badgeTextureSize.y), 0, 0);
        resultTex.Apply();

        RenderTexture.active = null;
        bakerCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTex);

        // --- 4. Cleanup Off-screen GameObjects ---
        DestroyImmediate(bakerRoot);
        DestroyImmediate(cameraObj);

        // --- 5. Generate and Return Final Sprite ---
        return Sprite.Create(resultTex, new Rect(0, 0, badgeTextureSize.x, badgeTextureSize.y), new Vector2(0.5f, 0.5f), 100f);
    }

    #region Procedural Visual Builders

    private void BuildBadgeHierarchy(Transform root, int level)
    {
        Color primaryColor, secondaryColor, plateColor, glowColor, gemColor;
        SetupTierColors(level, out primaryColor, out secondaryColor, out plateColor, out glowColor, out gemColor);

        float w = badgeTextureSize.x;
        float h = badgeTextureSize.y;
        int order = 0;

        // 1. Halo Aura (Level 7+)
        if (level >= 7)
        {
            float extraSize = (level >= 9) ? 40f : 20f;
            CreatePart("HaloAura", root, roundedBaseSprite, glowColor, new Vector2(w + extraSize, h + extraSize), Vector2.zero, order++);
        }

        // 2. Wings (Level 3+)
        if (level >= 3)
        {
            float wingW = (level >= 8) ? 60f : 40f;
            float wingH = h * 0.7f;
            GameObject leftWing = CreatePart("LeftWing", root, roundedBaseSprite, secondaryColor, new Vector2(wingW, wingH), new Vector2(-(w / 2f + wingW / 3f), 0), order++);
            GameObject rightWing = CreatePart("RightWing", root, roundedBaseSprite, secondaryColor, new Vector2(wingW, wingH), new Vector2((w / 2f + wingW / 3f), 0), order++);

            leftWing.transform.localRotation = Quaternion.Euler(0, 0, 15f);
            rightWing.transform.localRotation = Quaternion.Euler(0, 0, -15f);
        }

        // 3. Base Outer Frame & Inner Plate
        GameObject outerFrame = CreatePart("OuterFrame", root, roundedBaseSprite, primaryColor, badgeTextureSize, Vector2.zero, order++);
        GameObject innerPlate = CreatePart("InnerPlate", outerFrame.transform, roundedBaseSprite, plateColor, new Vector2(w - 12f, h - 12f), Vector2.zero, order++);

        // 4. Slanted Progress Chevrons
        float crestSize = h * 0.75f;
        float crestX = -(w / 2f) + (crestSize / 2f) + 16f;
        float chevronsStartX = crestX + (crestSize / 2f) + 24f;
        int chevronCount = Mathf.Clamp((level - 1) % 4 + 1, 1, 4);

        for (int i = 0; i < chevronCount; i++)
        {
            float posX = chevronsStartX + (i * 22f);
            GameObject chevron = CreatePart($"Chevron_{i}", innerPlate.transform, roundedBaseSprite, secondaryColor, new Vector2(12f, h * 0.5f), new Vector2(posX, 0), order++);
            chevron.transform.localRotation = Quaternion.Euler(0, 0, 15f);
        }

        // 5. Embedded Icon Crest (Left Side)
        GameObject rankIconFrame = CreatePart("RankIconFrame", innerPlate.transform, roundedBaseSprite, primaryColor, new Vector2(crestSize, crestSize), new Vector2(crestX, 0), order++);
        rankIconFrame.transform.localRotation = Quaternion.Euler(0, 0, 45f);

        if (level >= 5)
        {
            float gemSize = crestSize * 0.55f;
            CreatePart("CoreGem", rankIconFrame.transform, roundedBaseSprite, gemColor, new Vector2(gemSize, gemSize), Vector2.zero, order++);
        }

        // 6. Crown Accent (Level 9+)
        if (level >= 9)
        {
            CreatePart("CrownAccent", outerFrame.transform, roundedBaseSprite, primaryColor, new Vector2(crestSize * 0.7f, 16f), new Vector2(crestX, h / 2f + 8f), order++);
        }

        // 7. Right Side Star Insignias
        int starCount = (level <= 2) ? 1 : (level <= 4) ? 2 : (level <= 6) ? 3 : (level <= 8) ? 4 : 5;
        float starsStartX = (w / 2f) - 40f;

        for (int i = 0; i < starCount; i++)
        {
            float posX = starsStartX - (i * 28f);
            GameObject star = CreatePart($"StarInsignia_{i}", innerPlate.transform, roundedBaseSprite, primaryColor, new Vector2(18f, 18f), new Vector2(posX, 0), order++);
            star.transform.localRotation = Quaternion.Euler(0, 0, 45f);
        }
    }

    private void SetupTierColors(int level, out Color primary, out Color secondary, out Color plate, out Color glow, out Color gem)
    {
        if (level <= 2)
        {
            primary = new Color(0.68f, 0.44f, 0.25f); secondary = new Color(0.45f, 0.28f, 0.15f);
            plate = new Color(0.16f, 0.14f, 0.14f); glow = new Color(0.80f, 0.50f, 0.20f, 0.20f); gem = new Color(0.75f, 0.38f, 0.18f);
        }
        else if (level <= 4)
        {
            primary = new Color(0.78f, 0.82f, 0.88f); secondary = new Color(0.42f, 0.48f, 0.56f);
            plate = new Color(0.12f, 0.15f, 0.18f); glow = new Color(0.40f, 0.70f, 1.00f, 0.35f); gem = new Color(0.20f, 0.65f, 0.95f);
        }
        else if (level <= 6)
        {
            primary = new Color(0.98f, 0.78f, 0.18f); secondary = new Color(0.68f, 0.48f, 0.08f);
            plate = new Color(0.18f, 0.14f, 0.08f); glow = new Color(1.00f, 0.80f, 0.20f, 0.50f); gem = new Color(0.95f, 0.20f, 0.15f);
        }
        else if (level <= 8)
        {
            primary = new Color(0.85f, 0.95f, 0.98f); secondary = new Color(0.20f, 0.58f, 0.68f);
            plate = new Color(0.08f, 0.12f, 0.16f); glow = new Color(0.10f, 0.95f, 0.98f, 0.65f); gem = new Color(0.00f, 0.90f, 0.90f);
        }
        else
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

    private Sprite CreateRoundedSquareSprite()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        float cornerR = cornerRadius;
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

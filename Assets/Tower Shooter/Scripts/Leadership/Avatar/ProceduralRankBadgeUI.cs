using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class ProceduralHorizontalRankBadgeUI : MonoBehaviour
{
    [Header("Rank Settings")]
    [Range(1, 10)]
    public int rankLevel = 1;

    [Header("Layout & Sizing")]
    public Vector2 badgeSize = new Vector2(600f, 100f);
    public float cornerRadius = 16f;

    // Procedural Rounded Texture
    private Sprite badgeSprite;

    // Structural UI Components
    private GameObject haloAura;
    private GameObject outerFrame;
    private GameObject innerPlate;
    private GameObject leftWing, rightWing;
    private GameObject rankIconFrame;
    private GameObject coreGem;
    private GameObject levelTextContainer;
    private GameObject crownAccent;

    private List<GameObject> progressChevrons = new List<GameObject>();
    private List<GameObject> starInsignias = new List<GameObject>();

    // Color Palette Variables
    private Color primaryColor;
    private Color secondaryColor;
    private Color plateColor;
    private Color glowColor;
    private Color gemColor;

    private void Start()
    {
        badgeSprite = CreateRoundedSquareSprite();
        BuildBadgeHierarchy();
        UpdateBadge(rankLevel);
    }

    private void OnValidate()
    {
        if (outerFrame != null)
        {
            UpdateBadge(rankLevel);
        }
    }

    [ContextMenu("Rebuild & Refresh Badge")]
    public void RefreshBadge()
    {
        if (badgeSprite == null)
        {
            badgeSprite = CreateRoundedSquareSprite();
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        progressChevrons.Clear();
        starInsignias.Clear();
        BuildBadgeHierarchy();
        UpdateBadge(rankLevel);
    }

    public void SetRankLevel(int level)
    {
        rankLevel = Mathf.Clamp(level, 1, 10);
        UpdateBadge(rankLevel);
    }

    private void SetupTierColors(int level)
    {
        if (level <= 2) // Tier 1: Cadet (Bronze/Copper)
        {
            primaryColor = new Color(0.68f, 0.44f, 0.25f);
            secondaryColor = new Color(0.45f, 0.28f, 0.15f);
            plateColor = new Color(0.16f, 0.14f, 0.14f);
            glowColor = new Color(0.80f, 0.50f, 0.20f, 0.20f);
            gemColor = new Color(0.75f, 0.38f, 0.18f);
        }
        else if (level <= 4) // Tier 2: Officer (Silver/Steel)
        {
            primaryColor = new Color(0.78f, 0.82f, 0.88f);
            secondaryColor = new Color(0.42f, 0.48f, 0.56f);
            plateColor = new Color(0.12f, 0.15f, 0.18f);
            glowColor = new Color(0.40f, 0.70f, 1.00f, 0.35f);
            gemColor = new Color(0.20f, 0.65f, 0.95f);
        }
        else if (level <= 6) // Tier 3: Elite (Gold/Amber)
        {
            primaryColor = new Color(0.98f, 0.78f, 0.18f);
            secondaryColor = new Color(0.68f, 0.48f, 0.08f);
            plateColor = new Color(0.18f, 0.14f, 0.08f);
            glowColor = new Color(1.00f, 0.80f, 0.20f, 0.50f);
            gemColor = new Color(0.95f, 0.20f, 0.15f);
        }
        else if (level <= 8) // Tier 4: Master (Platinum/Cyan Glow)
        {
            primaryColor = new Color(0.85f, 0.95f, 0.98f);
            secondaryColor = new Color(0.20f, 0.58f, 0.68f);
            plateColor = new Color(0.08f, 0.12f, 0.16f);
            glowColor = new Color(0.10f, 0.95f, 0.98f, 0.65f);
            gemColor = new Color(0.00f, 0.90f, 0.90f);
        }
        else // Tier 5: Legend (Imperial Gold & Obsidian / Radiant Halo)
        {
            primaryColor = new Color(1.00f, 0.84f, 0.00f);
            secondaryColor = new Color(0.88f, 0.12f, 0.22f);
            plateColor = new Color(0.08f, 0.05f, 0.09f);
            glowColor = new Color(0.95f, 0.25f, 0.95f, 0.80f);
            gemColor = new Color(0.80f, 0.10f, 1.00f);
        }
    }

    private void BuildBadgeHierarchy()
    {
        int order = 0;

        // Back-to-front rendering order
        haloAura = CreatePart("HaloAura", transform, badgeSprite, glowColor, order++);

        leftWing = CreatePart("LeftWing", transform, badgeSprite, secondaryColor, order++);
        rightWing = CreatePart("RightWing", transform, badgeSprite, secondaryColor, order++);

        outerFrame = CreatePart("OuterFrame", transform, badgeSprite, primaryColor, order++);
        innerPlate = CreatePart("InnerPlate", outerFrame.transform, badgeSprite, plateColor, order++);

        // Structural Chevrons inside the horizontal bar
        for (int i = 0; i < 4; i++)
        {
            GameObject chevron = CreatePart($"Chevron_{i}", innerPlate.transform, badgeSprite, secondaryColor, order++);
            progressChevrons.Add(chevron);
        }

        // Left Crest / Icon Unit
        rankIconFrame = CreatePart("RankIconFrame", innerPlate.transform, badgeSprite, primaryColor, order++);
        coreGem = CreatePart("CoreGem", rankIconFrame.transform, badgeSprite, gemColor, order++);

        // Crown Accent at top
        crownAccent = CreatePart("CrownAccent", outerFrame.transform, badgeSprite, primaryColor, order++);

        // Right-Side Rank Star Insignias
        for (int i = 0; i < 5; i++)
        {
            GameObject star = CreatePart($"StarInsignia_{i}", innerPlate.transform, badgeSprite, primaryColor, order++);
            starInsignias.Add(star);
        }
    }

    private void UpdateBadge(int level)
    {
        SetupTierColors(level);

        float width = badgeSize.x;
        float height = badgeSize.y;

        // Set Root RectTransform size
        GetComponent<RectTransform>().sizeDelta = badgeSize;

        // 1. Base Banner Frame & Inner Plate
        ApplyUITransform(outerFrame, Vector2.zero, badgeSize, primaryColor);
        ApplyUITransform(innerPlate, Vector2.zero, new Vector2(width - 12f, height - 12f), plateColor);

        // 2. Backlight Aura (Level 7+)
        bool hasHalo = level >= 7;
        haloAura.SetActive(hasHalo);
        if (hasHalo)
        {
            float extraSize = (level >= 9) ? 40f : 20f;
            ApplyUITransform(haloAura, Vector2.zero, new Vector2(width + extraSize, height + extraSize), glowColor);
        }

        // 3. Horizontal Wings (Level 3+)
        bool hasWings = level >= 3;
        leftWing.SetActive(hasWings);
        rightWing.SetActive(hasWings);
        if (hasWings)
        {
            float wingW = (level >= 8) ? 60f : 40f;
            float wingH = height * 0.7f;
            ApplyUITransform(leftWing, new Vector2(-(width / 2f + wingW / 3f), 0), new Vector2(wingW, wingH), secondaryColor);
            ApplyUITransform(rightWing, new Vector2((width / 2f + wingW / 3f), 0), new Vector2(wingW, wingH), secondaryColor);
        }

        // 4. Embedded Icon Crest (Left Side)
        float crestSize = height * 0.75f;
        float crestX = -(width / 2f) + (crestSize / 2f) + 16f;
        ApplyUITransform(rankIconFrame, new Vector2(crestX, 0), new Vector2(crestSize, crestSize), primaryColor);
        rankIconFrame.transform.localRotation = Quaternion.Euler(0, 0, 45f); // Diamond shape

        // Core Gem (Level 5+)
        bool hasGem = level >= 5;
        coreGem.SetActive(hasGem);
        if (hasGem)
        {
            float gemSize = crestSize * 0.55f;
            ApplyUITransform(coreGem, Vector2.zero, new Vector2(gemSize, gemSize), gemColor);
        }

        // 5. Crown Top Ornament (Level 9+)
        bool hasCrown = level >= 9;
        crownAccent.SetActive(hasCrown);
        if (hasCrown)
        {
            ApplyUITransform(crownAccent, new Vector2(crestX, height / 2f + 8f), new Vector2(crestSize * 0.7f, 16f), primaryColor);
        }

        // 6. Progress Chevrons (Middle Section)
        int chevronCount = Mathf.Clamp((level - 1) % 4 + 1, 1, 4);
        float chevronsStartX = crestX + (crestSize / 2f) + 24f;

        for (int i = 0; i < progressChevrons.Count; i++)
        {
            if (i < chevronCount)
            {
                progressChevrons[i].SetActive(true);
                float posX = chevronsStartX + (i * 22f);
                ApplyUITransform(progressChevrons[i], new Vector2(posX, 0), new Vector2(12f, height * 0.5f), secondaryColor);
                progressChevrons[i].transform.localRotation = Quaternion.Euler(0, 0, 15f); // Slanted chevrons
            }
            else
            {
                progressChevrons[i].SetActive(false);
            }
        }

        // 7. Rank Star Insignias (Right Side)
        int starCount = (level <= 2) ? 1 : (level <= 4) ? 2 : (level <= 6) ? 3 : (level <= 8) ? 4 : 5;
        float starsStartX = (width / 2f) - 40f;

        for (int i = 0; i < starInsignias.Count; i++)
        {
            if (i < starCount)
            {
                starInsignias[i].SetActive(true);
                float posX = starsStartX - (i * 28f);
                ApplyUITransform(starInsignias[i], new Vector2(posX, 0), new Vector2(18f, 18f), primaryColor);
                starInsignias[i].transform.localRotation = Quaternion.Euler(0, 0, 45f);
            }
            else
            {
                starInsignias[i].SetActive(false);
            }
        }
    }

    private void ApplyUITransform(GameObject go, Vector2 pos, Vector2 size, Color color)
    {
        if (go == null) return;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    private GameObject CreatePart(string name, Transform parent, Sprite sprite, Color color, int siblingIndex)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;

        go.transform.SetSiblingIndex(siblingIndex);
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
}

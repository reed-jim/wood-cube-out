using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Procedural human avatar built from Unity UI Images (Unity 6.3 LTS, PC).
/// All art is generated at runtime: SDF-antialiased shapes, baked lighting overlays,
/// 9-sliced rounded rects, procedural iris / hair / brow textures, mip-mapped sprites
/// shared between every avatar through a static cache.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ProceduralHumanAvatarUI : MonoBehaviour
{
    [Header("Growth & Expression")]
    [Range(0f, 1f)]
    [Tooltip("0.0 = Child, 0.3 = Youth, 0.6 = Adult, 1.0 = Elder")]
    public float age = 0.6f;

    public enum Expression { WarmLeader, Inspirational, Approachable, Determined, Confident }
    public Expression expression = Expression.WarmLeader;

    [Header("Customize")]
    public float basicShapeCornerRadius = 24;

    [Header("Look (Lean-AAA)")]
    [Range(0f, 1.5f)]
    [Tooltip("Strength of baked shadows / highlights / skin shading.")]
    public float shadingStrength = 1f;
    [Tooltip("Cool back-light on the right edge of face and suit.")]
    public bool rimLight = true;
    public Color rimLightColor = new Color(0.62f, 0.80f, 1f);

    [Header("Idle Life (Optional)")]
    [Tooltip("Subtle blinking and breathing. Disabled automatically while dead.")]
    public bool idleLife = true;

    [Header("Manual Seed (Optional)")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    [Header("Death / Grayscale State")]
    [Tooltip("Optional. If empty, the avatar is desaturated by tinting (works with the default UI shader).")]
    public Material grayscaleMaterial;
    private bool isDead = false;
    private Dictionary<Image, Material> originalMaterials = new Dictionary<Image, Material>();

    public enum FaceShape { SoftOval, SoftSquare, Chiseled }

    // --- GENETIC DATA ---
    private Color skinColor;
    private Color eyeColor;
    private Color hairColor;

    // --- APOLLO A7L SPACESUIT COLORS ---
    private readonly Color suitFabricColor = new Color(0.92f, 0.93f, 0.94f);
    private readonly Color plssPackColor = new Color(0.85f, 0.86f, 0.88f);
    private readonly Color neckRingBaseColor = new Color(0.18f, 0.22f, 0.30f);
    private readonly Color neckRingBlueColor = new Color(0.10f, 0.35f, 0.75f);
    private readonly Color chestPackColor = new Color(0.80f, 0.82f, 0.85f);
    private readonly Color connectorRed = new Color(0.85f, 0.15f, 0.15f);
    private readonly Color connectorBlue = new Color(0.15f, 0.40f, 0.85f);
    private readonly Color flagRed = new Color(0.80f, 0.12f, 0.12f);
    private readonly Color flagBlue = new Color(0.12f, 0.20f, 0.55f);
    private readonly Color suitShadowTint = new Color(0.30f, 0.35f, 0.48f);

    private FaceShape faceShape;

    // Genetic Parameters (Friendly Leader)
    private float gene_HeadWidthRatio;
    private float gene_HeadHeightRatio;
    private float gene_EyeSpacing;
    private float gene_EyeScaleY;
    private float gene_EyeSize;
    private float gene_NoseWidth;
    private float gene_NoseLength;
    private float gene_LipWidth;
    private float gene_LipFullness;
    private float gene_EarSize;
    private float gene_EarYOffset;
    private float gene_BrowThickness;
    private float gene_BrowAngleOffset;
    private float gene_CheekProminence;
    private float gene_HairVolume;

    // Per-update derived colors
    private Color currentSkin;
    private Color currentShadowTint;
    private Color currentHairColor;

    // --- RENDER GAME OBJECTS ---
    private GameObject plssBackpack, plssShade, plssRim;
    private GameObject shoulders, suitShade, suitRim, shoulderPatchFlag, shoulderPatchBlue;
    private GameObject chestControlUnit, chestShade, chestRim;
    private GameObject rcuConnectorR1, rcuConnectorR2, rcuConnectorB1, rcuConnectorB2;
    private GameObject neck, neckShade, neckShadow, neckRingBase, neckRingBlue;

    private GameObject head, headShade, headLight, headRim;
    private GameObject leftEar, rightEar, earInnerL, earInnerR, simpleHair, hairShadow;
    private GameObject chinShadow, cheekL, cheekR;

    private GameObject foreheadWrinkle1, foreheadWrinkle2;
    private GameObject crowsFeetL, crowsFeetR;
    private GameObject nasolabialFoldL, nasolabialFoldR;
    private GameObject ageSpot1, ageSpot2;

    private GameObject eyeSocketL, eyeSocketR;
    private GameObject leftEye, rightEye, pupilL, pupilR, irisRingL, irisRingR;
    private GameObject eyeShadeL, eyeShadeR;
    private GameObject highlightL1, highlightR1, highlightL2, highlightR2;
    private GameObject upperLidL, upperLidR, lashL, lashR, lidCreaseL, lidCreaseR;
    private GameObject leftBrow, rightBrow;
    private GameObject noseBridge, noseTip, noseNostrilL, noseNostrilR, noseShadow, noseHighlight;
    private GameObject lipShadow, upperLip, lowerLip, lowerLipHighlight, mouthLine, mouthCornerL, mouthCornerR;

    /// <summary>Legacy field kept for compatibility. Holds the rounded-rect sprite.</summary>
    [HideInInspector] public Sprite circleSprite;

    // --- SPRITES (shared via static cache) ---
    private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private Sprite spEllipse, spSoft, spRound, spRoundShade, spRoundRim, spPill, spSphere;
    private Sprite spAlmond, spAlmondShade, spIris, spBrow, spLipUpper, spLipLower, spHair;
    private readonly Sprite[] spHead = new Sprite[3];
    private readonly Sprite[] spHeadShade = new Sprite[3];
    private readonly Sprite[] spHeadLight = new Sprite[3];
    private readonly Sprite[] spHeadRim = new Sprite[3];

    // Idle state
    private float nextBlinkTime = 2f;
    private float blinkStartTime = -1f;
    private const float BlinkDuration = 0.16f;

    // =====================================================================
    //  UNITY CALLBACKS
    // =====================================================================
    private void Start()
    {
        EnsureSprites();
        nextBlinkTime = Time.time + Random.Range(1.5f, 4f);

        GenerateNewRandomCharacter();
    }

    private void OnValidate()
    {
        if (head != null)
        {
            UpdateCharacter(age);
        }
    }

    private void Update()
    {
        if (!idleLife || head == null || isDead) return;

        // Breathing
        float breathe = Mathf.Sin(Time.time * 1.6f) * 0.5f + 0.5f;
        Vector3 bs = new Vector3(1f + breathe * 0.008f, 1f + breathe * 0.018f, 1f);
        if (shoulders != null) shoulders.transform.localScale = bs;
        if (plssBackpack != null) plssBackpack.transform.localScale = bs;

        // Blinking
        if (blinkStartTime < 0f && Time.time >= nextBlinkTime)
        {
            blinkStartTime = Time.time;
        }

        if (blinkStartTime >= 0f)
        {
            float k = (Time.time - blinkStartTime) / BlinkDuration;
            float sy = 1f;
            if (k >= 1f)
            {
                blinkStartTime = -1f;
                nextBlinkTime = Time.time + Random.Range(2.5f, 5.5f);
            }
            else
            {
                sy = Mathf.Lerp(0.08f, 1f, 1f - Mathf.Sin(k * Mathf.PI));
            }
            Vector3 es = new Vector3(1f, sy, 1f);
            if (leftEye != null) leftEye.transform.localScale = es;
            if (rightEye != null) rightEye.transform.localScale = es;
        }
    }

    // =====================================================================
    //  GENERATION API (unchanged)
    // =====================================================================
    [ContextMenu("Generate New Random Face")]
    public void GenerateNewRandomCharacter()
    {
        if (useRandomSeed) seed = Random.Range(1000, 999999);
        Rebuild(seed);
    }

    public void GenerateNewRandomCharacterWithSeed(int seed)
    {
        Rebuild(seed);
    }

    private void Rebuild(int s)
    {
        EnsureSprites();

        Random.State previous = Random.state;
        Random.InitState(s);
        RollGenetics();
        Random.state = previous;

        ClearChildren();
        BuildHierarchy();
        if (isDead) ApplyDeadMaterial(true);
        UpdateCharacter(age);
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject c = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                c.transform.SetParent(null, false);
                Destroy(c);
            }
            else
            {
                DestroyImmediate(c);
            }
        }
        originalMaterials.Clear();
        blinkStartTime = -1f;
    }

    private void RollGenetics()
    {
        float skinToneRoll = Random.value;
        if (skinToneRoll < 0.35f) skinColor = new Color(0.96f, 0.85f, 0.75f);
        else if (skinToneRoll < 0.65f) skinColor = new Color(0.84f, 0.65f, 0.50f);
        else if (skinToneRoll < 0.88f) skinColor = new Color(0.55f, 0.38f, 0.25f);
        else skinColor = new Color(0.32f, 0.22f, 0.15f);

        float hairRoll = Random.value;
        if (hairRoll < 0.35f) hairColor = new Color(0.18f, 0.14f, 0.12f); // Warm dark brown
        else if (hairRoll < 0.70f) hairColor = new Color(0.38f, 0.26f, 0.16f); // Chestnut brown
        else hairColor = new Color(0.60f, 0.58f, 0.55f); // Distinguished silver

        eyeColor = Random.ColorHSV(0.52f, 0.62f, 0.4f, 0.7f, 0.4f, 0.8f); // Warm hazel / steel blue
        faceShape = (FaceShape)Random.Range(0, System.Enum.GetValues(typeof(FaceShape)).Length);

        // Friendly leader proportions: Rounded, balanced, warm structure
        gene_HeadWidthRatio = Random.Range(0.96f, 1.04f);
        gene_HeadHeightRatio = Random.Range(0.96f, 1.03f);

        gene_EyeSpacing = Random.Range(-1f, 5f);
        gene_EyeScaleY = Random.Range(0.90f, 1.02f);
        gene_EyeSize = Random.Range(0.95f, 1.08f);

        gene_NoseWidth = Random.Range(0.90f, 1.10f);
        gene_NoseLength = Random.Range(0.90f, 1.05f);

        gene_LipWidth = Random.Range(0.95f, 1.15f);
        gene_LipFullness = Random.Range(0.90f, 1.15f);

        gene_EarSize = Random.Range(0.88f, 1.08f);
        gene_EarYOffset = Random.Range(-3f, 3f);

        gene_BrowThickness = Random.Range(5f, 8f);
        gene_BrowAngleOffset = Random.Range(-2f, 3f); // Relaxed, approachable brow line
        gene_CheekProminence = Random.Range(0.90f, 1.10f);
        gene_HairVolume = Random.Range(0.92f, 1.05f);
    }

    // =====================================================================
    //  HIERARCHY
    // =====================================================================
    private void BuildHierarchy()
    {
        int sh = (int)faceShape;
        Color none = Color.clear;

        // --- SUIT ---
        plssBackpack = Part("PLSS_Backpack", transform, spRound, plssPackColor, true);
        plssShade = Overlay("Shade", plssBackpack.transform, spRoundShade);
        plssRim = Overlay("Rim", plssBackpack.transform, spRoundRim);

        shoulders = Part("ApolloShoulders", transform, spRound, suitFabricColor, true);
        suitShade = Overlay("Shade", shoulders.transform, spRoundShade);
        suitRim = Overlay("Rim", shoulders.transform, spRoundRim);

        shoulderPatchBlue = Part("PatchFlagBlue", shoulders.transform, spRound, flagBlue, true);
        shoulderPatchFlag = Part("PatchFlagRed", shoulders.transform, spRound, flagRed, true);

        chestControlUnit = Part("RCU_ChestPack", shoulders.transform, spRound, chestPackColor, true);
        chestShade = Overlay("Shade", chestControlUnit.transform, spRoundShade);
        chestRim = Overlay("Rim", chestControlUnit.transform, spRoundRim);
        rcuConnectorR1 = Part("RCS_RedConnector1", chestControlUnit.transform, spSphere, connectorRed);
        rcuConnectorR2 = Part("RCS_RedConnector2", chestControlUnit.transform, spSphere, connectorRed);
        rcuConnectorB1 = Part("RCS_BlueConnector1", chestControlUnit.transform, spSphere, connectorBlue);
        rcuConnectorB2 = Part("RCS_BlueConnector2", chestControlUnit.transform, spSphere, connectorBlue);

        // --- NECK ---
        neck = Part("Neck", transform, spRound, Shade(skinColor, 0.88f), true);
        neckShade = Overlay("Shade", neck.transform, spRoundShade);
        neckShadow = Part("ChinCastShadow", neck.transform, spSoft, none);
        neckRingBase = Part("NeckRingBase", transform, spPill, neckRingBaseColor);
        neckRingBlue = Part("NeckRingBlueLock", neckRingBase.transform, spPill, neckRingBlueColor);

        // --- EARS ---
        leftEar = Part("LeftEar", transform, spEllipse, Shade(skinColor, 0.95f));
        rightEar = Part("RightEar", transform, spEllipse, Shade(skinColor, 0.95f));
        earInnerL = Part("EarInnerL", leftEar.transform, spEllipse, none);
        earInnerR = Part("EarInnerR", rightEar.transform, spEllipse, none);

        // --- HEAD ---
        head = Part("Head", transform, spHead[sh], skinColor);
        headShade = Overlay("Shade", head.transform, spHeadShade[sh]);
        headLight = Overlay("Light", head.transform, spHeadLight[sh]);
        headRim = Overlay("Rim", head.transform, spHeadRim[sh]);

        chinShadow = Part("ChinShadow", head.transform, spSoft, none);
        cheekL = Part("CheekL", head.transform, spSoft, none);
        cheekR = Part("CheekR", head.transform, spSoft, none);

        foreheadWrinkle1 = Part("ForeheadWrinkle1", head.transform, spEllipse, none);
        foreheadWrinkle2 = Part("ForeheadWrinkle2", head.transform, spEllipse, none);
        crowsFeetL = Part("CrowsFeetL", head.transform, spEllipse, none);
        crowsFeetR = Part("CrowsFeetR", head.transform, spEllipse, none);
        nasolabialFoldL = Part("NasolabialFoldL", head.transform, spEllipse, none);
        nasolabialFoldR = Part("NasolabialFoldR", head.transform, spEllipse, none);
        ageSpot1 = Part("AgeSpot1", head.transform, spSoft, none);
        ageSpot2 = Part("AgeSpot2", head.transform, spSoft, none);

        // --- EYES ---
        eyeSocketL = Part("EyeSocketL", head.transform, spSoft, none);
        eyeSocketR = Part("EyeSocketR", head.transform, spSoft, none);

        leftEye = Part("LeftEye", head.transform, spAlmond, Color.white);
        rightEye = Part("RightEye", head.transform, spAlmond, Color.white);

        irisRingL = Part("IrisRingL", leftEye.transform, spIris, eyeColor);
        irisRingR = Part("IrisRingR", rightEye.transform, spIris, eyeColor);
        pupilL = Part("PupilL", irisRingL.transform, spEllipse, new Color(0.08f, 0.08f, 0.09f));
        pupilR = Part("PupilR", irisRingR.transform, spEllipse, new Color(0.08f, 0.08f, 0.09f));

        eyeShadeL = Overlay("LidShadow", leftEye.transform, spAlmondShade);
        eyeShadeR = Overlay("LidShadow", rightEye.transform, spAlmondShade);

        highlightL1 = Part("HighlightL1", leftEye.transform, spEllipse, Color.white);
        highlightR1 = Part("HighlightR1", rightEye.transform, spEllipse, Color.white);
        highlightL2 = Part("HighlightL2", leftEye.transform, spEllipse, Color.white);
        highlightR2 = Part("HighlightR2", rightEye.transform, spEllipse, Color.white);

        upperLidL = Part("UpperLidL", leftEye.transform, spEllipse, none);
        upperLidR = Part("UpperLidR", rightEye.transform, spEllipse, none);
        lashL = Part("LashLineL", leftEye.transform, spEllipse, none);
        lashR = Part("LashLineR", rightEye.transform, spEllipse, none);

        lidCreaseL = Part("LidCreaseL", head.transform, spEllipse, none);
        lidCreaseR = Part("LidCreaseR", head.transform, spEllipse, none);

        // --- NOSE ---
        noseBridge = Part("NoseBridge", head.transform, spSoft, none);
        noseShadow = Part("NoseShadow", head.transform, spSoft, none);
        noseTip = Part("NoseTip", head.transform, spSphere, Shade(skinColor, 0.92f));
        noseNostrilL = Part("NostrilL", noseTip.transform, spEllipse, Shade(skinColor, 0.74f));
        noseNostrilR = Part("NostrilR", noseTip.transform, spEllipse, Shade(skinColor, 0.74f));
        noseHighlight = Part("NoseHighlight", noseTip.transform, spSoft, none);

        // --- MOUTH ---
        lipShadow = Part("LipShadow", head.transform, spSoft, none);
        upperLip = Part("UpperLip", head.transform, spLipUpper, new Color(0.68f, 0.42f, 0.42f));
        lowerLip = Part("LowerLip", head.transform, spLipLower, new Color(0.76f, 0.46f, 0.46f));
        lowerLipHighlight = Part("LowerLipHighlight", lowerLip.transform, spSoft, none);
        mouthLine = Part("MouthLine", head.transform, spEllipse, none);
        mouthCornerL = Part("MouthCornerL", head.transform, spSoft, new Color(0.52f, 0.28f, 0.28f));
        mouthCornerR = Part("MouthCornerR", head.transform, spSoft, new Color(0.52f, 0.28f, 0.28f));

        // --- BROWS & HAIR ---
        leftBrow = Part("LeftBrow", head.transform, spBrow, hairColor);
        rightBrow = Part("RightBrow", head.transform, spBrow, hairColor);
        rightBrow.transform.localScale = new Vector3(-1f, 1f, 1f); // mirror: thick end toward the nose

        hairShadow = Part("HairShadow", head.transform, spSoft, none);
        simpleHair = Part("SimpleHair", head.transform, spHair, hairColor);
    }

    // =====================================================================
    //  UPDATE (age + layout + shading)
    // =====================================================================
    public void UpdateCharacter(float t)
    {
        if (head == null) return;

        float kid = 1f - S(0f, 0.35f, t);   // 1 = child, 0 = adult
        float old = S(0.55f, 1f, t);        // 0 = adult, 1 = elder

        Color skin = Color.Lerp(skinColor, Color.Lerp(skinColor, new Color(0.85f, 0.80f, 0.77f), 0.55f), S(0.3f, 1f, t));
        currentSkin = skin;
        currentShadowTint = new Color(skin.r * 0.45f, skin.g * 0.18f, skin.b * 0.12f);
        Color sss = currentShadowTint;

        Color hair = Color.Lerp(hairColor, new Color(0.80f, 0.80f, 0.82f), S(0.45f, 0.9f, t));
        currentHairColor = hair;

        // --- SUIT ---
        float fixedShoulderW = 410f;
        float fixedShoulderH = 175f;

        ApplyUITransform(plssBackpack, new Vector2(0f, -125f), new Vector2(fixedShoulderW * 0.82f, 210f), plssPackColor);
        SetOverlay(plssShade, WithA(suitShadowTint, 0.70f));
        SetOverlay(plssRim, WithA(rimLightColor, rimLight ? 0.35f : 0f));

        ApplyUITransform(shoulders, new Vector2(0f, -155f), new Vector2(fixedShoulderW, fixedShoulderH), suitFabricColor);
        SetOverlay(suitShade, WithA(suitShadowTint, 0.72f));
        SetOverlay(suitRim, WithA(rimLightColor, rimLight ? 0.55f : 0f));

        ApplyUITransform(shoulderPatchFlag, new Vector2(-fixedShoulderW * 0.38f, 20f), new Vector2(32f, 22f), flagRed);
        ApplyUITransform(shoulderPatchBlue, new Vector2(-fixedShoulderW * 0.38f - 8f, 25f), new Vector2(14f, 12f), flagBlue);

        ApplyUITransform(chestControlUnit, new Vector2(0f, -12f), new Vector2(120f, 95f), chestPackColor);
        SetOverlay(chestShade, WithA(suitShadowTint, 0.80f));
        SetOverlay(chestRim, WithA(Color.white, 0.55f));
        ApplyUITransform(rcuConnectorR1, new Vector2(-32f, 15f), new Vector2(22f, 22f), connectorRed);
        ApplyUITransform(rcuConnectorR2, new Vector2(-32f, -18f), new Vector2(22f, 22f), connectorRed);
        ApplyUITransform(rcuConnectorB1, new Vector2(32f, 15f), new Vector2(22f, 22f), connectorBlue);
        ApplyUITransform(rcuConnectorB2, new Vector2(32f, -18f), new Vector2(22f, 22f), connectorBlue);

        float fixedNeckW = 125f;
        float fixedNeckH = 80f;
        ApplyUITransform(neck, new Vector2(0f, -80f), new Vector2(fixedNeckW, fixedNeckH), Shade(skin, 0.88f));
        SetOverlay(neckShade, WithA(sss, 0.50f));
        ApplyUITransform(neckShadow, new Vector2(0f, 26f), new Vector2(fixedNeckW * 0.95f, 56f), WithA(sss, 0.80f * Mathf.Min(1f, shadingStrength)));
        ApplyUITransform(neckRingBase, new Vector2(0f, -68f), new Vector2(fixedNeckW * 1.30f, 38f), neckRingBaseColor);
        ApplyUITransform(neckRingBlue, new Vector2(0f, 4f), new Vector2(fixedNeckW * 1.25f, 16f), neckRingBlueColor);

        // --- FACIAL GEOMETRY ---
        float baseHeadW = 182f * gene_HeadWidthRatio;
        float baseHeadH = 205f * gene_HeadHeightRatio;

        baseHeadW *= Mathf.Lerp(1f, 1.12f, kid);
        baseHeadH *= Mathf.Lerp(1f, 0.95f, kid);

        Vector2 shapeScale = Vector2.one;
        switch (faceShape)
        {
            case FaceShape.SoftSquare: shapeScale = new Vector2(1.02f, 0.99f); break;
            case FaceShape.Chiseled: shapeScale = new Vector2(0.99f, 1.02f); break;
            case FaceShape.SoftOval: shapeScale = new Vector2(1.0f, 1.0f); break;
        }

        float headW = baseHeadW * shapeScale.x;
        float headH = baseHeadH * shapeScale.y;
        ApplyUITransform(head, Vector2.zero, new Vector2(headW, headH), skin);
        SetOverlay(headShade, WithA(sss, 0.62f));
        SetOverlay(headLight, WithA(new Color(1f, 0.95f, 0.88f), 0.30f));
        SetOverlay(headRim, WithA(rimLightColor, rimLight ? 0.45f : 0f));

        float chinShadowH = Mathf.Lerp(18f, 28f, t);
        ApplyUITransform(chinShadow, new Vector2(0f, -headH * 0.38f), new Vector2(headW * 0.48f, chinShadowH * 1.4f), WithA(sss, 0.40f * Mathf.Min(1f, shadingStrength)));

        Color blush = new Color(Mathf.Min(1f, skin.r * 1.02f), skin.g * 0.70f, skin.b * 0.68f, 0.38f * Mathf.Min(1f, shadingStrength));
        float cheekW = 34f * gene_CheekProminence;
        ApplyUITransform(cheekL, new Vector2(-headW * 0.32f, -4f), new Vector2(cheekW * 1.7f, 42f), blush);
        ApplyUITransform(cheekR, new Vector2(headW * 0.32f, -4f), new Vector2(cheekW * 1.7f, 42f), blush);

        float earH = 46f * gene_EarSize;
        float earY = gene_EarYOffset;
        float earW = 23f * gene_EarSize;
        ApplyUITransform(leftEar, new Vector2(-headW * 0.49f, earY), new Vector2(earW, earH), Shade(skin, 0.95f));
        ApplyUITransform(rightEar, new Vector2(headW * 0.49f, earY), new Vector2(earW, earH), Shade(skin, 0.95f));
        ApplyUITransform(earInnerL, new Vector2(-1.5f, 0f), new Vector2(earW * 0.45f, earH * 0.58f), WithA(Shade(skin, 0.70f), 0.75f));
        ApplyUITransform(earInnerR, new Vector2(1.5f, 0f), new Vector2(earW * 0.45f, earH * 0.58f), WithA(Shade(skin, 0.70f), 0.75f));

        UpdateAgingDetails(headW, headH, t, skin);

        // --- EYES ---
        float eyeY = 12f - 6f * kid;
        float eyeSpacing = 42f + gene_EyeSpacing;
        float eyeSize = 35f * gene_EyeSize * Mathf.Lerp(1f, 1.25f, kid);

        PositionEyeUI(leftEye, irisRingL, pupilL, highlightL1, highlightL2, upperLidL, lashL, eyeSocketL, lidCreaseL, -eyeSpacing, eyeY, eyeSize, skin, t);
        PositionEyeUI(rightEye, irisRingR, pupilR, highlightR1, highlightR2, upperLidR, lashR, eyeSocketR, lidCreaseR, eyeSpacing, eyeY, eyeSize, skin, t);

        leftEye.GetComponent<RectTransform>().localRotation = Quaternion.identity;
        rightEye.GetComponent<RectTransform>().localRotation = Quaternion.identity;

        SetOverlay(eyeShadeL, WithA(sss, 0.75f));
        SetOverlay(eyeShadeR, WithA(sss, 0.75f));

        // --- BROWS / MOUTH (expression) ---
        ApplyFacialExpression(eyeY, eyeSpacing, t, hair);

        // --- HAIR ---
        Color hairTint = Boost(hair, 1.15f);
        float hairW = headW * 1.04f * gene_HairVolume;
        float hairH = headH * 0.34f * gene_HairVolume;
        hairH *= Mathf.Lerp(1f, 0.82f, old);   // thinning
        float hairY = (headH * 0.535f) - (hairH * 0.5f);
        ApplyUITransform(simpleHair, new Vector2(0f, hairY), new Vector2(hairW, hairH), hairTint);
        ApplyUITransform(hairShadow, new Vector2(0f, hairY - headH * 0.035f), new Vector2(headW * 0.80f, headH * 0.16f), WithA(sss, 0.34f * Mathf.Min(1f, shadingStrength)));

        // --- NOSE ---
        float noseY = -8f;
        float bridgeHeight = 26f * gene_NoseLength * Mathf.Lerp(1f, 0.7f, kid);
        float bridgeW = 8f * gene_NoseWidth;
        ApplyUITransform(noseBridge, new Vector2(0f, noseY + (bridgeHeight * 0.38f)), new Vector2(bridgeW * 1.8f, bridgeHeight * 1.1f), WithA(Shade(skin, 0.86f), 0.50f * Mathf.Min(1f, shadingStrength)));

        float noseWidth = 24f * gene_NoseWidth;
        float tipHeight = 15f;
        float tipY = noseY - (bridgeHeight * 0.15f);
        ApplyUITransform(noseShadow, new Vector2(0f, tipY - 7f), new Vector2(noseWidth * 1.35f, 13f), WithA(sss, 0.38f * Mathf.Min(1f, shadingStrength)));
        ApplyUITransform(noseTip, new Vector2(0f, tipY), new Vector2(noseWidth, tipHeight), Shade(skin, 0.97f));
        ApplyUITransform(noseNostrilL, new Vector2(-noseWidth * 0.35f, -2f), new Vector2(5f, 4f), WithA(Shade(skin, 0.45f), 0.85f));
        ApplyUITransform(noseNostrilR, new Vector2(noseWidth * 0.35f, -2f), new Vector2(5f, 4f), WithA(Shade(skin, 0.45f), 0.85f));
        ApplyUITransform(noseHighlight, new Vector2(-noseWidth * 0.12f, tipHeight * 0.22f), new Vector2(noseWidth * 0.34f, tipHeight * 0.38f), WithA(new Color(1f, 0.97f, 0.92f), 0.32f * Mathf.Min(1f, shadingStrength)));
    }

    private void UpdateAgingDetails(float headW, float headH, float t, Color skinCol)
    {
        Color wrinkleColor = Shade(skinCol, 0.68f);
        wrinkleColor.a = S(0.30f, 0.85f, t);

        Color ageSpotColor = Shade(skinCol, 0.72f);
        ageSpotColor.a = S(0.55f, 0.95f, t);

        ApplyUITransform(foreheadWrinkle1, new Vector2(0f, headH * 0.22f), new Vector2(headW * 0.45f, 3f), wrinkleColor);
        ApplyUITransform(foreheadWrinkle2, new Vector2(0f, headH * 0.28f), new Vector2(headW * 0.35f, 2.5f), wrinkleColor);

        // Softened crow's feet (laugh lines)
        ApplyUITransform(crowsFeetL, new Vector2(-headW * 0.38f, 10f), new Vector2(12f, 8f), wrinkleColor);
        ApplyUITransform(crowsFeetR, new Vector2(headW * 0.38f, 10f), new Vector2(12f, 8f), wrinkleColor);

        // Softened smile lines
        ApplyUITransform(nasolabialFoldL, new Vector2(-28f, -28f), new Vector2(3f, Mathf.Lerp(6f, 24f, t)), wrinkleColor);
        ApplyUITransform(nasolabialFoldR, new Vector2(28f, -28f), new Vector2(3f, Mathf.Lerp(6f, 24f, t)), wrinkleColor);

        nasolabialFoldL.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -12f);
        nasolabialFoldR.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 12f);

        ApplyUITransform(ageSpot1, new Vector2(-headW * 0.28f, headH * 0.18f), new Vector2(9f, 8f), ageSpotColor);
        ApplyUITransform(ageSpot2, new Vector2(headW * 0.22f, -headH * 0.15f), new Vector2(11f, 9f), ageSpotColor);
    }

    private void PositionEyeUI(GameObject eye, GameObject iris, GameObject pupil, GameObject h1, GameObject h2,
                               GameObject upperLid, GameObject lash, GameObject socket, GameObject crease,
                               float x, float y, float size, Color skinCol, float t)
    {
        float socketW = size * 1.15f;
        float socketH = size * 0.72f * gene_EyeScaleY;
        float shade = Mathf.Min(1f, shadingStrength);

        // Soft eye-socket occlusion (deepens with age)
        ApplyUITransform(socket, new Vector2(x, y + 1.5f), new Vector2(socketW * 1.55f, socketH * 2.4f),
            WithA(currentShadowTint, Mathf.Lerp(0.22f, 0.38f, t) * shade));

        // Sclera: warm off-white, yellows slightly with age
        Color sclera = Color.Lerp(new Color(0.96f, 0.95f, 0.93f), new Color(0.92f, 0.88f, 0.80f), t * 0.6f);
        ApplyUITransform(eye, new Vector2(x, y), new Vector2(socketW, socketH), sclera);

        float irisSize = socketH * 0.86f;
        ApplyUITransform(iris, Vector2.zero, new Vector2(irisSize, irisSize), Boost(eyeColor, 1.15f));

        float pupilSize = irisSize * 0.44f;
        ApplyUITransform(pupil, Vector2.zero, new Vector2(pupilSize, pupilSize), new Color(0.08f, 0.08f, 0.09f));

        // Catch-lights
        ApplyUITransform(h1, new Vector2(irisSize * 0.20f, irisSize * 0.22f), new Vector2(irisSize * 0.28f, irisSize * 0.28f), Color.white);
        ApplyUITransform(h2, new Vector2(-irisSize * 0.18f, -irisSize * 0.24f), new Vector2(irisSize * 0.12f, irisSize * 0.12f), new Color(1f, 1f, 1f, 0.55f));

        // Relaxed upper lid + lash line + crease
        ApplyUITransform(upperLid, new Vector2(0f, socketH * 0.50f), new Vector2(socketW * 1.04f, socketH * 0.22f), WithA(Shade(skinCol, 0.86f), 0.92f));
        ApplyUITransform(lash, new Vector2(0f, socketH * 0.47f), new Vector2(socketW * 1.06f, Mathf.Max(2.2f, socketH * 0.13f)),
            WithA(Shade(currentHairColor, 0.30f), 0.95f));
        ApplyUITransform(crease, new Vector2(x, y + socketH * 0.80f), new Vector2(socketW * 0.95f, 2.4f),
            WithA(Shade(skinCol, 0.70f), Mathf.Lerp(0.35f, 0.60f, t)));
    }

    private void ApplyFacialExpression(float eyeY, float eyeSpacing, float t, Color browColor)
    {
        float kid = 1f - S(0f, 0.35f, t);   // 1 = child, 0 = adult
        float old = S(0.55f, 1f, t);        // 0 = adult, 1 = elder

        float browY = eyeY + 16f;
        float mouthY = -46f + 4f * kid;

        // Lips blend slightly toward the skin tone so they sit naturally on every complexion
        Color upperLipCol = Color.Lerp(Shade(currentSkin, 0.88f), new Color(0.68f, 0.42f, 0.42f), 0.72f);
        Color lowerLipCol = Color.Lerp(Shade(currentSkin, 0.92f), new Color(0.76f, 0.46f, 0.46f), 0.72f);
        Color cornerCol = WithA(Shade(upperLipCol, 0.65f), 0.80f);

        float lipWidth = 48f * gene_LipWidth;
        float upperLipH = 6f * gene_LipFullness;
        float lowerLipH = 7.5f * gene_LipFullness;

        float browAngleL = 1f;
        float browAngleR = -1f;
        float browOffsetL = 0f;
        float browOffsetR = 0f;

        // Soft dimple-shadows at the corners (larger than the old hard 4-5px squares)
        Vector2 cornerSize = new Vector2(7f, 7f);

        switch (expression)
        {
            case Expression.WarmLeader:
                // Gentle smile curve + soft brow
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.92f, lowerLipH), lowerLipCol);

                // Slightly raised corners create a warm, natural smile
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.50f, mouthY + 3.5f), cornerSize, cornerCol);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.50f, mouthY + 3.5f), cornerSize, cornerCol);

                browAngleL = 2f;
                browAngleR = -2f;
                browOffsetL = 1f;
                browOffsetR = 1f;
                break;

            case Expression.Inspirational:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 2f), new Vector2(lipWidth * 1.02f, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 2f), new Vector2(lipWidth * 0.95f, lowerLipH + 1f), lowerLipCol);

                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.51f, mouthY + 4.5f), cornerSize, cornerCol);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.51f, mouthY + 4.5f), cornerSize, cornerCol);

                browAngleL = 4f;
                browAngleR = -4f;
                browOffsetL = 2f;
                browOffsetR = 2f;
                break;

            case Expression.Approachable:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth * 0.98f, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.90f, lowerLipH), lowerLipCol);

                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.49f, mouthY + 3f), cornerSize * 0.9f, cornerCol);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.49f, mouthY + 3f), cornerSize * 0.9f, cornerCol);

                browAngleL = 1f;
                browAngleR = -1f;
                break;

            case Expression.Determined:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY), new Vector2(lipWidth * 0.95f, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.88f, lowerLipH), lowerLipCol);

                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.48f, mouthY + 1f), cornerSize * 0.9f, cornerCol);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.48f, mouthY + 1f), cornerSize * 0.9f, cornerCol);

                browAngleL = -3f;
                browAngleR = 3f;
                browOffsetL = -1f;
                browOffsetR = -1f;
                break;

            case Expression.Confident:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.90f, lowerLipH), lowerLipCol);

                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.50f, mouthY + 2f), cornerSize * 0.9f, cornerCol);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.50f, mouthY + 4f), cornerSize * 0.9f, cornerCol);

                browAngleL = 0f;
                browAngleR = -3f;
                browOffsetR = 1f;
                break;
        }

        // --- Mouth detail derived from the lip layout (follows every expression, incl. the Confident smirk) ---
        RectTransform ur = upperLip.GetComponent<RectTransform>();
        RectTransform lr = lowerLip.GetComponent<RectTransform>();
        RectTransform cl = mouthCornerL.GetComponent<RectTransform>();
        RectTransform cr = mouthCornerR.GetComponent<RectTransform>();

        float upperBottom = ur.anchoredPosition.y - ur.sizeDelta.y * 0.5f;
        float lowerTop = lr.anchoredPosition.y + lr.sizeDelta.y * 0.5f;
        float lineY = (upperBottom + lowerTop) * 0.5f - 0.2f;
        float tilt = Mathf.Atan2(cr.anchoredPosition.y - cl.anchoredPosition.y, cr.anchoredPosition.x - cl.anchoredPosition.x) * Mathf.Rad2Deg;

        ApplyUITransform(mouthLine, new Vector2(0f, lineY), new Vector2(ur.sizeDelta.x * 0.97f, 2.2f), WithA(new Color(0.30f, 0.12f, 0.12f), 0.85f));
        mouthLine.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, tilt);

        ApplyUITransform(lowerLipHighlight, new Vector2(-lr.sizeDelta.x * 0.06f, lr.sizeDelta.y * 0.12f),
            new Vector2(lr.sizeDelta.x * 0.40f, lr.sizeDelta.y * 0.32f), WithA(new Color(1f, 0.92f, 0.90f), 0.35f * Mathf.Min(1f, shadingStrength)));

        ApplyUITransform(lipShadow, new Vector2(0f, lr.anchoredPosition.y - lr.sizeDelta.y * 0.5f - 4f),
            new Vector2(lr.sizeDelta.x * 0.75f, 11f), WithA(currentShadowTint, 0.30f * Mathf.Min(1f, shadingStrength)));

        // --- Brows ---
        Color browTint = Boost(browColor, 1.10f);
        Vector2 browSize = new Vector2(33f * gene_LipWidth, gene_BrowThickness);
        ApplyUITransform(leftBrow, new Vector2(-eyeSpacing, browY + browOffsetL), browSize, browTint);
        ApplyUITransform(rightBrow, new Vector2(eyeSpacing, browY + browOffsetR), browSize, browTint);

        leftBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, browAngleL + gene_BrowAngleOffset);
        rightBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, browAngleR - gene_BrowAngleOffset);
    }

    // =====================================================================
    //  DEATH / GRAYSCALE
    // =====================================================================
    [ContextMenu("Die")]
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Reset idle transforms so the corpse is still
        if (shoulders != null) shoulders.transform.localScale = Vector3.one;
        if (plssBackpack != null) plssBackpack.transform.localScale = Vector3.one;
        if (leftEye != null) leftEye.transform.localScale = Vector3.one;
        if (rightEye != null) rightEye.transform.localScale = Vector3.one;

        ApplyDeadMaterial(true);
        UpdateCharacter(age); // re-tints every part through Tint()
    }

    public void Revive()
    {
        if (!isDead) return;
        isDead = false;

        ApplyDeadMaterial(false);
        UpdateCharacter(age);
    }

    private void ApplyDeadMaterial(bool on)
    {
        if (grayscaleMaterial == null) return;

        if (on)
        {
            foreach (Image img in GetComponentsInChildren<Image>(true))
            {
                if (img == null) continue;
                if (!originalMaterials.ContainsKey(img)) originalMaterials[img] = img.material;
                img.material = grayscaleMaterial;
            }
        }
        else
        {
            foreach (var pair in originalMaterials)
            {
                if (pair.Key != null) pair.Key.material = pair.Value;
            }
            originalMaterials.Clear();
        }
    }

    /// <summary>Desaturates + dims a color while dead (works without any custom shader).</summary>
    private Color Tint(Color c)
    {
        if (!isDead) return c;
        float g = (c.r * 0.299f + c.g * 0.587f + c.b * 0.114f) * 0.82f;
        return new Color(g, g, g, c.a);
    }

    // =====================================================================
    //  PART HELPERS
    // =====================================================================
    private void ApplyUITransform(GameObject go, Vector2 pos, Vector2 size, Color color)
    {
        if (go == null) return;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        if (img != null) img.color = Tint(color);
    }

    /// <summary>Overlays stretch to their parent, so only the color needs updating.</summary>
    private void SetOverlay(GameObject go, Color color)
    {
        if (go == null) return;
        Image img = go.GetComponent<Image>();
        if (img == null) return;
        color.a = Mathf.Clamp01(color.a * shadingStrength);
        img.color = Tint(color);
    }

    private GameObject Part(string name, Transform parent, Sprite sprite, Color color, bool sliced = false)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = Tint(color);
        img.raycastTarget = false;
        if (sliced)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f;
        }
        return go;
    }

    private GameObject Overlay(string name, Transform parent, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = Color.clear;
        img.raycastTarget = false;

        // Rounded-rect overlays must slice exactly like their parent
        if (sprite != null && sprite.border.sqrMagnitude > 0f)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f;
        }
        return go;
    }

    private static Color Shade(Color c, float k)
    {
        return new Color(Mathf.Clamp01(c.r * k), Mathf.Clamp01(c.g * k), Mathf.Clamp01(c.b * k), 1f);
    }

    private static Color Boost(Color c, float k)
    {
        return new Color(Mathf.Clamp01(c.r * k), Mathf.Clamp01(c.g * k), Mathf.Clamp01(c.b * k), c.a);
    }

    private static Color WithA(Color c, float a)
    {
        return new Color(c.r, c.g, c.b, a);
    }

    // =====================================================================
    //  PROCEDURAL TEXTURES
    //  All sprites are white/grey (lighting baked into alpha or luminance)
    //  and tinted by Image.color, so a handful of textures serve every skin,
    //  hair and eye color. Textures are mip-mapped to avoid shimmering when
    //  small parts (brows, lashes, highlights) are drawn at low resolution.
    // =====================================================================
    private static float Sat(float x) { return Mathf.Clamp01(x); }

    private static float S(float a, float b, float x)
    {
        float t = Mathf.Clamp01((x - a) / (b - a));
        return t * t * (3f - 2f * t);
    }

    private static Color32 C(float r, float g, float b, float a)
    {
        return new Color32((byte)(Sat(r) * 255f + 0.5f), (byte)(Sat(g) * 255f + 0.5f),
                           (byte)(Sat(b) * 255f + 0.5f), (byte)(Sat(a) * 255f + 0.5f));
    }

    private static Sprite Build(string key, int w, int h, System.Func<float, float, Color32> px, Vector4 border = default(Vector4))
    {
        Sprite cached;
        if (spriteCache.TryGetValue(key, out cached) && cached != null) return cached;

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true, false);
        tex.name = key;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Trilinear;
        tex.hideFlags = HideFlags.HideAndDontSave;

        Color32[] pixels = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            float v = (y + 0.5f) / h;
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w;
                pixels[y * w + x] = px(u, v);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply(true, true);

        Sprite sp = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        sp.name = key;
        sp.hideFlags = HideFlags.HideAndDontSave;
        spriteCache[key] = sp;
        return sp;
    }

    private static float RoundDist(float u, float v, float R)
    {
        float px = (u - 0.5f) * 128f;
        float py = (v - 0.5f) * 128f;
        float dx = Mathf.Abs(px) - (64f - R);
        float dy = Mathf.Abs(py) - (64f - R);
        float ox = Mathf.Max(dx, 0f);
        float oy = Mathf.Max(dy, 0f);
        return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(dx, dy), 0f) - R;
    }

    private static float AlmondH(float x)
    {
        float ax = Mathf.Abs(x);
        return Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Pow(ax, 1.8f)), 0.85f);
    }

    /// <summary>Super-ellipse head with a jaw taper. Returns the normalized radius (1 = edge).</summary>
    private static float HeadR(float x, float y, int shape)
    {
        float n = shape == 1 ? 2.5f : (shape == 2 ? 2.2f : 2.0f);
        float taper = shape == 2 ? 0.20f : 0.08f;
        float wy = 1f - taper * Mathf.Pow(Sat(-y), 1.3f);
        float ax = Mathf.Abs(x) / wy;
        float ay = Mathf.Abs(y);
        return Mathf.Pow(Mathf.Pow(ax, n) + Mathf.Pow(ay, n), 1f / n);
    }

    private void EnsureSprites()
    {
        int R = Mathf.Clamp(Mathf.RoundToInt(basicShapeCornerRadius), 1, 63);
        Vector4 border = new Vector4(R, R, R, R);

        spEllipse = Build("ell", 256, 256, (u, v) =>
        {
            float x = u * 2f - 1f, y = v * 2f - 1f;
            float r = Mathf.Sqrt(x * x + y * y);
            return C(1f, 1f, 1f, Sat((1f - r) * 128f * 0.9f + 0.5f));
        });

        spSoft = Build("soft", 128, 128, (u, v) =>
        {
            float x = u * 2f - 1f, y = v * 2f - 1f;
            float r = Mathf.Sqrt(x * x + y * y);
            float s = Sat(1f - r);
            return C(1f, 1f, 1f, s * s * (3f - 2f * s));
        });

        // --- Rounded rect family (9-sliced: constant corner radius at any size) ---
        spRound = Build("round" + R, 128, 128, (u, v) =>
        {
            float d = RoundDist(u, v, R);
            float n = Mathf.PerlinNoise(u * 9f, v * 9f);
            float lum = 0.93f + 0.07f * n; // subtle fabric mottling
            return C(lum, lum, lum, Sat(0.5f - d));
        }, border);

        spRoundShade = Build("roundShade" + R, 128, 128, (u, v) =>
        {
            float d = RoundDist(u, v, R);
            float mask = Sat(0.5f - d);
            float inside = Sat(-d / (R * 1.2f + 4f));
            float edgeDark = 1f - S(0f, 1f, inside);
            float a = Sat(0.60f * edgeDark + 0.25f * (1f - v));
            return C(1f, 1f, 1f, mask * a);
        }, border);

        spRoundRim = Build("roundRim" + R, 128, 128, (u, v) =>
        {
            float d = RoundDist(u, v, R);
            float mask = Sat(0.5f - d);
            float band = Sat(1f - (-d) / 5f);
            float top = Sat((v - 0.45f) * 3f);
            return C(1f, 1f, 1f, mask * band * top * 0.9f);
        }, border);
        circleSprite = spRound;

        // --- Horizontal capsule with cylindrical shading (neck rings) ---
        spPill = Build("pill", 256, 64, (u, v) =>
        {
            float px = (u * 2f - 1f) * 128f;
            float py = (v * 2f - 1f) * 32f;
            float dx = Mathf.Max(Mathf.Abs(px) - 96f, 0f);
            float dist = Mathf.Sqrt(dx * dx + py * py) - 32f;
            float yy = py / 32f;
            float lum = 0.55f + 0.45f * Mathf.Cos((yy - 0.3f) * 1.2f);
            lum += 0.15f * Mathf.Exp(-Mathf.Pow((yy - 0.45f) / 0.12f, 2f));
            lum *= 1f - 0.30f * Mathf.Pow(Sat((Mathf.Abs(px) - 70f) / 58f), 2f);
            return C(lum, lum, lum, Sat(0.5f - dist));
        });

        // --- Shaded sphere (connectors, nose tip) ---
        spSphere = Build("sphere", 128, 128, (u, v) =>
        {
            float x = u * 2f - 1f, y = v * 2f - 1f;
            float r = Mathf.Sqrt(x * x + y * y);
            float z = Mathf.Sqrt(Mathf.Max(0f, 1f - r * r));
            Vector3 n = new Vector3(x, y, z);
            Vector3 L = new Vector3(-0.4f, 0.5f, 0.75f).normalized;
            float diff = Sat(Vector3.Dot(n, L));
            float lum = 0.42f + 0.58f * diff;
            return C(lum, lum, lum, Sat((1f - r) * 64f * 0.9f + 0.5f));
        });

        // --- Almond eye + lid shadow ---
        spAlmond = Build("almond", 256, 128, (u, v) =>
        {
            float x = u * 2f - 1f, y = v * 2f - 1f;
            float d = (AlmondH(x) - Mathf.Abs(y)) * 64f;
            return C(1f, 1f, 1f, Sat(d + 0.5f));
        });

        spAlmondShade = Build("almondShade", 256, 128, (u, v) =>
        {
            float x = u * 2f - 1f, y = v * 2f - 1f;
            float d = (AlmondH(x) - Mathf.Abs(y)) * 64f;
            float mask = Sat(d + 0.5f);
            float topS = Sat((y + 0.1f) / 1.1f);
            topS *= topS;
            float corner = Mathf.Pow(Mathf.Abs(x), 3f) * 0.5f;
            return C(1f, 1f, 1f, mask * Sat(0.62f * topS + corner + 0.05f));
        });

        // --- Procedural iris: radial fibers, limbal ring, collarette ---
        spIris = Build("iris", 128, 128, (u, v) =>
        {
            float x = u * 2f - 1f, y = v * 2f - 1f;
            float r = Mathf.Sqrt(x * x + y * y);
            float ang = Mathf.Atan2(y, x);
            float fib = Mathf.PerlinNoise(Mathf.Cos(ang) * 5f + 9f, Mathf.Sin(ang) * 5f + 9f);
            float lum = Mathf.Lerp(0.62f, 1f, fib);
            lum *= 1f - 0.60f * S(0.70f, 1f, r);
            lum += 0.22f * Mathf.Exp(-Mathf.Pow((r - 0.52f) / 0.14f, 2f));
            lum *= 0.75f + 0.25f * S(0.15f, 0.40f, r);
            return C(lum, lum, lum, Sat((1f - r) * 64f * 0.9f + 0.5f));
        });

        // --- Tapered, arched eyebrow with hair-stroke texture (thick end at u = 1) ---
        spBrow = Build("brow", 256, 64, (u, v) =>
        {
            float x = u * 2f - 1f;
            float y = v * 2f - 1f;
            float ht = Mathf.Lerp(0.38f, 0.92f, Mathf.Pow(u, 1.4f)) * Mathf.Pow(Sat(Mathf.Min(u, 1f - u) * 7f), 0.55f);
            float y0 = 0.12f * (1f - x * x) - 0.06f;
            float d = ht - Mathf.Abs(y - y0);
            float jitter = (Mathf.PerlinNoise(u * 55f, v * 7f) - 0.5f) * 0.35f;
            float a = Sat((d + jitter * Sat(1f - d * 3f)) * 7f);
            float lum = 0.75f + 0.25f * Mathf.PerlinNoise(u * 40f, v * 6f);
            return C(lum, lum, lum, a);
        });

        // --- Lips: soft ellipse with baked vertical gradient ---
        spLipUpper = Build("lipUp", 256, 64, (u, v) =>
        {
            float x = u * 2f - 1f, y = v * 2f - 1f;
            float r = Mathf.Sqrt(x * x + y * y);
            float lum = (0.78f + 0.22f * Sat(y + 0.2f)) * (1f - 0.15f * Mathf.Pow(Sat(r), 3f));
            return C(lum, lum, lum, Sat((1f - r) * 32f * 0.9f + 0.5f));
        });

        spLipLower = Build("lipLow", 256, 64, (u, v) =>
        {
            float x = u * 2f - 1f, y = v * 2f - 1f;
            float r = Mathf.Sqrt(x * x + y * y);
            float lum = (0.80f + 0.20f * Sat(1.1f - Mathf.Abs(y - 0.25f) * 1.1f)) * (1f - 0.12f * Mathf.Pow(Sat(r), 3f));
            return C(lum, lum, lum, Sat((1f - r) * 32f * 0.9f + 0.5f));
        });

        // --- Hair cap: hairline, radial strands, gloss band ---
        spHair = Build("hair", 256, 128, (u, v) =>
        {
            float x = u * 2f - 1f;
            float ex = (u - 0.5f) / 0.5f, ey = (v - 0.35f) / 0.65f;
            float r = Mathf.Sqrt(ex * ex + ey * ey);
            float outer = Sat((1f - r) * 128f * 0.9f + 0.5f);

            float ax = Mathf.Abs(x);
            float vb = 0.50f * (1f - Mathf.Pow(ax, 2.2f)) + 0.04f;
            vb += (Mathf.PerlinNoise(u * 30f, 3.1f) - 0.5f) * 0.05f;
            float cut = Sat((v - vb) * 40f);

            float ang = Mathf.Atan2(u - 0.5f, v + 0.35f);
            float stripe = Mathf.PerlinNoise(ang * 45f + 20f, 7.3f);
            float lum = 0.62f + 0.38f * stripe;
            lum *= 0.85f + 0.15f * Sat((v - vb) * 6f);
            float gloss = Mathf.Exp(-Mathf.Pow((v - 0.80f) / 0.10f, 2f)) * (0.25f + 0.5f * Sat(1f - ax * 1.1f));
            lum = Sat(lum + gloss * 0.35f);
            return C(lum, lum, lum, outer * cut);
        });

        // --- Head family: one set per face shape ---
        for (int i = 0; i < 3; i++)
        {
            int sh = i;

            spHead[i] = Build("head" + sh, 256, 256, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f;
                float r = HeadR(x, y, sh);
                return C(1f, 1f, 1f, Sat((1f - r) * 128f * 0.9f + 0.5f));
            });

            // Warm sub-surface shadow: rim falloff, lower-right form shadow, under-jaw, pore noise
            spHeadShade[i] = Build("headShade" + sh, 256, 256, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f;
                float r = HeadR(x, y, sh);
                float mask = Sat((1f - r) * 128f * 0.9f + 0.5f);
                float edge = Mathf.Pow(Sat(r), 3.2f) * 0.55f;
                float lr = Sat(0.15f + x * 0.55f - y * 0.75f);
                float side = lr * lr * 0.32f;
                float jaw = 0.22f * Sat((-y - 0.5f) / 0.5f);
                float n = (Mathf.PerlinNoise(u * 90f, v * 90f) - 0.5f) * 0.06f
                        + (Mathf.PerlinNoise(u * 18f + 5f, v * 18f + 5f) - 0.5f) * 0.05f;
                return C(1f, 1f, 1f, mask * Sat(0.08f + edge + side + jaw + n));
            });

            // Key light from upper-left
            spHeadLight[i] = Build("headLight" + sh, 256, 256, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f;
                float r = HeadR(x, y, sh);
                float mask = Sat((1f - r) * 128f * 0.9f + 0.5f);
                float dx = x + 0.30f, dy = y - 0.38f;
                return C(1f, 1f, 1f, mask * 0.85f * Mathf.Exp(-(dx * dx + dy * dy) / 0.32f));
            });

            // Cool back-light on the right edge
            spHeadRim[i] = Build("headRim" + sh, 256, 256, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f;
                float r = HeadR(x, y, sh);
                float mask = Sat((1f - r) * 128f * 0.9f + 0.5f);
                return C(1f, 1f, 1f, mask * Mathf.Pow(Sat(r), 7f) * Sat(0.25f + x * 1.1f));
            });
        }

        // Legacy-compatible alias so external code reading the field still gets a valid sprite
        if (circleSprite == null) circleSprite = spRound;
    }
}

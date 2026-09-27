using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class ProceduralHumanAvatarUI : MonoBehaviour
{
    [Header("Growth & Expression")]
    [Range(0f, 1f)]
    [Tooltip("0.0 = Child, 0.3 = Youth, 0.6 = Adult, 1.0 = Elder")]
    public float age = 0.6f;

    public enum Expression { FriendlyNeutral, Happy, Gentle, Focused, Surprised }
    public Expression expression = Expression.FriendlyNeutral;

    [Header("Customize")]
    public float basicShapeCornerRadius = 24;

    [Header("Manual Seed (Optional)")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    [Header("Death / Grayscale State")]
    public Material grayscaleMaterial; // Assign a custom Grayscale UI Material or leave null for dynamic creation
    private bool isDead = false;
    private Dictionary<Image, Material> originalMaterials = new Dictionary<Image, Material>();

    public enum FaceShape { SoftOval, Round, Heart, SoftSquare, Chiseled, LongRectangular }

    // --- GENETIC DATA ---
    private Color skinColor;
    private Color eyeColor;
    private Color hairColor;

    // --- APOLLO A7L SPACESUIT COLORS ---
    private readonly Color suitFabricColor = new Color(0.92f, 0.93f, 0.94f);      // NASA White Beta Cloth
    private readonly Color plssPackColor = new Color(0.85f, 0.86f, 0.88f);        // Life Support Pack White
    private readonly Color neckRingBaseColor = new Color(0.18f, 0.22f, 0.30f);    // Anodized Dark Metal Collar
    private readonly Color neckRingBlueColor = new Color(0.10f, 0.35f, 0.75f);    // Blue Anodized Helmet Ring
    private readonly Color chestPackColor = new Color(0.80f, 0.82f, 0.85f);       // RCU Control Module Base
    private readonly Color connectorRed = new Color(0.85f, 0.15f, 0.15f);          // Red Hose Port
    private readonly Color connectorBlue = new Color(0.15f, 0.40f, 0.85f);         // Blue Hose Port
    private readonly Color flagRed = new Color(0.80f, 0.12f, 0.12f);              // US Flag Red
    private readonly Color flagBlue = new Color(0.12f, 0.20f, 0.55f);             // US Flag Blue

    private FaceShape faceShape;

    // High Variability Genetic Parameters
    private float gene_HeadWidthRatio;
    private float gene_HeadHeightRatio;
    private float gene_EyeSpacing;
    private float gene_EyeScaleY;
    private float gene_EyeSize;
    private float gene_EyeTilt;
    private float gene_NoseWidth;
    private float gene_NoseLength;
    private int gene_NoseType; // 0 = Rounded, 1 = Sharp Bridge, 2 = Wide Base
    private float gene_LipWidth;
    private float gene_LipFullness;
    private float gene_EarSize;
    private float gene_EarYOffset;
    private float gene_BrowThickness;
    private float gene_BrowAngleOffset;
    private float gene_CheekProminence;
    private float gene_HairVolume;

    // --- RENDER GAME OBJECTS (UI) ---
    // Apollo A7L Spacesuit Parts
    private GameObject plssBackpack, shoulders, shoulderPatchFlag, shoulderPatchBlue;
    private GameObject chestControlUnit, rcuConnectorR1, rcuConnectorR2, rcuConnectorB1, rcuConnectorB2;
    private GameObject neck, neckRingBase, neckRingBlue;

    // Head & Single-Object Hair
    private GameObject head, leftEar, rightEar, simpleHair;
    private GameObject chinShadow, cheekL, cheekR;

    // Aging Detail Layers
    private GameObject foreheadWrinkle1, foreheadWrinkle2;
    private GameObject crowsFeetL, crowsFeetR;
    private GameObject nasolabialFoldL, nasolabialFoldR;
    private GameObject ageSpot1, ageSpot2;

    // Eyes & Face Details
    private GameObject leftEye, rightEye, pupilL, pupilR, irisRingL, irisRingR;
    private GameObject highlightL1, highlightR1, highlightL2, highlightR2;
    private GameObject upperLidL, upperLidR, eyeBagL, eyeBagR;
    private GameObject leftBrow, rightBrow;
    private GameObject noseBridge, noseTip, noseNostrilL, noseNostrilR;
    private GameObject upperLip, lowerLip, mouthCornerL, mouthCornerR;

    public Sprite circleSprite;

    private void Start()
    {
        circleSprite = CreateCircleSprite();
    }

    private void OnValidate()
    {
        if (head != null)
        {
            UpdateCharacter(age);
        }
    }

    [ContextMenu("Generate New Random Face")]
    public void GenerateNewRandomCharacter()
    {
        if (useRandomSeed) seed = Random.Range(1000, 999999);
        Random.InitState(seed);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        RollGenetics();
        BuildHierarchy();
        UpdateCharacter(age);
    }

    public void GenerateNewRandomCharacterWithSeed(int seed)
    {
        if (circleSprite == null)
        {
            circleSprite = CreateCircleSprite();
        }

        Random.InitState(seed);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        RollGenetics();
        BuildHierarchy();
        UpdateCharacter(age);
    }

    private void RollGenetics()
    {
        float skinToneRoll = Random.value;
        if (skinToneRoll < 0.25f) skinColor = new Color(0.98f, 0.86f, 0.76f);
        else if (skinToneRoll < 0.50f) skinColor = new Color(0.85f, 0.65f, 0.48f);
        else if (skinToneRoll < 0.75f) skinColor = new Color(0.58f, 0.39f, 0.25f);
        else skinColor = new Color(0.28f, 0.18f, 0.12f);

        float hairRoll = Random.value;
        if (hairRoll < 0.35f) hairColor = new Color(0.12f, 0.10f, 0.08f);
        else if (hairRoll < 0.65f) hairColor = new Color(0.35f, 0.22f, 0.14f);
        else if (hairRoll < 0.85f) hairColor = new Color(0.65f, 0.45f, 0.22f);
        else hairColor = new Color(0.75f, 0.25f, 0.15f);

        eyeColor = Random.ColorHSV(0.0f, 1.0f, 0.4f, 0.85f, 0.3f, 0.9f);
        faceShape = (FaceShape)Random.Range(0, System.Enum.GetValues(typeof(FaceShape)).Length);

        gene_HeadWidthRatio = Random.Range(0.75f, 1.25f);
        gene_HeadHeightRatio = Random.Range(0.78f, 1.22f);

        gene_EyeSpacing = Random.Range(-12f, 12f);
        gene_EyeScaleY = Random.Range(0.75f, 1.15f);
        gene_EyeSize = Random.Range(0.80f, 1.15f);
        gene_EyeTilt = Random.Range(-6f, 6f);

        gene_NoseWidth = Random.Range(0.65f, 1.40f);
        gene_NoseLength = Random.Range(0.65f, 1.35f);
        gene_NoseType = Random.Range(0, 3);

        gene_LipWidth = Random.Range(0.70f, 1.30f);
        gene_LipFullness = Random.Range(0.60f, 1.40f);

        gene_EarSize = Random.Range(0.70f, 1.30f);
        gene_EarYOffset = Random.Range(-15f, 15f);

        gene_BrowThickness = Random.Range(3f, 9f);
        gene_BrowAngleOffset = Random.Range(-10f, 10f);
        gene_CheekProminence = Random.Range(0.5f, 1.5f);
        gene_HairVolume = Random.Range(0.85f, 1.25f);
    }

    private void BuildHierarchy()
    {
        int order = 0;

        // --- 1. SPACESUIT BASE ---
        plssBackpack = CreatePart("PLSS_Backpack", transform, circleSprite, plssPackColor, order++);
        shoulders = CreatePart("ApolloShoulders", transform, circleSprite, suitFabricColor, order++);

        shoulderPatchBlue = CreatePart("PatchFlagBlue", shoulders.transform, circleSprite, flagBlue, order++);
        shoulderPatchFlag = CreatePart("PatchFlagRed", shoulders.transform, circleSprite, flagRed, order++);

        chestControlUnit = CreatePart("RCU_ChestPack", shoulders.transform, circleSprite, chestPackColor, order++);
        rcuConnectorR1 = CreatePart("RCS_RedConnector1", chestControlUnit.transform, circleSprite, connectorRed, order++);
        rcuConnectorR2 = CreatePart("RCS_RedConnector2", chestControlUnit.transform, circleSprite, connectorRed, order++);
        rcuConnectorB1 = CreatePart("RCS_BlueConnector1", chestControlUnit.transform, circleSprite, connectorBlue, order++);
        rcuConnectorB2 = CreatePart("RCS_BlueConnector2", chestControlUnit.transform, circleSprite, connectorBlue, order++);

        neck = CreatePart("Neck", transform, circleSprite, skinColor * 0.88f, order++);
        neckRingBase = CreatePart("NeckRingBase", transform, circleSprite, neckRingBaseColor, order++);
        neckRingBlue = CreatePart("NeckRingBlueLock", neckRingBase.transform, circleSprite, neckRingBlueColor, order++);

        // --- 2. HEAD & EARS ---
        leftEar = CreatePart("LeftEar", transform, circleSprite, skinColor * 0.95f, order++);
        rightEar = CreatePart("RightEar", transform, circleSprite, skinColor * 0.95f, order++);
        head = CreatePart("Head", transform, circleSprite, skinColor, order++);
        chinShadow = CreatePart("ChinShadow", head.transform, circleSprite, skinColor * 0.90f, order++);
        cheekL = CreatePart("CheekL", head.transform, circleSprite, skinColor * 1.04f, order++);
        cheekR = CreatePart("CheekR", head.transform, circleSprite, skinColor * 1.04f, order++);

        // --- 3. AGING LAYERS ---
        foreheadWrinkle1 = CreatePart("ForeheadWrinkle1", head.transform, circleSprite, Color.clear, order++);
        foreheadWrinkle2 = CreatePart("ForeheadWrinkle2", head.transform, circleSprite, Color.clear, order++);
        crowsFeetL = CreatePart("CrowsFeetL", head.transform, circleSprite, Color.clear, order++);
        crowsFeetR = CreatePart("CrowsFeetR", head.transform, circleSprite, Color.clear, order++);
        nasolabialFoldL = CreatePart("NasolabialFoldL", head.transform, circleSprite, Color.clear, order++);
        nasolabialFoldR = CreatePart("NasolabialFoldR", head.transform, circleSprite, Color.clear, order++);
        ageSpot1 = CreatePart("AgeSpot1", head.transform, circleSprite, Color.clear, order++);
        ageSpot2 = CreatePart("AgeSpot2", head.transform, circleSprite, Color.clear, order++);

        // --- 4. EYES (FIXED HIERARCHY) ---
        leftEye = CreatePart("LeftEye", head.transform, circleSprite, Color.white, order++);
        rightEye = CreatePart("RightEye", head.transform, circleSprite, Color.white, order++);

        irisRingL = CreatePart("IrisRingL", leftEye.transform, circleSprite, eyeColor, order++);
        irisRingR = CreatePart("IrisRingR", rightEye.transform, circleSprite, eyeColor, order++);
        pupilL = CreatePart("PupilL", irisRingL.transform, circleSprite, new Color(0.1f, 0.1f, 0.1f), order++);
        pupilR = CreatePart("PupilR", irisRingR.transform, circleSprite, new Color(0.1f, 0.1f, 0.1f), order++);

        highlightL1 = CreatePart("HighlightL1", leftEye.transform, circleSprite, Color.white, order++);
        highlightR1 = CreatePart("HighlightR1", rightEye.transform, circleSprite, Color.white, order++);
        highlightL2 = CreatePart("HighlightL2", leftEye.transform, circleSprite, Color.white, order++);
        highlightR2 = CreatePart("HighlightR2", rightEye.transform, circleSprite, Color.white, order++);

        upperLidL = CreatePart("UpperLidL", leftEye.transform, circleSprite, skinColor * 0.85f, order++);
        upperLidR = CreatePart("UpperLidR", rightEye.transform, circleSprite, skinColor * 0.85f, order++);
        eyeBagL = CreatePart("EyeBagL", leftEye.transform, circleSprite, skinColor * 0.82f, order++);
        eyeBagR = CreatePart("EyeBagR", rightEye.transform, circleSprite, skinColor * 0.82f, order++);

        // --- 5. NOSE & MOUTH ---
        noseBridge = CreatePart("NoseBridge", head.transform, circleSprite, skinColor * 0.94f, order++);
        noseTip = CreatePart("NoseTip", head.transform, circleSprite, skinColor * 0.88f, order++);
        noseNostrilL = CreatePart("NostrilL", noseTip.transform, circleSprite, skinColor * 0.72f, order++);
        noseNostrilR = CreatePart("NostrilR", noseTip.transform, circleSprite, skinColor * 0.72f, order++);

        upperLip = CreatePart("UpperLip", head.transform, circleSprite, new Color(0.68f, 0.38f, 0.38f), order++);
        lowerLip = CreatePart("LowerLip", head.transform, circleSprite, new Color(0.75f, 0.45f, 0.45f), order++);
        mouthCornerL = CreatePart("MouthCornerL", head.transform, circleSprite, new Color(0.50f, 0.25f, 0.25f), order++);
        mouthCornerR = CreatePart("MouthCornerR", head.transform, circleSprite, new Color(0.50f, 0.25f, 0.25f), order++);

        // --- 6. BROWS & SINGLE HAIR OBJECT ---
        leftBrow = CreatePart("LeftBrow", head.transform, circleSprite, hairColor, order++);
        rightBrow = CreatePart("RightBrow", head.transform, circleSprite, hairColor, order++);

        // Single Hair Object Layer
        simpleHair = CreatePart("SimpleHair", head.transform, circleSprite, hairColor, order++);
    }

    private void UpdateCharacter(float t)
    {
        Color currentSkinColor = Color.Lerp(skinColor, Color.Lerp(skinColor, new Color(0.82f, 0.78f, 0.75f), 0.45f), t);
        head.GetComponent<Image>().color = currentSkinColor;

        // --- 1. SPACESUIT ---
        float fixedShoulderW = 410f;
        float fixedShoulderH = 175f;

        ApplyUITransform(plssBackpack, new Vector2(0f, -125f), new Vector2(fixedShoulderW * 0.82f, 210f), plssPackColor);
        ApplyUITransform(shoulders, new Vector2(0f, -155f), new Vector2(fixedShoulderW, fixedShoulderH), suitFabricColor);

        ApplyUITransform(shoulderPatchFlag, new Vector2(-fixedShoulderW * 0.38f, 20f), new Vector2(32f, 22f), flagRed);
        ApplyUITransform(shoulderPatchBlue, new Vector2(-fixedShoulderW * 0.38f - 8f, 25f), new Vector2(14f, 12f), flagBlue);

        ApplyUITransform(chestControlUnit, new Vector2(0f, -12f), new Vector2(120f, 95f), chestPackColor);
        ApplyUITransform(rcuConnectorR1, new Vector2(-32f, 15f), new Vector2(22f, 22f), connectorRed);
        ApplyUITransform(rcuConnectorR2, new Vector2(-32f, -18f), new Vector2(22f, 22f), connectorRed);
        ApplyUITransform(rcuConnectorB1, new Vector2(32f, 15f), new Vector2(22f, 22f), connectorBlue);
        ApplyUITransform(rcuConnectorB2, new Vector2(32f, -18f), new Vector2(22f, 22f), connectorBlue);

        float fixedNeckW = 125f;
        float fixedNeckH = 80f;
        ApplyUITransform(neck, new Vector2(0f, -80f), new Vector2(fixedNeckW, fixedNeckH), currentSkinColor * 0.88f);
        ApplyUITransform(neckRingBase, new Vector2(0f, -68f), new Vector2(fixedNeckW * 1.30f, 38f), neckRingBaseColor);
        ApplyUITransform(neckRingBlue, new Vector2(0f, 4f), new Vector2(fixedNeckW * 1.25f, 16f), neckRingBlueColor);

        // --- 2. FACIAL GEOMETRY ---
        float baseHeadW = 185f * gene_HeadWidthRatio;
        float baseHeadH = 215f * gene_HeadHeightRatio;

        Vector2 shapeScale = Vector2.one;
        switch (faceShape)
        {
            case FaceShape.SoftSquare: shapeScale = new Vector2(1.14f, 0.92f); break;
            case FaceShape.Round: shapeScale = new Vector2(1.12f, 1.08f); break;
            case FaceShape.Heart: shapeScale = new Vector2(1.08f, 0.88f); break;
            case FaceShape.Chiseled: shapeScale = new Vector2(0.90f, 1.18f); break;
            case FaceShape.LongRectangular: shapeScale = new Vector2(0.85f, 1.25f); break;
            case FaceShape.SoftOval: shapeScale = new Vector2(1.0f, 1.0f); break;
        }

        float headW = baseHeadW * shapeScale.x;
        float headH = baseHeadH * shapeScale.y;
        ApplyUITransform(head, Vector2.zero, new Vector2(headW, headH), currentSkinColor);

        float chinShadowH = Mathf.Lerp(20f, 38f, t);
        ApplyUITransform(chinShadow, new Vector2(0f, -headH * 0.38f), new Vector2(headW * 0.45f, chinShadowH), currentSkinColor * 0.88f);

        Color cheekColor = currentSkinColor;
        cheekColor.r = Mathf.Min(1f, cheekColor.r * 1.04f);
        float cheekW = 40f * gene_CheekProminence;
        ApplyUITransform(cheekL, new Vector2(-headW * 0.32f, -8f), new Vector2(cheekW, 35f), cheekColor);
        ApplyUITransform(cheekR, new Vector2(headW * 0.32f, -8f), new Vector2(cheekW, 35f), cheekColor);

        float earH = 55f * gene_EarSize * Mathf.Lerp(1.0f, 1.20f, t);
        float earY = gene_EarYOffset;
        ApplyUITransform(leftEar, new Vector2(-headW * 0.49f, earY), new Vector2(32f * gene_EarSize, earH), currentSkinColor * 0.95f);
        ApplyUITransform(rightEar, new Vector2(headW * 0.49f, earY), new Vector2(32f * gene_EarSize, earH), currentSkinColor * 0.95f);

        // --- 3. AGING ---
        UpdateAgingDetails(headW, headH, t, currentSkinColor);

        // --- 4. FIXED EYES ---
        float eyeY = Mathf.Lerp(8f, 2f, t);
        float eyeSpacing = 44f + gene_EyeSpacing;
        float eyeSize = 38f * gene_EyeSize;

        PositionEyeUI(leftEye, irisRingL, pupilL, highlightL1, highlightL2, upperLidL, eyeBagL, -eyeSpacing, eyeY, eyeSize, t, currentSkinColor);
        PositionEyeUI(rightEye, irisRingR, pupilR, highlightR1, highlightR2, upperLidR, eyeBagR, eyeSpacing, eyeY, eyeSize, t, currentSkinColor);

        leftEye.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, gene_EyeTilt);
        rightEye.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -gene_EyeTilt);

        // --- 5. BROWS & SINGLE HAIR OBJECT ---
        Color currentHair = Color.Lerp(hairColor, new Color(0.85f, 0.85f, 0.88f), Mathf.Clamp01((t - 0.25f) * 1.35f));
        ApplyFacialExpression(eyeY, eyeSpacing, t, currentHair);

        // Single Hair Cap Object
        float hairW = headW * 1.06f * gene_HairVolume;
        float hairH = headH * 0.45f * gene_HairVolume;
        float hairY = headH * 0.38f;
        ApplyUITransform(simpleHair, new Vector2(0f, hairY), new Vector2(hairW, hairH), currentHair);

        // --- 6. NOSE ---
        float noseY = -12f;
        float bridgeHeight = (28f * gene_NoseLength) * Mathf.Lerp(1.0f, 1.15f, t);
        float bridgeW = (gene_NoseType == 1 ? 6f : 9f) * gene_NoseWidth;
        ApplyUITransform(noseBridge, new Vector2(0f, noseY + (bridgeHeight * 0.40f)), new Vector2(bridgeW, bridgeHeight), currentSkinColor * 0.92f);

        float noseWidth = 28f * gene_NoseWidth;
        float tipHeight = (gene_NoseType == 2 ? 24f : 18f);
        ApplyUITransform(noseTip, new Vector2(0f, noseY - (bridgeHeight * 0.15f)), new Vector2(noseWidth, tipHeight), currentSkinColor * 0.86f);
        ApplyUITransform(noseNostrilL, new Vector2(-noseWidth * 0.35f, -1f), new Vector2(5f, 4f), currentSkinColor * 0.70f);
        ApplyUITransform(noseNostrilR, new Vector2(noseWidth * 0.35f, -1f), new Vector2(5f, 4f), currentSkinColor * 0.70f);
    }

    private void UpdateAgingDetails(float headW, float headH, float t, Color skinCol)
    {
        Color wrinkleColor = skinCol * 0.72f;
        wrinkleColor.a = Mathf.Clamp01((t - 0.30f) * 2.2f);

        Color ageSpotColor = skinCol * 0.65f;
        ageSpotColor.a = Mathf.Clamp01((t - 0.55f) * 2.2f);

        ApplyUITransform(foreheadWrinkle1, new Vector2(0f, headH * 0.22f), new Vector2(headW * 0.55f, 4f), wrinkleColor);
        ApplyUITransform(foreheadWrinkle2, new Vector2(0f, headH * 0.28f), new Vector2(headW * 0.42f, 3.5f), wrinkleColor);

        ApplyUITransform(crowsFeetL, new Vector2(-headW * 0.38f, 5f), new Vector2(18f, 14f), wrinkleColor);
        ApplyUITransform(crowsFeetR, new Vector2(headW * 0.38f, 5f), new Vector2(18f, 14f), wrinkleColor);

        ApplyUITransform(nasolabialFoldL, new Vector2(-28f, -32f), new Vector2(6f, Mathf.Lerp(10f, 38f, t)), wrinkleColor);
        ApplyUITransform(nasolabialFoldR, new Vector2(28f, -32f), new Vector2(6f, Mathf.Lerp(10f, 38f, t)), wrinkleColor);

        nasolabialFoldL.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -18f);
        nasolabialFoldR.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 18f);

        ApplyUITransform(ageSpot1, new Vector2(-headW * 0.28f, headH * 0.18f), new Vector2(12f, 9f), ageSpotColor);
        ApplyUITransform(ageSpot2, new Vector2(headW * 0.22f, -headH * 0.15f), new Vector2(15f, 11f), ageSpotColor);
    }

    private void PositionEyeUI(GameObject eye, GameObject iris, GameObject pupil, GameObject h1, GameObject h2, GameObject upperLid, GameObject eyeBag, float x, float y, float size, float ageVal, Color skinCol)
    {
        // 1. Eye Socket (Sclera / Eyeball Base)
        float socketW = size * 1.1f;
        float socketH = size * 0.72f * gene_EyeScaleY;
        ApplyUITransform(eye, new Vector2(x, y), new Vector2(socketW, socketH), Color.white);

        // 2. Iris & Pupil (Centered cleanly inside socket)
        float irisSize = socketH * 0.75f;
        ApplyUITransform(iris, Vector2.zero, new Vector2(irisSize, irisSize), eyeColor);

        float pupilSize = irisSize * 0.50f;
        ApplyUITransform(pupil, Vector2.zero, new Vector2(pupilSize, pupilSize), new Color(0.1f, 0.1f, 0.1f));

        // 3. Highlights
        ApplyUITransform(h1, new Vector2(irisSize * 0.18f, irisSize * 0.18f), new Vector2(irisSize * 0.25f, irisSize * 0.25f), Color.white);
        ApplyUITransform(h2, new Vector2(-irisSize * 0.15f, -irisSize * 0.15f), new Vector2(irisSize * 0.14f, irisSize * 0.14f), new Color(1f, 1f, 1f, 0.7f));

        // 4. Upper Eyelid (Droops naturally from above without clipping into iris center)
        float lidDroop = Mathf.Lerp(0.0f, socketH * 0.35f, ageVal);
        ApplyUITransform(upperLid, new Vector2(0f, (socketH * 0.38f) - (lidDroop * 0.5f)), new Vector2(socketW * 1.05f, (socketH * 0.40f) + lidDroop), skinCol * 0.85f);

        // 5. Lower Eye Bag
        Color bagColor = skinCol * 0.72f;
        bagColor.a = Mathf.Clamp01((ageVal - 0.25f) * 1.8f);
        float bagHeight = Mathf.Lerp(2f, 10f, ageVal);
        ApplyUITransform(eyeBag, new Vector2(0f, -socketH * 0.52f), new Vector2(socketW * 0.95f, bagHeight), bagColor);
    }

    private void ApplyFacialExpression(float eyeY, float eyeSpacing, float t, Color browColor)
    {
        float browY = eyeY + 16f;
        float mouthY = -48f;

        float browAngle = gene_BrowAngleOffset;
        float browOffset = 0f;

        Color upperLipCol = new Color(0.68f, 0.38f, 0.38f);
        Color lowerLipCol = new Color(0.75f, 0.45f, 0.45f);

        float ageLipFactor = Mathf.Lerp(1.0f, 0.65f, t);
        float lipWidth = 44f * gene_LipWidth;
        float upperLipH = 6f * gene_LipFullness * ageLipFactor;
        float lowerLipH = 8f * gene_LipFullness * ageLipFactor;

        switch (expression)
        {
            case Expression.FriendlyNeutral:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 2f), new Vector2(lipWidth, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.88f, lowerLipH), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.5f, mouthY + 2f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.5f, mouthY + 2f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                break;

            case Expression.Happy:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 3f), new Vector2(lipWidth * 1.1f, upperLipH + 1f), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 4f), new Vector2(lipWidth * 1.02f, lowerLipH + 3f), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.55f, mouthY + 6f), new Vector2(5f, 5f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.55f, mouthY + 6f), new Vector2(5f, 5f), upperLipCol * 0.8f);
                browAngle += -6f;
                browOffset = 2f;
                break;

            case Expression.Gentle:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth * 0.95f, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.85f, lowerLipH), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.48f, mouthY + 1f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.48f, mouthY + 1f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                browAngle += 4f;
                break;

            case Expression.Focused:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 2f), new Vector2(lipWidth * 0.85f, lowerLipH - 1f), lowerLipCol);
                browAngle += -10f;
                browOffset = -2f;
                break;

            case Expression.Surprised:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 4f), new Vector2(26f * gene_LipWidth, 8f), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 4f), new Vector2(22f * gene_LipWidth, 12f), lowerLipCol);
                browOffset = 8f;
                break;
        }

        Vector2 browSize = new Vector2(36f * gene_LipWidth, gene_BrowThickness);
        ApplyUITransform(leftBrow, new Vector2(-eyeSpacing, browY + browOffset), browSize, browColor);
        ApplyUITransform(rightBrow, new Vector2(eyeSpacing, browY + browOffset), browSize, browColor);

        leftBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, browAngle);
        rightBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -browAngle);
    }

    [ContextMenu("Die")]
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (grayscaleMaterial == null)
        {
            grayscaleMaterial = CreateDefaultGrayscaleMaterial();
        }

        Image[] images = GetComponentsInChildren<Image>(true);
        originalMaterials.Clear();

        foreach (Image img in images)
        {
            if (img != null)
            {
                originalMaterials[img] = img.material;
                img.material = grayscaleMaterial;
            }
        }
    }

    public void Revive()
    {
        if (!isDead) return;
        isDead = false;

        foreach (var pair in originalMaterials)
        {
            if (pair.Key != null)
            {
                pair.Key.material = pair.Value;
            }
        }
        originalMaterials.Clear();
    }

    private Material CreateDefaultGrayscaleMaterial()
    {
        Shader shader = Shader.Find("UI/Default");
        Material mat = new Material(shader);
        return mat;
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

    private Sprite CreateCircleSprite()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        float cornerRadius = basicShapeCornerRadius;
        float halfSize = size / 2f;
        Vector2 boxHalfSize = new Vector2(halfSize - cornerRadius, halfSize - cornerRadius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x - halfSize + 0.5f, y - halfSize + 0.5f);
                Vector2 d = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - boxHalfSize;
                float dist = Vector2.Max(d, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0f) - cornerRadius;

                float alpha = Mathf.Clamp01(0.5f - dist);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
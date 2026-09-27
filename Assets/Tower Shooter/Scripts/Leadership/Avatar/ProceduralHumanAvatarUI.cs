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

    public enum Expression { ConfidentLeader, Determined, Commanding, Noble, FriendlyNeutral }
    public Expression expression = Expression.ConfidentLeader;

    [Header("Customize")]
    public float basicShapeCornerRadius = 24;

    [Header("Manual Seed (Optional)")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    [Header("Death / Grayscale State")]
    public Material grayscaleMaterial;
    private bool isDead = false;
    private Dictionary<Image, Material> originalMaterials = new Dictionary<Image, Material>();

    public enum FaceShape { SoftOval, SoftSquare, Chiseled, CommandingBlock }

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

    private FaceShape faceShape;

    // Genetic Parameters (Leader-tuned)
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

    // --- RENDER GAME OBJECTS ---
    private GameObject plssBackpack, shoulders, shoulderPatchFlag, shoulderPatchBlue;
    private GameObject chestControlUnit, rcuConnectorR1, rcuConnectorR2, rcuConnectorB1, rcuConnectorB2;
    private GameObject neck, neckRingBase, neckRingBlue;

    private GameObject head, leftEar, rightEar, simpleHair;
    private GameObject chinShadow, cheekL, cheekR;

    private GameObject foreheadWrinkle1, foreheadWrinkle2;
    private GameObject crowsFeetL, crowsFeetR;
    private GameObject nasolabialFoldL, nasolabialFoldR;
    private GameObject ageSpot1, ageSpot2;

    private GameObject leftEye, rightEye, pupilL, pupilR, irisRingL, irisRingR;
    private GameObject highlightL1, highlightR1;
    private GameObject upperLidL, upperLidR;
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
        if (circleSprite == null) circleSprite = CreateCircleSprite();

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
        if (skinToneRoll < 0.30f) skinColor = new Color(0.96f, 0.84f, 0.74f);
        else if (skinToneRoll < 0.60f) skinColor = new Color(0.82f, 0.62f, 0.46f);
        else if (skinToneRoll < 0.85f) skinColor = new Color(0.52f, 0.35f, 0.22f);
        else skinColor = new Color(0.28f, 0.18f, 0.12f);

        float hairRoll = Random.value;
        if (hairRoll < 0.40f) hairColor = new Color(0.12f, 0.10f, 0.08f); // Dark silver/black
        else if (hairRoll < 0.70f) hairColor = new Color(0.30f, 0.20f, 0.12f); // Dark brown
        else hairColor = new Color(0.55f, 0.55f, 0.58f); // Commander grey

        eyeColor = Random.ColorHSV(0.55f, 0.65f, 0.5f, 0.8f, 0.4f, 0.8f); // Steel Blue / Gray eyes
        faceShape = (FaceShape)Random.Range(0, System.Enum.GetValues(typeof(FaceShape)).Length);

        // Leader proportions: Balanced & strong jawlines
        gene_HeadWidthRatio = Random.Range(0.95f, 1.08f);
        gene_HeadHeightRatio = Random.Range(0.96f, 1.06f);

        gene_EyeSpacing = Random.Range(-2f, 6f);
        gene_EyeScaleY = Random.Range(0.85f, 1.0f);
        gene_EyeSize = Random.Range(0.90f, 1.05f);

        gene_NoseWidth = Random.Range(0.85f, 1.15f);
        gene_NoseLength = Random.Range(0.90f, 1.10f);

        gene_LipWidth = Random.Range(0.90f, 1.15f);
        gene_LipFullness = Random.Range(0.80f, 1.10f);

        gene_EarSize = Random.Range(0.85f, 1.10f);
        gene_EarYOffset = Random.Range(-4f, 4f);

        gene_BrowThickness = Random.Range(6f, 10f);
        gene_BrowAngleOffset = Random.Range(-6f, 2f); // Sharp, focused brows
        gene_CheekProminence = Random.Range(0.85f, 1.15f);
        gene_HairVolume = Random.Range(0.90f, 1.10f);
    }

    private void BuildHierarchy()
    {
        int order = 0;

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

        leftEar = CreatePart("LeftEar", transform, circleSprite, skinColor * 0.95f, order++);
        rightEar = CreatePart("RightEar", transform, circleSprite, skinColor * 0.95f, order++);
        head = CreatePart("Head", transform, circleSprite, skinColor, order++);
        chinShadow = CreatePart("ChinShadow", head.transform, circleSprite, skinColor * 0.90f, order++);
        cheekL = CreatePart("CheekL", head.transform, circleSprite, skinColor * 1.03f, order++);
        cheekR = CreatePart("CheekR", head.transform, circleSprite, skinColor * 1.03f, order++);

        foreheadWrinkle1 = CreatePart("ForeheadWrinkle1", head.transform, circleSprite, Color.clear, order++);
        foreheadWrinkle2 = CreatePart("ForeheadWrinkle2", head.transform, circleSprite, Color.clear, order++);
        crowsFeetL = CreatePart("CrowsFeetL", head.transform, circleSprite, Color.clear, order++);
        crowsFeetR = CreatePart("CrowsFeetR", head.transform, circleSprite, Color.clear, order++);
        nasolabialFoldL = CreatePart("NasolabialFoldL", head.transform, circleSprite, Color.clear, order++);
        nasolabialFoldR = CreatePart("NasolabialFoldR", head.transform, circleSprite, Color.clear, order++);
        ageSpot1 = CreatePart("AgeSpot1", head.transform, circleSprite, Color.clear, order++);
        ageSpot2 = CreatePart("AgeSpot2", head.transform, circleSprite, Color.clear, order++);

        leftEye = CreatePart("LeftEye", head.transform, circleSprite, Color.white, order++);
        rightEye = CreatePart("RightEye", head.transform, circleSprite, Color.white, order++);

        irisRingL = CreatePart("IrisRingL", leftEye.transform, circleSprite, eyeColor, order++);
        irisRingR = CreatePart("IrisRingR", rightEye.transform, circleSprite, eyeColor, order++);
        pupilL = CreatePart("PupilL", irisRingL.transform, circleSprite, new Color(0.1f, 0.1f, 0.1f), order++);
        pupilR = CreatePart("PupilR", irisRingR.transform, circleSprite, new Color(0.1f, 0.1f, 0.1f), order++);

        highlightL1 = CreatePart("HighlightL1", leftEye.transform, circleSprite, Color.white, order++);
        highlightR1 = CreatePart("HighlightR1", rightEye.transform, circleSprite, Color.white, order++);

        upperLidL = CreatePart("UpperLidL", leftEye.transform, circleSprite, skinColor * 0.85f, order++);
        upperLidR = CreatePart("UpperLidR", rightEye.transform, circleSprite, skinColor * 0.85f, order++);

        noseBridge = CreatePart("NoseBridge", head.transform, circleSprite, skinColor * 0.94f, order++);
        noseTip = CreatePart("NoseTip", head.transform, circleSprite, skinColor * 0.86f, order++);
        noseNostrilL = CreatePart("NostrilL", noseTip.transform, circleSprite, skinColor * 0.72f, order++);
        noseNostrilR = CreatePart("NostrilR", noseTip.transform, circleSprite, skinColor * 0.72f, order++);

        upperLip = CreatePart("UpperLip", head.transform, circleSprite, new Color(0.65f, 0.38f, 0.38f), order++);
        lowerLip = CreatePart("LowerLip", head.transform, circleSprite, new Color(0.72f, 0.42f, 0.42f), order++);
        mouthCornerL = CreatePart("MouthCornerL", head.transform, circleSprite, new Color(0.48f, 0.25f, 0.25f), order++);
        mouthCornerR = CreatePart("MouthCornerR", head.transform, circleSprite, new Color(0.48f, 0.25f, 0.25f), order++);

        leftBrow = CreatePart("LeftBrow", head.transform, circleSprite, hairColor, order++);
        rightBrow = CreatePart("RightBrow", head.transform, circleSprite, hairColor, order++);

        simpleHair = CreatePart("SimpleHair", head.transform, circleSprite, hairColor, order++);
    }

    private void UpdateCharacter(float t)
    {
        Color currentSkinColor = Color.Lerp(skinColor, Color.Lerp(skinColor, new Color(0.82f, 0.78f, 0.75f), 0.45f), t);
        head.GetComponent<Image>().color = currentSkinColor;

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

        // --- FACIAL GEOMETRY (Solid, regal proportions) ---
        float baseHeadW = 185f * gene_HeadWidthRatio;
        float baseHeadH = 210f * gene_HeadHeightRatio;

        Vector2 shapeScale = Vector2.one;
        switch (faceShape)
        {
            case FaceShape.SoftSquare: shapeScale = new Vector2(1.04f, 0.98f); break;
            case FaceShape.Chiseled: shapeScale = new Vector2(0.98f, 1.04f); break;
            case FaceShape.CommandingBlock: shapeScale = new Vector2(1.06f, 1.02f); break;
            case FaceShape.SoftOval: shapeScale = new Vector2(1.0f, 1.0f); break;
        }

        float headW = baseHeadW * shapeScale.x;
        float headH = baseHeadH * shapeScale.y;
        ApplyUITransform(head, Vector2.zero, new Vector2(headW, headH), currentSkinColor);

        float chinShadowH = Mathf.Lerp(22f, 34f, t);
        ApplyUITransform(chinShadow, new Vector2(0f, -headH * 0.38f), new Vector2(headW * 0.50f, chinShadowH), currentSkinColor * 0.88f);

        Color cheekColor = currentSkinColor;
        cheekColor.r = Mathf.Min(1f, cheekColor.r * 1.03f);
        float cheekW = 32f * gene_CheekProminence;
        ApplyUITransform(cheekL, new Vector2(-headW * 0.33f, -5f), new Vector2(cheekW, 28f), cheekColor);
        ApplyUITransform(cheekR, new Vector2(headW * 0.33f, -5f), new Vector2(cheekW, 28f), cheekColor);

        float earH = 48f * gene_EarSize;
        float earY = gene_EarYOffset;
        ApplyUITransform(leftEar, new Vector2(-headW * 0.49f, earY), new Vector2(24f * gene_EarSize, earH), currentSkinColor * 0.95f);
        ApplyUITransform(rightEar, new Vector2(headW * 0.49f, earY), new Vector2(24f * gene_EarSize, earH), currentSkinColor * 0.95f);

        UpdateAgingDetails(headW, headH, t, currentSkinColor);

        // --- FIXED EYES ---
        float eyeY = 12f;
        float eyeSpacing = 42f + gene_EyeSpacing;
        float eyeSize = 34f * gene_EyeSize;

        PositionEyeUI(leftEye, irisRingL, pupilL, highlightL1, upperLidL, -eyeSpacing, eyeY, eyeSize, currentSkinColor);
        PositionEyeUI(rightEye, irisRingR, pupilR, highlightR1, upperLidR, eyeSpacing, eyeY, eyeSize, currentSkinColor);

        leftEye.GetComponent<RectTransform>().localRotation = Quaternion.identity;
        rightEye.GetComponent<RectTransform>().localRotation = Quaternion.identity;

        // --- HAIR (THINNER & HORIZONTALLY SCALED CORRECTLY) ---
        Color currentHair = Color.Lerp(hairColor, new Color(0.82f, 0.82f, 0.85f), Mathf.Clamp01((t - 0.3f) * 1.2f));
        ApplyFacialExpression(eyeY, eyeSpacing, t, currentHair);

        // Thin, sharp military style hair cut matching top skull outline exactly
        float hairW = headW * 0.96f * gene_HairVolume;
        float hairH = headH * 0.16f; // Thinner profile
        float hairY = (headH * 0.50f) - (hairH * 0.45f); // Anchored cleanly to the crown
        ApplyUITransform(simpleHair, new Vector2(0f, hairY), new Vector2(hairW, hairH), currentHair);

        // --- NOSE (Strong Bridge) ---
        float noseY = -8f;
        float bridgeHeight = 28f * gene_NoseLength;
        float bridgeW = 8f * gene_NoseWidth;
        ApplyUITransform(noseBridge, new Vector2(0f, noseY + (bridgeHeight * 0.38f)), new Vector2(bridgeW, bridgeHeight), currentSkinColor * 0.92f);

        float noseWidth = 24f * gene_NoseWidth;
        float tipHeight = 16f;
        ApplyUITransform(noseTip, new Vector2(0f, noseY - (bridgeHeight * 0.15f)), new Vector2(noseWidth, tipHeight), currentSkinColor * 0.86f);
        ApplyUITransform(noseNostrilL, new Vector2(-noseWidth * 0.35f, -1f), new Vector2(5f, 4f), currentSkinColor * 0.70f);
        ApplyUITransform(noseNostrilR, new Vector2(noseWidth * 0.35f, -1f), new Vector2(5f, 4f), currentSkinColor * 0.70f);
    }

    private void UpdateAgingDetails(float headW, float headH, float t, Color skinCol)
    {
        Color wrinkleColor = skinCol * 0.75f;
        wrinkleColor.a = Mathf.Clamp01((t - 0.35f) * 2.0f);

        Color ageSpotColor = skinCol * 0.68f;
        ageSpotColor.a = Mathf.Clamp01((t - 0.60f) * 2.0f);

        ApplyUITransform(foreheadWrinkle1, new Vector2(0f, headH * 0.22f), new Vector2(headW * 0.50f, 3.5f), wrinkleColor);
        ApplyUITransform(foreheadWrinkle2, new Vector2(0f, headH * 0.28f), new Vector2(headW * 0.38f, 3f), wrinkleColor);

        ApplyUITransform(crowsFeetL, new Vector2(-headW * 0.38f, 10f), new Vector2(14f, 10f), wrinkleColor);
        ApplyUITransform(crowsFeetR, new Vector2(headW * 0.38f, 10f), new Vector2(14f, 10f), wrinkleColor);

        ApplyUITransform(nasolabialFoldL, new Vector2(-26f, -30f), new Vector2(4f, Mathf.Lerp(8f, 30f, t)), wrinkleColor);
        ApplyUITransform(nasolabialFoldR, new Vector2(26f, -30f), new Vector2(4f, Mathf.Lerp(8f, 30f, t)), wrinkleColor);

        nasolabialFoldL.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -15f);
        nasolabialFoldR.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 15f);

        ApplyUITransform(ageSpot1, new Vector2(-headW * 0.28f, headH * 0.18f), new Vector2(10f, 8f), ageSpotColor);
        ApplyUITransform(ageSpot2, new Vector2(headW * 0.22f, -headH * 0.15f), new Vector2(12f, 9f), ageSpotColor);
    }

    private void PositionEyeUI(GameObject eye, GameObject iris, GameObject pupil, GameObject h1, GameObject upperLid, float x, float y, float size, Color skinCol)
    {
        float socketW = size * 1.15f;
        float socketH = size * 0.70f * gene_EyeScaleY;
        ApplyUITransform(eye, new Vector2(x, y), new Vector2(socketW, socketH), Color.white);

        // Iris centered perfectly
        float irisSize = socketH * 0.80f;
        ApplyUITransform(iris, Vector2.zero, new Vector2(irisSize, irisSize), eyeColor);

        float pupilSize = irisSize * 0.48f;
        ApplyUITransform(pupil, Vector2.zero, new Vector2(pupilSize, pupilSize), new Color(0.1f, 0.1f, 0.1f));

        ApplyUITransform(h1, new Vector2(irisSize * 0.20f, irisSize * 0.20f), new Vector2(irisSize * 0.25f, irisSize * 0.25f), Color.white);

        // Subtle lid outline that doesn't obscure pupil center
        ApplyUITransform(upperLid, new Vector2(0f, socketH * 0.42f), new Vector2(socketW * 1.05f, socketH * 0.20f), skinCol * 0.85f);
    }

    private void ApplyFacialExpression(float eyeY, float eyeSpacing, float t, Color browColor)
    {
        float browY = eyeY + 15f;
        float mouthY = -48f;

        Color upperLipCol = new Color(0.65f, 0.38f, 0.38f);
        Color lowerLipCol = new Color(0.72f, 0.42f, 0.42f);

        float lipWidth = 46f * gene_LipWidth;
        float upperLipH = 6f * gene_LipFullness;
        float lowerLipH = 7f * gene_LipFullness;

        float browAngleL = -3f;
        float browAngleR = 3f;
        float browOffsetL = 0f;
        float browOffsetR = 0f;

        switch (expression)
        {
            case Expression.ConfidentLeader:
                // Determined brow tilt + disciplined mouth
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.90f, lowerLipH), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.5f, mouthY + 2f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.5f, mouthY + 3f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                browAngleL = -8f;
                browAngleR = 8f;
                browOffsetL = -1f;
                browOffsetR = -1f;
                break;

            case Expression.Determined:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY), new Vector2(lipWidth * 0.95f, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.85f, lowerLipH), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.48f, mouthY - 1f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.48f, mouthY - 1f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                browAngleL = -12f;
                browAngleR = 12f;
                browOffsetL = -3f;
                browOffsetR = -3f;
                break;

            case Expression.Commanding:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth * 1.05f, upperLipH + 1f), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.92f, lowerLipH), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.52f, mouthY + 1f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.52f, mouthY + 1f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                browAngleL = -6f;
                browAngleR = 14f; // Raised brow for authority
                browOffsetR = 3f;
                break;

            case Expression.Noble:
            case Expression.FriendlyNeutral:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth, upperLipH), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.88f, lowerLipH), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.48f, mouthY + 2f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.48f, mouthY + 2f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                break;
        }

        Vector2 browSize = new Vector2(34f * gene_LipWidth, gene_BrowThickness);
        ApplyUITransform(leftBrow, new Vector2(-eyeSpacing, browY + browOffsetL), browSize, browColor);
        ApplyUITransform(rightBrow, new Vector2(eyeSpacing, browY + browOffsetR), browSize, browColor);

        leftBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, browAngleL + gene_BrowAngleOffset);
        rightBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, browAngleR - gene_BrowAngleOffset);
    }

    [ContextMenu("Die")]
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (grayscaleMaterial == null) grayscaleMaterial = CreateDefaultGrayscaleMaterial();

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
            if (pair.Key != null) pair.Key.material = pair.Value;
        }
        originalMaterials.Clear();
    }

    private Material CreateDefaultGrayscaleMaterial()
    {
        Shader shader = Shader.Find("UI/Default");
        return new Material(shader);
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
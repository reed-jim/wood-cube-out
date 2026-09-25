using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class ProceduralHumanAvatarUI : MonoBehaviour
{
    [Header("Growth & Expression")]
    [Range(0f, 1f)]
    [Tooltip("0.0 = Baby, 0.3 = Child, 0.6 = Adult, 1.0 = Elder")]
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

    public enum HairType { Bald, ShortCrop, MessyBangs, SoftWaves, BuzzCut, SidePart }
    public enum BeardType { None, LightStubble, NeatGoatee }

    // --- CHANGED: Leader Outfit Types ---
    public enum LeaderOutfitType { ClassicBlackTie, ExecutiveNavy, ModernCharcoal, RegalBurgundy }
    public enum FaceShape { SoftOval, Round, Heart, SoftSquare }

    // --- GENETIC & CLOTHING DATA ---
    private Color skinColor;
    private Color eyeColor;
    private Color naturalHairColor;

    // --- CHANGED: Leader Attire Colors ---
    private Color suitColor;
    private Color shirtColor;
    private Color tieColor;
    private Color lapelColor;

    private HairType hairStyle;
    private BeardType beardStyle;
    private LeaderOutfitType outfitStyle;
    private FaceShape faceShape;

    // Softened Genetic Parameters
    private float gene_HeadWidthRatio;
    private float gene_HeadHeightRatio;
    private float gene_EyeSpacing;
    private float gene_EyeScaleY;
    private float gene_NoseWidth;
    private float gene_NoseLength;
    private float gene_ShoulderWidth;

    private float gene_HairVolume;
    private float gene_BrowThickness;

    // --- RENDER GAME OBJECTS (UI) ---
    private GameObject neck, shoulders, shirt, tie, lapelL, lapelR, suitPocket, pocketSquare;
    private GameObject head, leftEar, rightEar;
    private GameObject chinShadow, cheekL, cheekR;

    // Eyes & Face Details
    private GameObject leftEye, rightEye, pupilL, pupilR, irisRingL, irisRingR;
    private GameObject highlightL1, highlightR1, highlightL2, highlightR2;
    private GameObject upperLidL, upperLidR, eyeBagL, eyeBagR;
    private GameObject leftBrow, rightBrow, foreheadWrinkle;
    private GameObject noseBridge, noseTip, noseNostrilL, noseNostrilR;
    private GameObject upperLip, lowerLip, mouthCornerL, mouthCornerR;

    // Shirt Collar
    private GameObject collarL, collarR;

    // Multi-Clump Hair Objects
    private List<GameObject> hairClumps = new List<GameObject>();
    private List<Vector2> hairPositions = new List<Vector2>();
    private List<Vector2> hairScales = new List<Vector2>();
    private GameObject beardJaw, beardMustache;

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
        hairClumps.Clear();
        hairPositions.Clear();
        hairScales.Clear();

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
        hairClumps.Clear();
        hairPositions.Clear();
        hairScales.Clear();

        RollGenetics();
        BuildHierarchy();
        UpdateCharacter(age);
    }

    private void RollGenetics()
    {
        // Warm Skin Tones
        float skinToneRoll = Random.value;
        if (skinToneRoll < 0.30f) skinColor = new Color(0.97f, 0.83f, 0.73f);
        else if (skinToneRoll < 0.60f) skinColor = new Color(0.82f, 0.63f, 0.48f);
        else if (skinToneRoll < 0.85f) skinColor = new Color(0.55f, 0.38f, 0.26f);
        else skinColor = new Color(0.32f, 0.21f, 0.15f);

        // Natural Hair Colors
        float hairColorRoll = Random.value;
        if (hairColorRoll < 0.35f) naturalHairColor = new Color(0.12f, 0.10f, 0.09f);
        else if (hairColorRoll < 0.65f) naturalHairColor = new Color(0.38f, 0.24f, 0.14f);
        else if (hairColorRoll < 0.85f) naturalHairColor = new Color(0.72f, 0.55f, 0.28f);
        else naturalHairColor = new Color(0.58f, 0.25f, 0.12f);

        eyeColor = Random.ColorHSV(0.05f, 0.65f, 0.4f, 0.7f, 0.4f, 0.8f);

        outfitStyle = (LeaderOutfitType)Random.Range(0, System.Enum.GetValues(typeof(LeaderOutfitType)).Length);
        SetupLeaderOutfitColors();

        hairStyle = (HairType)Random.Range(0, System.Enum.GetValues(typeof(HairType)).Length);
        beardStyle = (BeardType)Random.Range(0, System.Enum.GetValues(typeof(BeardType)).Length);
        faceShape = (FaceShape)Random.Range(0, System.Enum.GetValues(typeof(FaceShape)).Length);

        // Friendly/Proportional Genetics
        gene_HeadWidthRatio = Random.Range(0.88f, 1.12f);
        gene_HeadHeightRatio = Random.Range(0.90f, 1.10f);
        gene_EyeSpacing = Random.Range(-8f, 8f);
        gene_EyeScaleY = Random.Range(0.80f, 1.10f);

        gene_NoseWidth = Random.Range(0.80f, 1.20f);
        gene_NoseLength = Random.Range(0.80f, 1.15f);
        gene_ShoulderWidth = Random.Range(0.95f, 1.20f);

        gene_HairVolume = Random.Range(0.85f, 1.25f);
        gene_BrowThickness = Random.Range(4f, 7f);
    }

    // --- CHANGED: Tailored Suit Color Palettes ---
    private void SetupLeaderOutfitColors()
    {
        switch (outfitStyle)
        {
            case LeaderOutfitType.ClassicBlackTie:
                suitColor = new Color(0.10f, 0.10f, 0.12f);       // Obsidian Black
                shirtColor = new Color(0.96f, 0.96f, 0.98f);      // Crisp White
                tieColor = new Color(0.65f, 0.08f, 0.12f);        // Deep Red / Crimson
                lapelColor = new Color(0.05f, 0.05f, 0.06f);      // Satin Black Accent
                break;
            case LeaderOutfitType.ExecutiveNavy:
                suitColor = new Color(0.12f, 0.18f, 0.30f);       // Midnight Navy
                shirtColor = new Color(0.92f, 0.94f, 0.98f);      // Light Ice Blue
                tieColor = new Color(0.80f, 0.60f, 0.15f);        // Gold/Amber
                lapelColor = new Color(0.09f, 0.14f, 0.24f);
                break;
            case LeaderOutfitType.ModernCharcoal:
                suitColor = new Color(0.22f, 0.24f, 0.26f);       // Slate Charcoal
                shirtColor = new Color(0.98f, 0.98f, 0.98f);      // Pure White
                tieColor = new Color(0.15f, 0.32f, 0.38f);        // Teal/Royal Steel
                lapelColor = new Color(0.16f, 0.18f, 0.20f);
                break;
            case LeaderOutfitType.RegalBurgundy:
                suitColor = new Color(0.28f, 0.10f, 0.14f);       // Wine Burgundy
                shirtColor = new Color(0.95f, 0.92f, 0.88f);      // Cream
                tieColor = new Color(0.12f, 0.12f, 0.15f);        // Dark Onyx
                lapelColor = new Color(0.20f, 0.07f, 0.10f);
                break;
        }
    }

    private void BuildHierarchy()
    {
        int order = 0;

        // --- Body & Leader Suit Layer ---
        shoulders = CreatePart("Shoulders", transform, circleSprite, suitColor, order++);
        shirt = CreatePart("DressShirt", shoulders.transform, circleSprite, shirtColor, order++);
        tie = CreatePart("NeckTie", shoulders.transform, circleSprite, tieColor, order++);
        lapelL = CreatePart("LapelL", shoulders.transform, circleSprite, lapelColor, order++);
        lapelR = CreatePart("LapelR", shoulders.transform, circleSprite, lapelColor, order++);
        suitPocket = CreatePart("SuitPocket", shoulders.transform, circleSprite, lapelColor, order++);
        pocketSquare = CreatePart("PocketSquare", suitPocket.transform, circleSprite, shirtColor, order++);

        neck = CreatePart("Neck", transform, circleSprite, skinColor * 0.88f, order++);
        collarL = CreatePart("ShirtCollarL", neck.transform, circleSprite, shirtColor, order++);
        collarR = CreatePart("ShirtCollarR", neck.transform, circleSprite, shirtColor, order++);

        // --- Head Base Layer ---
        leftEar = CreatePart("LeftEar", transform, circleSprite, skinColor * 0.95f, order++);
        rightEar = CreatePart("RightEar", transform, circleSprite, skinColor * 0.95f, order++);
        head = CreatePart("Head", transform, circleSprite, skinColor, order++);
        chinShadow = CreatePart("ChinShadow", head.transform, circleSprite, skinColor * 0.90f, order++);
        cheekL = CreatePart("CheekL", head.transform, circleSprite, skinColor * 1.04f, order++);
        cheekR = CreatePart("CheekR", head.transform, circleSprite, skinColor * 1.04f, order++);

        // --- Eyes ---
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

        // --- Nose ---
        noseBridge = CreatePart("NoseBridge", head.transform, circleSprite, skinColor * 0.94f, order++);
        noseTip = CreatePart("NoseTip", head.transform, circleSprite, skinColor * 0.88f, order++);
        noseNostrilL = CreatePart("NostrilL", noseTip.transform, circleSprite, skinColor * 0.72f, order++);
        noseNostrilR = CreatePart("NostrilR", noseTip.transform, circleSprite, skinColor * 0.72f, order++);

        // --- Mouth ---
        upperLip = CreatePart("UpperLip", head.transform, circleSprite, new Color(0.68f, 0.38f, 0.38f), order++);
        lowerLip = CreatePart("LowerLip", head.transform, circleSprite, new Color(0.75f, 0.45f, 0.45f), order++);
        mouthCornerL = CreatePart("MouthCornerL", head.transform, circleSprite, new Color(0.50f, 0.25f, 0.25f), order++);
        mouthCornerR = CreatePart("MouthCornerR", head.transform, circleSprite, new Color(0.50f, 0.25f, 0.25f), order++);

        // --- Facial Hair & Eyebrows ---
        beardJaw = CreatePart("BeardJaw", head.transform, circleSprite, naturalHairColor, order++);
        beardMustache = CreatePart("BeardMustache", head.transform, circleSprite, naturalHairColor, order++);

        leftBrow = CreatePart("LeftBrow", head.transform, circleSprite, naturalHairColor, order++);
        rightBrow = CreatePart("RightBrow", head.transform, circleSprite, naturalHairColor, order++);
        foreheadWrinkle = CreatePart("ForeheadWrinkle", head.transform, circleSprite, skinColor * 0.82f, order++);

        BuildImprovedHairClumps(order);
    }

    private void UpdateCharacter(float t)
    {
        Color currentHairColor = (t > 0.60f)
            ? Color.Lerp(naturalHairColor, new Color(0.85f, 0.85f, 0.85f), (t - 0.60f) * 2.5f)
            : naturalHairColor;

        Color currentSkinColor = Color.Lerp(skinColor, skinColor * 0.92f, t);
        head.GetComponent<Image>().color = currentSkinColor;

        // --- 1. EXECUTIVE SUIT & FORMAL ATTIRE ---
        float shoulderW = Mathf.Lerp(140f, 380f * gene_ShoulderWidth, Mathf.Clamp01(t * 1.5f));
        ApplyUITransform(shoulders, new Vector2(0f, Mathf.Lerp(-130f, -190f, t)), new Vector2(shoulderW, Mathf.Lerp(90f, 200f, t)), suitColor);

        // V-Neck Opening for Dress Shirt
        ApplyUITransform(shirt, new Vector2(0f, 30f), new Vector2(shoulderW * 0.38f, 110f), shirtColor);

        // Silk Tie
        ApplyUITransform(tie, new Vector2(0f, 10f), new Vector2(18f, 105f), tieColor);

        // Tailored Suit Lapels
        float lapelW = 32f;
        float lapelH = 115f;
        ApplyUITransform(lapelL, new Vector2(-shoulderW * 0.20f, 15f), new Vector2(lapelW, lapelH), lapelColor);
        ApplyUITransform(lapelR, new Vector2(shoulderW * 0.20f, 15f), new Vector2(lapelW, lapelH), lapelColor);
        lapelL.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -18f);
        lapelR.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 18f);

        // Breast Pocket & Folded Pocket Square
        ApplyUITransform(suitPocket, new Vector2(shoulderW * 0.30f, -15f), new Vector2(40f, 6f), lapelColor);
        ApplyUITransform(pocketSquare, new Vector2(0f, 6f), new Vector2(28f, 10f), shirtColor);

        // Pressed Dress Shirt Collar
        float neckW = Mathf.Lerp(55f, 105f * gene_HeadWidthRatio, t);
        float neckH = Mathf.Lerp(60f, 115f, t);
        ApplyUITransform(neck, new Vector2(0f, Mathf.Lerp(-70f, -100f, t)), new Vector2(neckW, neckH), currentSkinColor * 0.88f);

        ApplyUITransform(collarL, new Vector2(-neckW * 0.28f, -10f), new Vector2(30f, 38f), shirtColor);
        ApplyUITransform(collarR, new Vector2(neckW * 0.28f, -10f), new Vector2(30f, 38f), shirtColor);
        collarL.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -35f);
        collarR.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 35f);

        // --- 2. SOFT, FRIENDLY FACE SHAPES ---
        float baseHeadW = Mathf.Lerp(200f, 180f, t) * gene_HeadWidthRatio;
        float baseHeadH = Mathf.Lerp(195f, 250f, t) * gene_HeadHeightRatio;

        Vector2 shapeScale = Vector2.one;
        switch (faceShape)
        {
            case FaceShape.SoftSquare: shapeScale = new Vector2(1.08f, 0.98f); break;
            case FaceShape.Round: shapeScale = new Vector2(1.06f, 1.02f); break;
            case FaceShape.Heart: shapeScale = new Vector2(1.05f, 0.95f); break;
            case FaceShape.SoftOval: shapeScale = new Vector2(1.0f, 1.0f); break;
        }

        float headW = baseHeadW * shapeScale.x;
        float headH = baseHeadH * shapeScale.y;
        ApplyUITransform(head, Vector2.zero, new Vector2(headW, headH), currentSkinColor);

        // Soft Chin Contour
        ApplyUITransform(chinShadow, new Vector2(0f, Mathf.Lerp(-42f, -35f, t) * shapeScale.y), new Vector2(headW * 0.42f, Mathf.Lerp(20f, 32f, t)), currentSkinColor * 0.90f);

        // Gentle Cheek Blush/Structure
        Color cheekColor = currentSkinColor;
        cheekColor.r = Mathf.Min(1f, cheekColor.r * 1.04f);
        ApplyUITransform(cheekL, new Vector2(-headW * 0.30f, -8f), new Vector2(45f, 35f), cheekColor);
        ApplyUITransform(cheekR, new Vector2(headW * 0.30f, -8f), new Vector2(45f, 35f), cheekColor);

        // Ears
        float earY = Mathf.Lerp(-10f, 5f, t);
        float earH = Mathf.Lerp(45f, 70f, t);
        ApplyUITransform(leftEar, new Vector2(-headW * 0.48f, earY), new Vector2(36f, earH), currentSkinColor * 0.95f);
        ApplyUITransform(rightEar, new Vector2(headW * 0.48f, earY), new Vector2(36f, earH), currentSkinColor * 0.95f);

        // --- 3. EXPRESSIVE EYES ---
        float eyeY = Mathf.Lerp(-8f, 20f, t);
        float eyeSpacing = Mathf.Lerp(50f, 40f, t) + gene_EyeSpacing;
        float eyeSize = Mathf.Lerp(50f, 36f, t);

        PositionEyeUI(leftEye, irisRingL, pupilL, highlightL1, highlightL2, upperLidL, eyeBagL, -eyeSpacing, eyeY, eyeSize, t, currentSkinColor);
        PositionEyeUI(rightEye, irisRingR, pupilR, highlightR1, highlightR2, upperLidR, eyeBagR, eyeSpacing, eyeY, eyeSize, t, currentSkinColor);

        Color wColor = currentSkinColor * 0.82f;
        wColor.a = Mathf.Clamp01((t - 0.55f) * 2f);
        ApplyUITransform(foreheadWrinkle, new Vector2(0f, eyeY + 36f), new Vector2(55f, 3f), wColor);

        // --- 4. WARM EXPRESSIONS ---
        ApplyFacialExpression(eyeY, eyeSpacing, t, currentHairColor);

        // --- 5. NATURAL NOSE ---
        float noseY = Mathf.Lerp(-28f, -4f, t);
        float bridgeHeight = Mathf.Lerp(12f, 38f * gene_NoseLength, t);
        ApplyUITransform(noseBridge, new Vector2(0f, noseY + (bridgeHeight * 0.45f)), new Vector2(8f * gene_NoseWidth, bridgeHeight), currentSkinColor * 0.94f);

        float noseWidth = Mathf.Lerp(22f, 34f, t) * gene_NoseWidth;
        ApplyUITransform(noseTip, new Vector2(0f, noseY - (bridgeHeight * 0.1f)), new Vector2(noseWidth, Mathf.Lerp(16f, 24f, t)), currentSkinColor * 0.88f);
        ApplyUITransform(noseNostrilL, new Vector2(-noseWidth * 0.35f, -1f), new Vector2(5f, 4f), currentSkinColor * 0.72f);
        ApplyUITransform(noseNostrilR, new Vector2(noseWidth * 0.35f, -1f), new Vector2(5f, 4f), currentSkinColor * 0.72f);

        // Beard
        UpdateBeardGrowth(t, currentHairColor, Mathf.Lerp(-50f, -40f, t));

        // Softened Hair
        UpdateMultiClumpHair(t, currentHairColor, headW);
    }

    private void PositionEyeUI(GameObject eye, GameObject iris, GameObject pupil, GameObject h1, GameObject h2, GameObject upperLid, GameObject eyeBag, float x, float y, float size, float ageVal, Color skinCol)
    {
        ApplyUITransform(eye, new Vector2(x, y), new Vector2(size, size * 0.76f * gene_EyeScaleY), Color.white);

        float irisSize = size * 0.58f;
        ApplyUITransform(iris, Vector2.zero, new Vector2(irisSize, irisSize), eyeColor);
        ApplyUITransform(pupil, Vector2.zero, new Vector2(irisSize * 0.50f, irisSize * 0.50f), new Color(0.12f, 0.12f, 0.12f));

        // Vibrant Catchlights
        ApplyUITransform(h1, new Vector2(size * 0.14f, size * 0.14f), new Vector2(size * 0.22f, size * 0.22f), Color.white);
        ApplyUITransform(h2, new Vector2(-size * 0.12f, -size * 0.12f), new Vector2(size * 0.12f, size * 0.12f), new Color(1f, 1f, 1f, 0.7f));

        ApplyUITransform(upperLid, new Vector2(0f, size * 0.32f), new Vector2(size * 1.05f, size * 0.22f), skinCol * 0.85f);

        Color bagColor = skinCol * 0.82f;
        bagColor.a = Mathf.Clamp01((ageVal - 0.5f) * 2f);
        ApplyUITransform(eyeBag, new Vector2(0f, -size * 0.45f), new Vector2(size * 1.0f, size * 0.2f), bagColor);
    }

    private void ApplyFacialExpression(float eyeY, float eyeSpacing, float t, Color hairColor)
    {
        float browY = eyeY + Mathf.Lerp(18f, 14f, t);
        float mouthY = Mathf.Lerp(-50f, -40f, t);

        float browAngle = 0f;
        float browOffset = 0f;

        Color upperLipCol = new Color(0.68f, 0.38f, 0.38f);
        Color lowerLipCol = new Color(0.75f, 0.45f, 0.45f);

        float lipWidth = Mathf.Lerp(36f, 52f, t);

        switch (expression)
        {
            case Expression.FriendlyNeutral:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 2f), new Vector2(lipWidth, 6f), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.88f, 8f), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.5f, mouthY + 2f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.5f, mouthY + 2f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                break;

            case Expression.Happy:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 3f), new Vector2(lipWidth * 1.1f, 7f), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 4f), new Vector2(lipWidth * 1.02f, 11f), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.55f, mouthY + 6f), new Vector2(5f, 5f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.55f, mouthY + 6f), new Vector2(5f, 5f), upperLipCol * 0.8f);
                browAngle = -6f;
                browOffset = 2f;
                break;

            case Expression.Gentle:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth * 0.95f, 5f), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 3f), new Vector2(lipWidth * 0.85f, 8f), lowerLipCol);
                ApplyUITransform(mouthCornerL, new Vector2(-lipWidth * 0.48f, mouthY + 1f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                ApplyUITransform(mouthCornerR, new Vector2(lipWidth * 0.48f, mouthY + 1f), new Vector2(4f, 4f), upperLipCol * 0.8f);
                browAngle = 4f;
                break;

            case Expression.Focused:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 1f), new Vector2(lipWidth, 5f), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 2f), new Vector2(lipWidth * 0.85f, 6f), lowerLipCol);
                browAngle = -10f;
                browOffset = -2f;
                break;

            case Expression.Surprised:
                ApplyUITransform(upperLip, new Vector2(0f, mouthY + 4f), new Vector2(26f, 8f), upperLipCol);
                ApplyUITransform(lowerLip, new Vector2(0f, mouthY - 4f), new Vector2(22f, 12f), lowerLipCol);
                browOffset = 8f;
                break;
        }

        Vector2 browSize = new Vector2(Mathf.Lerp(30f, 44f, t), gene_BrowThickness);
        ApplyUITransform(leftBrow, new Vector2(-eyeSpacing, browY + browOffset), browSize, hairColor);
        ApplyUITransform(rightBrow, new Vector2(eyeSpacing, browY + browOffset), browSize, hairColor);

        leftBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, browAngle);
        rightBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -browAngle);
    }

    private void BuildImprovedHairClumps(int baseOrder)
    {
        int clumpCount = 0;
        switch (hairStyle)
        {
            case HairType.ShortCrop: clumpCount = 6; break;
            case HairType.MessyBangs: clumpCount = 8; break;
            case HairType.SoftWaves: clumpCount = 10; break;
            case HairType.BuzzCut: clumpCount = 5; break;
            case HairType.SidePart: clumpCount = 7; break;
        }

        for (int i = 0; i < clumpCount; i++)
        {
            GameObject clump = CreatePart("HairClump_" + i, head.transform, circleSprite, naturalHairColor, baseOrder + i);
            hairClumps.Add(clump);

            hairPositions.Add(new Vector2(Random.Range(-8f, 8f), Random.Range(-6f, 6f)));
            hairScales.Add(new Vector2(Random.Range(0.9f, 1.15f), Random.Range(0.9f, 1.15f)));
        }
    }

    private void UpdateMultiClumpHair(float t, Color c, float headW)
    {
        float hairScaleProgress = Mathf.Clamp01(t * 3f);

        for (int i = 0; i < hairClumps.Count; i++)
        {
            GameObject clump = hairClumps[i];
            clump.GetComponent<Image>().color = c;

            if (hairStyle == HairType.Bald)
            {
                clump.SetActive(false);
                continue;
            }

            clump.SetActive(true);
            RectTransform rect = clump.GetComponent<RectTransform>();
            Vector2 seedPosOffset = hairPositions[i];
            Vector2 seedScaleOffset = hairScales[i];

            switch (hairStyle)
            {
                case HairType.BuzzCut:
                    float buzzAngle = Mathf.Lerp(-65f, 65f, (float)i / (hairClumps.Count - 1));
                    Vector3 buzzPos = Quaternion.Euler(0, 0, buzzAngle) * Vector3.up * (headW * 0.48f);
                    rect.anchoredPosition = (Vector2)buzzPos + new Vector2(0f, 30f);
                    rect.sizeDelta = Vector2.one * (32f * hairScaleProgress * gene_HairVolume);
                    break;

                case HairType.ShortCrop:
                    float cropAngle = Mathf.Lerp(-60f, 60f, (float)i / (hairClumps.Count - 1));
                    Vector3 cropPos = Quaternion.Euler(0, 0, cropAngle) * Vector3.up * (headW * 0.48f);
                    rect.anchoredPosition = (Vector2)cropPos + new Vector2(0f, 38f) + seedPosOffset;
                    rect.sizeDelta = Vector2.one * (44f * hairScaleProgress * gene_HairVolume * seedScaleOffset.x);
                    break;

                case HairType.MessyBangs:
                    float bangX = Mathf.Lerp(-headW * 0.42f, headW * 0.42f, (float)i / (hairClumps.Count - 1));
                    float bangY = 95f - (Mathf.Abs(bangX) * 0.15f);
                    rect.anchoredPosition = new Vector2(bangX, bangY) + seedPosOffset;
                    rect.sizeDelta = new Vector2(38f * seedScaleOffset.x, 44f * hairScaleProgress * gene_HairVolume * seedScaleOffset.y);
                    break;

                case HairType.SoftWaves:
                    float waveX = Mathf.Lerp(-headW * 0.45f, headW * 0.45f, (float)i / (hairClumps.Count - 1));
                    rect.anchoredPosition = new Vector2(waveX, 90f + (Mathf.Sin(i) * 8f)) + seedPosOffset;
                    rect.sizeDelta = new Vector2(46f * seedScaleOffset.x, 52f * hairScaleProgress * gene_HairVolume);
                    break;

                case HairType.SidePart:
                    float partX = Mathf.Lerp(-headW * 0.40f, headW * 0.50f, (float)i / (hairClumps.Count - 1));
                    rect.anchoredPosition = new Vector2(partX, 100f) + seedPosOffset;
                    rect.sizeDelta = new Vector2(45f * seedScaleOffset.x, 38f * hairScaleProgress * gene_HairVolume);
                    break;
            }
        }
    }

    private void UpdateBeardGrowth(float t, Color c, float mouthY)
    {
        float beardGrowth = Mathf.Clamp01((t - 0.50f) * 2.5f);

        if (beardStyle == BeardType.None || beardGrowth <= 0f)
        {
            beardJaw.SetActive(false);
            beardMustache.SetActive(false);
            return;
        }

        beardJaw.SetActive(true);
        beardMustache.SetActive(beardStyle == BeardType.NeatGoatee);

        if (beardStyle == BeardType.LightStubble)
        {
            Color stubbleC = c;
            stubbleC.a = 0.35f * beardGrowth;
            ApplyUITransform(beardJaw, new Vector2(0f, -36f), new Vector2(80f, 35f), stubbleC);
            beardMustache.SetActive(false);
        }
        else if (beardStyle == BeardType.NeatGoatee)
        {
            ApplyUITransform(beardJaw, new Vector2(0f, -44f), new Vector2(48f, 28f * beardGrowth), c);
            ApplyUITransform(beardMustache, new Vector2(0f, mouthY + 6f), new Vector2(38f, 8f * beardGrowth), c);
        }
    }

    /// <summary>
    /// Triggers the death state and turns all avatar UI elements grayscale.
    /// </summary>
    [ContextMenu("Die")]
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Ensure we have a valid grayscale material
        if (grayscaleMaterial == null)
        {
            grayscaleMaterial = CreateDefaultGrayscaleMaterial();
        }

        // Cache original materials and apply grayscale material to every Image component
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

    /// <summary>
    /// Resets the avatar back to its original colored state.
    /// </summary>
    public void Revive()
    {
        if (!isDead) return;
        isDead = false;

        // Restore original materials
        foreach (var pair in originalMaterials)
        {
            if (pair.Key != null)
            {
                pair.Key.material = pair.Value;
            }
        }
        originalMaterials.Clear();
    }

    /// <summary>
    /// Helper method to dynamically generate a Grayscale Material using Unity's UI shader if none is provided.
    /// </summary>
    private Material CreateDefaultGrayscaleMaterial()
    {
        // Uses a built-in Unity UI grayscale-compatible shader or custom shader string
        Shader shader = Shader.Find("UI/Default");
        Material mat = new Material(shader);

        // If using a custom shader with a grayscale toggle/property:
        // mat.SetFloat("_GrayscaleAmount", 1.0f);

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

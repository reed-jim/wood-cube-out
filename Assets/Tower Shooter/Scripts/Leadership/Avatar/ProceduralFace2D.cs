using UnityEngine;
using System.Collections.Generic;

public class ProceduralHumanAvatarV2 : MonoBehaviour
{
    [Header("Growth & Expression")]
    [Range(0f, 1f)]
    [Tooltip("0.0 = Baby, 0.3 = Child, 0.6 = Adult, 1.0 = Elder")]
    public float age = 0.0f;

    public enum Expression { Neutral, Happy, Sad, Angry, Surprised }
    public Expression expression = Expression.Neutral;

    [Header("Manual Seed (Optional)")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    public enum HairType { Bald, ShortCrop, MessyBangs, AfroPuff, MediumSlick }
    public enum BeardType { None, StubbleOnly, FullBeard, Goatee }

    // --- GENETIC DATA ---
    private Color skinColor;
    private Color eyeColor;
    private Color naturalHairColor;
    private Color shirtColor;
    private HairType hairStyle;
    private BeardType beardStyle;

    private float gene_HeadRatio;
    private float gene_EyeSpacing;
    private float gene_NoseSize;
    private float gene_ShoulderWidth;

    // --- RENDER GAME OBJECTS ---
    private GameObject neck, shoulders, shirtCollar;
    private GameObject head, leftEar, rightEar, chinShadow;
    private GameObject leftEye, rightEye, pupilL, pupilR, highlightL, highlightR, eyeBagL, eyeBagR;
    private GameObject leftBrow, rightBrow, foreheadWrinkle;
    private GameObject noseBridge, noseTip;
    private GameObject mouthBase, mouthSmile, mouthFrown;

    // Multi-Clump Hair Container Objects
    private List<GameObject> hairClumps = new List<GameObject>();
    private GameObject beardJaw, beardMustache;

    private Sprite circleSprite;

    private void Start()
    {
        circleSprite = CreateCircleSprite();
        GenerateNewRandomCharacter();
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

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        hairClumps.Clear();

        RollGenetics();
        BuildHierarchy();
        UpdateCharacter(age);
    }

    private void RollGenetics()
    {
        float skinToneRoll = Random.value;
        if (skinToneRoll < 0.35f) skinColor = new Color(0.96f, 0.80f, 0.69f);
        else if (skinToneRoll < 0.70f) skinColor = new Color(0.72f, 0.50f, 0.35f);
        else skinColor = new Color(0.38f, 0.24f, 0.16f);

        eyeColor = Random.ColorHSV(0f, 1f, 0.4f, 0.8f, 0.3f, 0.8f);
        naturalHairColor = Random.ColorHSV(0f, 1f, 0.2f, 0.7f, 0.1f, 0.3f);
        shirtColor = Random.ColorHSV(0f, 1f, 0.5f, 0.8f, 0.4f, 0.8f);

        hairStyle = (HairType)Random.Range(0, System.Enum.GetValues(typeof(HairType)).Length);
        beardStyle = (BeardType)Random.Range(0, System.Enum.GetValues(typeof(BeardType)).Length);

        gene_HeadRatio = Random.Range(0.85f, 1.15f);
        gene_EyeSpacing = Random.Range(-0.05f, 0.05f);
        gene_NoseSize = Random.Range(0.8f, 1.25f);
        gene_ShoulderWidth = Random.Range(0.85f, 1.2f);
    }

    private void BuildHierarchy()
    {
        // Body (0 - 2)
        shoulders = CreatePart("Shoulders", transform, circleSprite, shirtColor, 0);
        shirtCollar = CreatePart("ShirtCollar", transform, circleSprite, shirtColor * 0.85f, 1);
        neck = CreatePart("Neck", transform, circleSprite, skinColor * 0.85f, 2);

        // Head & Ears (3 - 5)
        leftEar = CreatePart("LeftEar", transform, circleSprite, skinColor * 0.95f, 3);
        rightEar = CreatePart("RightEar", transform, circleSprite, skinColor * 0.95f, 3);
        head = CreatePart("Head", transform, circleSprite, skinColor, 4);
        chinShadow = CreatePart("ChinShadow", head.transform, circleSprite, skinColor * 0.85f, 5);

        // Eyes & Aging Details (6 - 8)
        leftEye = CreatePart("LeftEye", head.transform, circleSprite, Color.white, 6);
        rightEye = CreatePart("RightEye", head.transform, circleSprite, Color.white, 6);
        eyeBagL = CreatePart("EyeBagL", leftEye.transform, circleSprite, skinColor * 0.75f, 6);
        eyeBagR = CreatePart("EyeBagR", rightEye.transform, circleSprite, skinColor * 0.75f, 6);
        pupilL = CreatePart("PupilL", leftEye.transform, circleSprite, eyeColor, 7);
        pupilR = CreatePart("PupilR", rightEye.transform, circleSprite, eyeColor, 7);
        highlightL = CreatePart("HighlightL", leftEye.transform, circleSprite, Color.white, 8);
        highlightR = CreatePart("HighlightR", rightEye.transform, circleSprite, Color.white, 8);

        // Nose (7 - 8)
        noseBridge = CreatePart("NoseBridge", head.transform, circleSprite, skinColor * 0.88f, 7);
        noseTip = CreatePart("NoseTip", head.transform, circleSprite, skinColor * 0.8f, 8);

        // Expressions Mouth Components (7)
        mouthBase = CreatePart("MouthBase", head.transform, circleSprite, new Color(0.65f, 0.25f, 0.25f), 7);
        mouthSmile = CreatePart("MouthSmile", head.transform, circleSprite, new Color(0.65f, 0.25f, 0.25f), 7);
        mouthFrown = CreatePart("MouthFrown", head.transform, circleSprite, new Color(0.65f, 0.25f, 0.25f), 7);

        // Beard (8 - 9)
        beardJaw = CreatePart("BeardJaw", head.transform, circleSprite, naturalHairColor, 8);
        beardMustache = CreatePart("BeardMustache", head.transform, circleSprite, naturalHairColor, 9);

        // Brows, Wrinkles & Hair Layering (10 - 12)
        leftBrow = CreatePart("LeftBrow", head.transform, circleSprite, naturalHairColor, 10);
        rightBrow = CreatePart("RightBrow", head.transform, circleSprite, naturalHairColor, 10);
        foreheadWrinkle = CreatePart("ForeheadWrinkle", head.transform, circleSprite, skinColor * 0.75f, 10);

        BuildImprovedHairClumps();
    }

    private void BuildImprovedHairClumps()
    {
        // Construct layered hair using multiple clump sprites instead of 1 blob
        int clumpCount = 0;
        switch (hairStyle)
        {
            case HairType.ShortCrop: clumpCount = 5; break;
            case HairType.MessyBangs: clumpCount = 7; break;
            case HairType.AfroPuff: clumpCount = 10; break;
            case HairType.MediumSlick: clumpCount = 6; break;
        }

        for (int i = 0; i < clumpCount; i++)
        {
            GameObject clump = CreatePart("HairClump_" + i, head.transform, circleSprite, naturalHairColor, 11);
            hairClumps.Add(clump);
        }
    }

    private void UpdateCharacter(float t)
    {
        Color currentHairColor = (t > 0.55f)
            ? Color.Lerp(naturalHairColor, new Color(0.85f, 0.85f, 0.85f), (t - 0.55f) * 2.2f)
            : naturalHairColor;

        Color currentSkinColor = Color.Lerp(skinColor, skinColor * 0.88f, t);
        head.GetComponent<SpriteRenderer>().color = currentSkinColor;

        // --- 1. SHOULDERS & NECK ---
        float shoulderW = Mathf.Lerp(1.2f, 3.8f * gene_ShoulderWidth, Mathf.Clamp01(t * 1.5f));
        if (t > 0.75f) shoulderW *= Mathf.Lerp(1.0f, 0.9f, (t - 0.75f) * 4f);

        shoulders.transform.localPosition = new Vector3(0f, Mathf.Lerp(-1.2f, -1.8f, t), 0f);
        shoulders.transform.localScale = new Vector3(shoulderW, Mathf.Lerp(0.8f, 1.8f, t), 1f);

        float neckW = Mathf.Lerp(0.5f, 1.1f * gene_HeadRatio, t);
        float neckH = Mathf.Lerp(0.6f, 1.2f, t);
        neck.transform.localPosition = new Vector3(0f, Mathf.Lerp(-0.7f, -1.0f, t), 0f);
        neck.transform.localScale = new Vector3(neckW, neckH, 1f);

        shirtCollar.transform.localPosition = neck.transform.localPosition + new Vector3(0f, -neckH * 0.3f, 0f);
        shirtCollar.transform.localScale = new Vector3(neckW * 1.3f, 0.4f, 1f);

        // --- 2. HEAD & FACE SHAPE ---
        float headW = Mathf.Lerp(2.2f, 1.8f * gene_HeadRatio, t);
        float headH = Mathf.Lerp(2.0f, 2.6f, t);
        head.transform.localScale = new Vector3(headW, headH, 1f);

        chinShadow.transform.localPosition = new Vector3(0f, Mathf.Lerp(-0.4f, -0.35f, t), 0f);
        chinShadow.transform.localScale = new Vector3(Mathf.Lerp(0.6f, 0.85f, t), Mathf.Lerp(0.2f, 0.35f, t), 1f);

        float earY = Mathf.Lerp(-0.1f, 0.05f, t);
        leftEar.transform.localPosition = new Vector3(-headW * 0.48f, earY, 0f);
        rightEar.transform.localPosition = new Vector3(headW * 0.48f, earY, 0f);
        leftEar.transform.localScale = new Vector3(0.4f, Mathf.Lerp(0.45f, 0.75f, t), 1f);
        rightEar.transform.localScale = new Vector3(0.4f, Mathf.Lerp(0.45f, 0.75f, t), 1f);

        // --- 3. EYES & AGE DETAILS ---
        float eyeY = Mathf.Lerp(-0.1f, 0.22f, t);
        float eyeSpacing = Mathf.Lerp(0.52f, 0.42f, t) + gene_EyeSpacing;
        float eyeScale = Mathf.Lerp(0.55f, 0.32f, t);

        PositionEye(leftEye, pupilL, highlightL, eyeBagL, -eyeSpacing, eyeY, eyeScale, t);
        PositionEye(rightEye, pupilR, highlightR, eyeBagR, eyeSpacing, eyeY, eyeScale, t);

        Color wColor = currentSkinColor * 0.75f;
        wColor.a = Mathf.Clamp01((t - 0.5f) * 2f);
        foreheadWrinkle.GetComponent<SpriteRenderer>().color = wColor;
        foreheadWrinkle.transform.localPosition = new Vector3(0f, eyeY + 0.35f, 0f);
        foreheadWrinkle.transform.localScale = new Vector3(0.6f, 0.04f, 1f);

        // --- 4. EXPRESSIONS (Brows & Mouth) ---
        ApplyFacialExpression(eyeY, eyeSpacing, t, currentHairColor);

        // --- 5. NOSE & BEARD ---
        float noseY = Mathf.Lerp(-0.3f, -0.05f, t);
        float bridgeHeight = Mathf.Lerp(0.05f, 0.4f * gene_NoseSize, t);
        noseBridge.transform.localPosition = new Vector3(0f, noseY + (bridgeHeight * 0.4f), 0f);
        noseBridge.transform.localScale = new Vector3(0.08f, bridgeHeight, 1f);

        noseTip.transform.localPosition = new Vector3(0f, noseY - (bridgeHeight * 0.1f), 0f);
        noseTip.transform.localScale = new Vector3(Mathf.Lerp(0.18f, 0.35f, t), Mathf.Lerp(0.14f, 0.28f, t), 1f);

        UpdateBeardGrowth(t, currentHairColor, Mathf.Lerp(-0.52f, -0.42f, t));

        // --- 6. LAYERED STYLED HAIR ---
        UpdateMultiClumpHair(t, currentHairColor);
    }

    private void ApplyFacialExpression(float eyeY, float eyeSpacing, float t, Color hairColor)
    {
        float browY = eyeY + Mathf.Lerp(0.18f, 0.12f, t);
        float mouthY = Mathf.Lerp(-0.52f, -0.42f, t);

        leftBrow.GetComponent<SpriteRenderer>().color = hairColor;
        rightBrow.GetComponent<SpriteRenderer>().color = hairColor;

        float browAngle = 0f;
        float browOffset = 0f;

        mouthBase.SetActive(false);
        mouthSmile.SetActive(false);
        mouthFrown.SetActive(false);

        switch (expression)
        {
            case Expression.Neutral:
                mouthBase.SetActive(true);
                mouthBase.transform.localPosition = new Vector3(0f, mouthY, 0f);
                mouthBase.transform.localScale = new Vector3(Mathf.Lerp(0.32f, 0.58f, t), Mathf.Lerp(0.12f, 0.04f, t), 1f);
                break;

            case Expression.Happy:
                mouthSmile.SetActive(true); // Curved upward smile shape
                mouthSmile.transform.localPosition = new Vector3(0f, mouthY - 0.02f, 0f);
                mouthSmile.transform.localScale = new Vector3(Mathf.Lerp(0.38f, 0.62f, t), Mathf.Lerp(0.2f, 0.18f, t), 1f);
                browAngle = -12f; // Eyebrows tilt outwards
                browOffset = 0.03f;
                break;

            case Expression.Sad:
                mouthFrown.SetActive(true);
                mouthFrown.transform.localPosition = new Vector3(0f, mouthY, 0f);
                mouthFrown.transform.localScale = new Vector3(Mathf.Lerp(0.3f, 0.5f, t), Mathf.Lerp(0.15f, 0.08f, t), 1f);
                browAngle = 15f; // Inner brows tilt up
                break;

            case Expression.Angry:
                mouthBase.SetActive(true);
                mouthBase.transform.localPosition = new Vector3(0f, mouthY, 0f);
                mouthBase.transform.localScale = new Vector3(Mathf.Lerp(0.35f, 0.55f, t), 0.04f, 1f);
                browAngle = -22f; // V-shaped angry eyebrows
                browOffset = -0.05f;
                break;

            case Expression.Surprised:
                mouthSmile.SetActive(true); // Open 'O' mouth
                mouthSmile.transform.localPosition = new Vector3(0f, mouthY - 0.05f, 0f);
                mouthSmile.transform.localScale = new Vector3(0.3f, 0.35f, 1f);
                browOffset = 0.1f; // High eyebrows
                break;
        }

        // Apply Brows Transform
        leftBrow.transform.localPosition = new Vector3(-eyeSpacing, browY + browOffset, 0f);
        rightBrow.transform.localPosition = new Vector3(eyeSpacing, browY + browOffset, 0f);
        leftBrow.transform.localRotation = Quaternion.Euler(0, 0, browAngle);
        rightBrow.transform.localRotation = Quaternion.Euler(0, 0, -browAngle);

        leftBrow.transform.localScale = new Vector3(Mathf.Lerp(0.3f, 0.45f, t), Mathf.Lerp(0.04f, 0.08f, t), 1f);
        rightBrow.transform.localScale = new Vector3(Mathf.Lerp(0.3f, 0.45f, t), Mathf.Lerp(0.04f, 0.08f, t), 1f);
    }

    private void UpdateMultiClumpHair(float t, Color c)
    {
        float hairScaleProgress = Mathf.Clamp01(t * 3f); // Full size by child age

        for (int i = 0; i < hairClumps.Count; i++)
        {
            GameObject clump = hairClumps[i];
            SpriteRenderer sr = clump.GetComponent<SpriteRenderer>();
            sr.color = c;
            sr.sortingOrder = 12; // Render above brows and forehead wrinkles

            if (hairStyle == HairType.Bald)
            {
                clump.SetActive(false);
                continue;
            }

            clump.SetActive(true);

            switch (hairStyle)
            {
                case HairType.ShortCrop:
                    // Arced around the very top edge of the head
                    float angle = Mathf.Lerp(-65f, 65f, (float)i / (hairClumps.Count - 1));
                    Vector3 pos = Quaternion.Euler(0, 0, angle) * Vector3.up * 0.52f;
                    clump.transform.localPosition = pos + new Vector3(0f, 0.45f, 0f);
                    clump.transform.localScale = Vector3.one * (0.45f * hairScaleProgress);
                    break;

                case HairType.MessyBangs:
                    // Sits along the upper forehead hairline instead of covering the eyes
                    float bangX = Mathf.Lerp(-0.45f, 0.45f, (float)i / (hairClumps.Count - 1));
                    float bangY = 0.55f - (Mathf.Abs(bangX) * 0.15f);
                    clump.transform.localPosition = new Vector3(bangX, bangY, 0f);
                    clump.transform.localScale = new Vector3(0.38f, 0.42f * hairScaleProgress, 1f);
                    break;

                case HairType.AfroPuff:
                    // Lifted afro ring crown
                    float afroAngle = (i / (float)hairClumps.Count) * 360f;
                    Vector3 afroPos = Quaternion.Euler(0, 0, afroAngle) * Vector3.up * 0.42f;
                    clump.transform.localPosition = afroPos + new Vector3(0f, 0.55f, 0f);
                    clump.transform.localScale = Vector3.one * (0.6f * hairScaleProgress);
                    break;

                case HairType.MediumSlick:
                    // Shifted upwards to sit comfortably on top of the head
                    float slickX = Mathf.Lerp(-0.38f, 0.38f, (float)i / (hairClumps.Count - 1));
                    clump.transform.localPosition = new Vector3(slickX, 0.62f, 0f);
                    clump.transform.localScale = new Vector3(0.5f, 0.35f * hairScaleProgress, 1f);
                    break;
            }
        }
    }

    private void UpdateBeardGrowth(float t, Color c, float mouthY)
    {
        float beardGrowth = Mathf.Clamp01((t - 0.45f) * 3f);

        if (beardStyle == BeardType.None || beardGrowth <= 0f)
        {
            beardJaw.SetActive(false);
            beardMustache.SetActive(false);
            return;
        }

        beardJaw.SetActive(true);
        beardMustache.SetActive(beardStyle == BeardType.FullBeard || beardStyle == BeardType.Goatee);

        beardJaw.GetComponent<SpriteRenderer>().color = c;
        beardMustache.GetComponent<SpriteRenderer>().color = c;

        if (beardStyle == BeardType.StubbleOnly)
        {
            Color stubbleC = c;
            stubbleC.a = 0.4f * beardGrowth;
            beardJaw.GetComponent<SpriteRenderer>().color = stubbleC;
            beardJaw.transform.localPosition = new Vector3(0f, -0.38f, 0f);
            beardJaw.transform.localScale = new Vector3(0.85f, 0.4f, 1f);
            beardMustache.SetActive(false);
        }
        else if (beardStyle == BeardType.FullBeard)
        {
            beardJaw.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            beardJaw.transform.localScale = new Vector3(0.95f, 0.55f * beardGrowth, 1f);
            beardMustache.transform.localPosition = new Vector3(0f, mouthY + 0.08f, 0f);
            beardMustache.transform.localScale = new Vector3(0.45f, 0.12f * beardGrowth, 1f);
        }
        else if (beardStyle == BeardType.Goatee)
        {
            beardJaw.transform.localPosition = new Vector3(0f, -0.48f, 0f);
            beardJaw.transform.localScale = new Vector3(0.5f, 0.35f * beardGrowth, 1f);
            beardMustache.transform.localPosition = new Vector3(0f, mouthY + 0.08f, 0f);
            beardMustache.transform.localScale = new Vector3(0.4f, 0.1f * beardGrowth, 1f);
        }
    }

    private void PositionEye(GameObject eye, GameObject pupil, GameObject highlight, GameObject eyeBag, float x, float y, float scale, float ageVal)
    {
        eye.transform.localPosition = new Vector3(x, y, 0f);
        eye.transform.localScale = new Vector3(scale, scale * 0.88f, 1f);
        pupil.transform.localScale = Vector3.one * 0.55f;
        highlight.transform.localPosition = new Vector3(0.2f, 0.2f, 0f);
        highlight.transform.localScale = Vector3.one * 0.25f;

        Color bagColor = skinColor * 0.78f;
        bagColor.a = Mathf.Clamp01((ageVal - 0.45f) * 2.2f);
        eyeBag.GetComponent<SpriteRenderer>().color = bagColor;
        eyeBag.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        eyeBag.transform.localScale = new Vector3(1.1f, 0.35f, 1f);
    }

    private GameObject CreatePart(string name, Transform parent, Sprite sprite, Color color, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    private Sprite CreateCircleSprite()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = (size / 2f) - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01((radius - dist) / 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class ProceduralHumanAvatarUI : MonoBehaviour
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

    // --- RENDER GAME OBJECTS (UI) ---
    private GameObject neck, shoulders, shirtCollar;
    private GameObject head, leftEar, rightEar, chinShadow;
    private GameObject leftEye, rightEye, pupilL, pupilR, highlightL, highlightR, eyeBagL, eyeBagR;
    private GameObject leftBrow, rightBrow, foreheadWrinkle;
    private GameObject noseBridge, noseTip;
    private GameObject mouthBase, mouthSmile, mouthFrown;

    // Multi-Clump Hair Objects
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

        // Clear existing children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
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
        gene_EyeSpacing = Random.Range(-5f, 5f);
        gene_NoseSize = Random.Range(0.8f, 1.25f);
        gene_ShoulderWidth = Random.Range(0.85f, 1.2f);
    }

    private void BuildHierarchy()
    {
        // Sibling index handles Canvas UI layering (0 = back, higher = front)
        int order = 0;

        shoulders = CreatePart("Shoulders", transform, circleSprite, shirtColor, order++);
        shirtCollar = CreatePart("ShirtCollar", transform, circleSprite, shirtColor * 0.85f, order++);
        neck = CreatePart("Neck", transform, circleSprite, skinColor * 0.85f, order++);

        leftEar = CreatePart("LeftEar", transform, circleSprite, skinColor * 0.95f, order++);
        rightEar = CreatePart("RightEar", transform, circleSprite, skinColor * 0.95f, order++);
        head = CreatePart("Head", transform, circleSprite, skinColor, order++);
        chinShadow = CreatePart("ChinShadow", head.transform, circleSprite, skinColor * 0.85f, order++);

        leftEye = CreatePart("LeftEye", head.transform, circleSprite, Color.white, order++);
        rightEye = CreatePart("RightEye", head.transform, circleSprite, Color.white, order++);
        eyeBagL = CreatePart("EyeBagL", leftEye.transform, circleSprite, skinColor * 0.75f, order++);
        eyeBagR = CreatePart("EyeBagR", rightEye.transform, circleSprite, skinColor * 0.75f, order++);
        pupilL = CreatePart("PupilL", leftEye.transform, circleSprite, eyeColor, order++);
        pupilR = CreatePart("PupilR", rightEye.transform, circleSprite, eyeColor, order++);
        highlightL = CreatePart("HighlightL", leftEye.transform, circleSprite, Color.white, order++);
        highlightR = CreatePart("HighlightR", rightEye.transform, circleSprite, Color.white, order++);

        noseBridge = CreatePart("NoseBridge", head.transform, circleSprite, skinColor * 0.88f, order++);
        noseTip = CreatePart("NoseTip", head.transform, circleSprite, skinColor * 0.8f, order++);

        mouthBase = CreatePart("MouthBase", head.transform, circleSprite, new Color(0.65f, 0.25f, 0.25f), order++);
        mouthSmile = CreatePart("MouthSmile", head.transform, circleSprite, new Color(0.65f, 0.25f, 0.25f), order++);
        mouthFrown = CreatePart("MouthFrown", head.transform, circleSprite, new Color(0.65f, 0.25f, 0.25f), order++);

        beardJaw = CreatePart("BeardJaw", head.transform, circleSprite, naturalHairColor, order++);
        beardMustache = CreatePart("BeardMustache", head.transform, circleSprite, naturalHairColor, order++);

        leftBrow = CreatePart("LeftBrow", head.transform, circleSprite, naturalHairColor, order++);
        rightBrow = CreatePart("RightBrow", head.transform, circleSprite, naturalHairColor, order++);
        foreheadWrinkle = CreatePart("ForeheadWrinkle", head.transform, circleSprite, skinColor * 0.75f, order++);

        BuildImprovedHairClumps(order);
    }

    private void BuildImprovedHairClumps(int baseOrder)
    {
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
            GameObject clump = CreatePart("HairClump_" + i, head.transform, circleSprite, naturalHairColor, baseOrder + i);
            hairClumps.Add(clump);
        }
    }

    private void UpdateCharacter(float t)
    {
        Color currentHairColor = (t > 0.55f) 
            ? Color.Lerp(naturalHairColor, new Color(0.85f, 0.85f, 0.85f), (t - 0.55f) * 2.2f) 
            : naturalHairColor;

        Color currentSkinColor = Color.Lerp(skinColor, skinColor * 0.88f, t);
        head.GetComponent<Image>().color = currentSkinColor;

        // --- 1. SHOULDERS & NECK ---
        float shoulderW = Mathf.Lerp(120f, 380f * gene_ShoulderWidth, Mathf.Clamp01(t * 1.5f));
        if (t > 0.75f) shoulderW *= Mathf.Lerp(1.0f, 0.9f, (t - 0.75f) * 4f);

        ApplyUITransform(shoulders, new Vector2(0f, Mathf.Lerp(-120f, -180f, t)), new Vector2(shoulderW, Mathf.Lerp(80f, 180f, t)), shirtColor);

        float neckW = Mathf.Lerp(50f, 110f * gene_HeadRatio, t);
        float neckH = Mathf.Lerp(60f, 120f, t);
        ApplyUITransform(neck, new Vector2(0f, Mathf.Lerp(-70f, -100f, t)), new Vector2(neckW, neckH), currentSkinColor * 0.85f);
        ApplyUITransform(shirtCollar, new Vector2(0f, Mathf.Lerp(-70f, -100f, t) - (neckH * 0.3f)), new Vector2(neckW * 1.3f, 40f), shirtColor * 0.85f);

        // --- 2. HEAD & EARS ---
        float headW = Mathf.Lerp(220f, 180f * gene_HeadRatio, t);
        float headH = Mathf.Lerp(200f, 260f, t);
        ApplyUITransform(head, Vector2.zero, new Vector2(headW, headH), currentSkinColor);

        ApplyUITransform(chinShadow, new Vector2(0f, Mathf.Lerp(-40f, -35f, t)), new Vector2(headW * Mathf.Lerp(0.3f, 0.45f, t), Mathf.Lerp(20f, 35f, t)), currentSkinColor * 0.85f);

        float earY = Mathf.Lerp(-10f, 5f, t);
        float earH = Mathf.Lerp(45f, 75f, t);
        ApplyUITransform(leftEar, new Vector2(-headW * 0.48f, earY), new Vector2(40f, earH), currentSkinColor * 0.95f);
        ApplyUITransform(rightEar, new Vector2(headW * 0.48f, earY), new Vector2(40f, earH), currentSkinColor * 0.95f);

        // --- 3. EYES & AGE DETAILS ---
        float eyeY = Mathf.Lerp(-10f, 22f, t);
        float eyeSpacing = Mathf.Lerp(52f, 42f, t) + gene_EyeSpacing;
        float eyeSize = Mathf.Lerp(55f, 32f, t);

        PositionEyeUI(leftEye, pupilL, highlightL, eyeBagL, -eyeSpacing, eyeY, eyeSize, t, currentSkinColor);
        PositionEyeUI(rightEye, pupilR, highlightR, eyeBagR, eyeSpacing, eyeY, eyeSize, t, currentSkinColor);

        Color wColor = currentSkinColor * 0.75f;
        wColor.a = Mathf.Clamp01((t - 0.5f) * 2f);
        ApplyUITransform(foreheadWrinkle, new Vector2(0f, eyeY + 35f), new Vector2(60f, 4f), wColor);

        // --- 4. EXPRESSIONS (Brows & Mouth) ---
        ApplyFacialExpression(eyeY, eyeSpacing, t, currentHairColor);

        // --- 5. NOSE & BEARD ---
        float noseY = Mathf.Lerp(-30f, -5f, t);
        float bridgeHeight = Mathf.Lerp(5f, 40f * gene_NoseSize, t);
        ApplyUITransform(noseBridge, new Vector2(0f, noseY + (bridgeHeight * 0.4f)), new Vector2(8f, bridgeHeight), currentSkinColor * 0.88f);
        ApplyUITransform(noseTip, new Vector2(0f, noseY - (bridgeHeight * 0.1f)), new Vector2(Mathf.Lerp(18f, 35f, t), Mathf.Lerp(14f, 28f, t)), currentSkinColor * 0.8f);

        UpdateBeardGrowth(t, currentHairColor, Mathf.Lerp(-52f, -42f, t));

        // --- 6. LAYERED STYLED HAIR ---
        UpdateMultiClumpHair(t, currentHairColor);
    }

    private void ApplyFacialExpression(float eyeY, float eyeSpacing, float t, Color hairColor)
    {
        float browY = eyeY + Mathf.Lerp(18f, 12f, t);
        float mouthY = Mathf.Lerp(-52f, -42f, t);

        float browAngle = 0f;
        float browOffset = 0f;

        mouthBase.SetActive(false);
        mouthSmile.SetActive(false);
        mouthFrown.SetActive(false);

        Color mouthColor = new Color(0.65f, 0.25f, 0.25f);

        switch (expression)
        {
            case Expression.Neutral:
                mouthBase.SetActive(true);
                ApplyUITransform(mouthBase, new Vector2(0f, mouthY), new Vector2(Mathf.Lerp(32f, 58f, t), Mathf.Lerp(12f, 4f, t)), mouthColor);
                break;

            case Expression.Happy:
                mouthSmile.SetActive(true);
                ApplyUITransform(mouthSmile, new Vector2(0f, mouthY - 2f), new Vector2(Mathf.Lerp(38f, 62f, t), Mathf.Lerp(20f, 18f, t)), mouthColor);
                browAngle = -12f;
                browOffset = 3f;
                break;

            case Expression.Sad:
                mouthFrown.SetActive(true);
                ApplyUITransform(mouthFrown, new Vector2(0f, mouthY), new Vector2(Mathf.Lerp(30f, 50f, t), Mathf.Lerp(15f, 8f, t)), mouthColor);
                browAngle = 15f;
                break;

            case Expression.Angry:
                mouthBase.SetActive(true);
                ApplyUITransform(mouthBase, new Vector2(0f, mouthY), new Vector2(Mathf.Lerp(35f, 55f, t), 4f), mouthColor);
                browAngle = -22f;
                browOffset = -5f;
                break;

            case Expression.Surprised:
                mouthSmile.SetActive(true);
                ApplyUITransform(mouthSmile, new Vector2(0f, mouthY - 5f), new Vector2(30f, 35f), mouthColor);
                browOffset = 10f;
                break;
        }

        Vector2 browSize = new Vector2(Mathf.Lerp(30f, 45f, t), Mathf.Lerp(4f, 8f, t));
        ApplyUITransform(leftBrow, new Vector2(-eyeSpacing, browY + browOffset), browSize, hairColor);
        ApplyUITransform(rightBrow, new Vector2(eyeSpacing, browY + browOffset), browSize, hairColor);

        leftBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, browAngle);
        rightBrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -browAngle);
    }

    private void UpdateMultiClumpHair(float t, Color c)
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

            switch (hairStyle)
            {
                case HairType.ShortCrop:
                    float angle = Mathf.Lerp(-65f, 65f, (float)i / (hairClumps.Count - 1));
                    Vector3 pos = Quaternion.Euler(0, 0, angle) * Vector3.up * 52f;
                    rect.anchoredPosition = pos + new Vector3(0f, 45f, 0f);
                    rect.sizeDelta = Vector2.one * (45f * hairScaleProgress);
                    break;

                case HairType.MessyBangs:
                    float bangX = Mathf.Lerp(-45f, 45f, (float)i / (hairClumps.Count - 1));
                    float bangY = 55f - (Mathf.Abs(bangX) * 0.15f);
                    rect.anchoredPosition = new Vector2(bangX, bangY);
                    rect.sizeDelta = new Vector2(38f, 42f * hairScaleProgress);
                    break;

                case HairType.AfroPuff:
                    float afroAngle = (i / (float)hairClumps.Count) * 360f;
                    Vector3 afroPos = Quaternion.Euler(0, 0, afroAngle) * Vector3.up * 42f;
                    rect.anchoredPosition = afroPos + new Vector3(0f, 55f, 0f);
                    rect.sizeDelta = Vector2.one * (60f * hairScaleProgress);
                    break;

                case HairType.MediumSlick:
                    float slickX = Mathf.Lerp(-38f, 38f, (float)i / (hairClumps.Count - 1));
                    rect.anchoredPosition = new Vector2(slickX, 62f);
                    rect.sizeDelta = new Vector2(50f, 35f * hairScaleProgress);
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

        if (beardStyle == BeardType.StubbleOnly)
        {
            Color stubbleC = c;
            stubbleC.a = 0.4f * beardGrowth;
            ApplyUITransform(beardJaw, new Vector2(0f, -38f), new Vector2(85f, 40f), stubbleC);
            beardMustache.SetActive(false);
        }
        else if (beardStyle == BeardType.FullBeard)
        {
            ApplyUITransform(beardJaw, new Vector2(0f, -42f), new Vector2(95f, 55f * beardGrowth), c);
            ApplyUITransform(beardMustache, new Vector2(0f, mouthY + 8f), new Vector2(45f, 12f * beardGrowth), c);
        }
        else if (beardStyle == BeardType.Goatee)
        {
            ApplyUITransform(beardJaw, new Vector2(0f, -48f), new Vector2(50f, 35f * beardGrowth), c);
            ApplyUITransform(beardMustache, new Vector2(0f, mouthY + 8f), new Vector2(40f, 10f * beardGrowth), c);
        }
    }

    private void PositionEyeUI(GameObject eye, GameObject pupil, GameObject highlight, GameObject eyeBag, float x, float y, float size, float ageVal, Color skinCol)
    {
        ApplyUITransform(eye, new Vector2(x, y), new Vector2(size, size * 0.88f), Color.white);
        ApplyUITransform(pupil, Vector2.zero, new Vector2(size * 0.55f, size * 0.55f), eyeColor);
        ApplyUITransform(highlight, new Vector2(size * 0.2f, size * 0.2f), new Vector2(size * 0.25f, size * 0.25f), Color.white);

        Color bagColor = skinCol * 0.78f;
        bagColor.a = Mathf.Clamp01((ageVal - 0.45f) * 2.2f);
        ApplyUITransform(eyeBag, new Vector2(0f, -size * 0.55f), new Vector2(size * 1.1f, size * 0.35f), bagColor);
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

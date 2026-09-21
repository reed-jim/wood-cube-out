using UnityEngine;

public class StylizedFaceWithShoulders : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("0.0 = Baby, 0.3 = Child, 0.6 = Adult, 1.0 = Elder")]
    public float age = 0.0f;

    [Header("Base Colors")]
    public Color skinColor = new Color(0.95f, 0.78f, 0.66f);
    public Color eyeColor = new Color(0.25f, 0.5f, 0.85f);
    public Color hairColor = new Color(0.25f, 0.18f, 0.12f);
    public Color shirtColor = new Color(0.2f, 0.5f, 0.4f);

    // Body Parts
    private GameObject neck, shoulders, shirtCollar;
    
    // Facial Features
    private GameObject head, leftEar, rightEar, chinShadow;
    private GameObject leftEye, rightEye, pupilL, pupilR, highlightL, highlightR, eyeBagL, eyeBagR;
    private GameObject leftBrow, rightBrow, foreheadWrinkle;
    private GameObject noseBridge, noseTip;
    private GameObject mouth;

    // Persistent Randomness
    private float rand_ShoulderWidth;
    private float rand_NoseLength;
    private float rand_EyeSpacing;
    private Sprite circleSprite;

    private void Start()
    {
        circleSprite = CreateCircleSprite();
        
        // Pick random genetics
        rand_ShoulderWidth = Random.Range(0.85f, 1.2f);
        rand_NoseLength = Random.Range(0.85f, 1.2f);
        rand_EyeSpacing = Random.Range(-0.04f, 0.04f);

        BuildHierarchy();
        UpdateCharacter(age);
    }

    private void OnValidate()
    {
        if (head != null)
        {
            UpdateCharacter(age);
        }
    }

    private void BuildHierarchy()
    {
        // --- 1. BODY & NECK (Layers 0 - 2) ---
        shoulders = CreatePart("Shoulders", transform, circleSprite, shirtColor, 0);
        shirtCollar = CreatePart("ShirtCollar", transform, circleSprite, shirtColor * 0.85f, 1);
        neck = CreatePart("Neck", transform, circleSprite, skinColor * 0.85f, 2);

        // --- 2. HEAD & BASE (Layers 3 - 4) ---
        leftEar = CreatePart("LeftEar", transform, circleSprite, skinColor * 0.95f, 3);
        rightEar = CreatePart("RightEar", transform, circleSprite, skinColor * 0.95f, 3);
        head = CreatePart("Head", transform, circleSprite, skinColor, 4);
        chinShadow = CreatePart("ChinShadow", head.transform, circleSprite, skinColor * 0.85f, 5);

        // --- 3. EYES & AGING DETAILS (Layers 6 - 8) ---
        leftEye = CreatePart("LeftEye", head.transform, circleSprite, Color.white, 6);
        rightEye = CreatePart("RightEye", head.transform, circleSprite, Color.white, 6);
        
        eyeBagL = CreatePart("EyeBagL", leftEye.transform, circleSprite, skinColor * 0.75f, 6);
        eyeBagR = CreatePart("EyeBagR", rightEye.transform, circleSprite, skinColor * 0.75f, 6);

        pupilL = CreatePart("PupilL", leftEye.transform, circleSprite, eyeColor, 7);
        pupilR = CreatePart("PupilR", rightEye.transform, circleSprite, eyeColor, 7);
        
        highlightL = CreatePart("HighlightL", leftEye.transform, circleSprite, Color.white, 8);
        highlightR = CreatePart("HighlightR", rightEye.transform, circleSprite, Color.white, 8);

        // --- 4. BROWS & WRINKLES (Layer 7) ---
        leftBrow = CreatePart("LeftBrow", head.transform, circleSprite, hairColor, 7);
        rightBrow = CreatePart("RightBrow", head.transform, circleSprite, hairColor, 7);
        foreheadWrinkle = CreatePart("ForeheadWrinkle", head.transform, circleSprite, skinColor * 0.75f, 7);

        // --- 5. NOSE & MOUTH (Layer 6 - 7) ---
        noseBridge = CreatePart("NoseBridge", head.transform, circleSprite, skinColor * 0.88f, 6);
        noseTip = CreatePart("NoseTip", head.transform, circleSprite, skinColor * 0.8f, 7);
        mouth = CreatePart("Mouth", head.transform, circleSprite, new Color(0.65f, 0.25f, 0.25f), 6);
    }

    private void UpdateCharacter(float t)
    {
        // --- AGE COLOR CALCULATIONS ---
        // Hair & eyebrows turn gray/white as character ages past middle age (t > 0.5)
        Color currentHairColor = (t > 0.5f) 
            ? Color.Lerp(hairColor, new Color(0.85f, 0.85f, 0.85f), (t - 0.5f) * 2f) 
            : hairColor;

        // Skin loses saturation slightly with age
        Color currentSkinColor = Color.Lerp(skinColor, skinColor * 0.88f, t);
        head.GetComponent<SpriteRenderer>().color = currentSkinColor;

        // --- 1. SHOULDERS & NECK ---
        // Shoulders are narrow in babies, broaden in adults, slightly hunch in elders
        float shoulderW = Mathf.Lerp(1.2f, 3.8f * rand_ShoulderWidth, Mathf.Clamp01(t * 1.5f));
        if (t > 0.7f) shoulderW *= Mathf.Lerp(1.0f, 0.9f, (t - 0.7f) * 3.3f); // Elder hunch
        
        float shoulderH = Mathf.Lerp(0.8f, 1.8f, t);
        shoulders.transform.localPosition = new Vector3(0f, Mathf.Lerp(-1.2f, -1.8f, t), 0f);
        shoulders.transform.localScale = new Vector3(shoulderW, shoulderH, 1f);

        // Neck gets longer and thicker
        float neckW = Mathf.Lerp(0.5f, 1.1f, t);
        float neckH = Mathf.Lerp(0.6f, 1.2f, t);
        neck.transform.localPosition = new Vector3(0f, Mathf.Lerp(-0.7f, -1.0f, t), 0f);
        neck.transform.localScale = new Vector3(neckW, neckH, 1f);

        // Collar opening
        shirtCollar.transform.localPosition = neck.transform.localPosition + new Vector3(0f, -neckH * 0.3f, 0f);
        shirtCollar.transform.localScale = new Vector3(neckW * 1.3f, 0.4f, 1f);

        // --- 2. HEAD & JAW ---
        float headW = Mathf.Lerp(2.2f, 1.8f, t);
        float headH = Mathf.Lerp(2.0f, 2.6f, t);
        head.transform.localScale = new Vector3(headW, headH, 1f);

        // Chin shadow widens/sags with age
        chinShadow.transform.localPosition = new Vector3(0f, Mathf.Lerp(-0.4f, -0.35f, t), 0f);
        chinShadow.transform.localScale = new Vector3(Mathf.Lerp(0.6f, 0.85f, t), Mathf.Lerp(0.2f, 0.35f, t), 1f);

        // Ears grow continuously
        float earY = Mathf.Lerp(-0.1f, 0.05f, t);
        leftEar.transform.localPosition = new Vector3(-headW * 0.48f, earY, 0f);
        rightEar.transform.localPosition = new Vector3(headW * 0.48f, earY, 0f);
        leftEar.transform.localScale = new Vector3(0.4f, Mathf.Lerp(0.45f, 0.75f, t), 1f);
        rightEar.transform.localScale = new Vector3(0.4f, Mathf.Lerp(0.45f, 0.75f, t), 1f);

        // --- 3. EYES & CLEAR AGING DETAILS ---
        float eyeY = Mathf.Lerp(-0.1f, 0.22f, t);
        float eyeSpacing = Mathf.Lerp(0.52f, 0.42f, t) + rand_EyeSpacing;
        float eyeScale = Mathf.Lerp(0.55f, 0.32f, t);

        PositionEye(leftEye, pupilL, highlightL, eyeBagL, -eyeSpacing, eyeY, eyeScale, t);
        PositionEye(rightEye, pupilR, highlightR, eyeBagR, eyeSpacing, eyeY, eyeScale, t);

        // Forehead Wrinkles (Fade in for elders t > 0.5)
        float wrinkleAlpha = Mathf.Clamp01((t - 0.5f) * 2f);
        Color wColor = currentSkinColor * 0.75f;
        wColor.a = wrinkleAlpha;
        foreheadWrinkle.GetComponent<SpriteRenderer>().color = wColor;
        foreheadWrinkle.transform.localPosition = new Vector3(0f, eyeY + 0.35f, 0f);
        foreheadWrinkle.transform.localScale = new Vector3(0.6f, 0.04f, 1f);

        // Eyebrows grow gray and drop slightly closer to eyes in elders
        float browY = eyeY + Mathf.Lerp(0.18f, 0.12f, t);
        leftBrow.GetComponent<SpriteRenderer>().color = currentHairColor;
        rightBrow.GetComponent<SpriteRenderer>().color = currentHairColor;
        leftBrow.transform.localPosition = new Vector3(-eyeSpacing, browY, 0f);
        rightBrow.transform.localPosition = new Vector3(eyeSpacing, browY, 0f);
        leftBrow.transform.localScale = new Vector3(Mathf.Lerp(0.3f, 0.45f, t), Mathf.Lerp(0.04f, 0.08f, t), 1f);
        rightBrow.transform.localScale = new Vector3(Mathf.Lerp(0.3f, 0.45f, t), Mathf.Lerp(0.04f, 0.08f, t), 1f);

        // --- 4. NOSE & MOUTH ---
        float noseY = Mathf.Lerp(-0.3f, -0.05f, t);
        float bridgeHeight = Mathf.Lerp(0.05f, 0.4f * rand_NoseLength, t);

        noseBridge.transform.localPosition = new Vector3(0f, noseY + (bridgeHeight * 0.4f), 0f);
        noseBridge.transform.localScale = new Vector3(0.08f, bridgeHeight, 1f);

        // Nose tip gets visibly larger/droopier in elders
        noseTip.transform.localPosition = new Vector3(0f, noseY - (bridgeHeight * 0.1f), 0f);
        noseTip.transform.localScale = new Vector3(Mathf.Lerp(0.18f, 0.35f, t), Mathf.Lerp(0.14f, 0.28f, t), 1f);

        // Mouth widens and lips thin with age
        float mouthY = Mathf.Lerp(-0.52f, -0.42f, t);
        mouth.transform.localPosition = new Vector3(0f, mouthY, 0f);
        mouth.transform.localScale = new Vector3(Mathf.Lerp(0.32f, 0.58f, t), Mathf.Lerp(0.12f, 0.04f, t), 1f);
    }

    private void PositionEye(GameObject eye, GameObject pupil, GameObject highlight, GameObject eyeBag, float x, float y, float scale, float ageVal)
    {
        eye.transform.localPosition = new Vector3(x, y, 0f);
        eye.transform.localScale = new Vector3(scale, scale * 0.88f, 1f);

        pupil.transform.localScale = Vector3.one * 0.55f;
        
        highlight.transform.localPosition = new Vector3(0.2f, 0.2f, 0f);
        highlight.transform.localScale = Vector3.one * 0.25f;

        // Eye bags fade in past age 0.45
        float bagAlpha = Mathf.Clamp01((ageVal - 0.45f) * 2.2f);
        Color bagColor = skinColor * 0.78f;
        bagColor.a = bagAlpha;
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

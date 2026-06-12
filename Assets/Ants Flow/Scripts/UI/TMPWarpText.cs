using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMPWarpText : MonoBehaviour
{
    public AnimationCurve warpCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(0.5f, 1),
        new Keyframe(1, 0)
    );

    public float curveScale = 50f;

    private TMP_Text tmp;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    void LateUpdate()
    {
        if (tmp == null)
            tmp = GetComponent<TMP_Text>();

        WarpText();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (tmp == null)
            tmp = GetComponent<TMP_Text>();

        if (tmp != null)
            tmp.ForceMeshUpdate();
    }
#endif

    void WarpText()
    {
        tmp.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmp.textInfo;

        if (textInfo.characterCount == 0)
            return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        // Find actual text width from characters
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var c = textInfo.characterInfo[i];

            if (!c.isVisible)
                continue;

            minX = Mathf.Min(minX, c.bottomLeft.x);
            maxX = Mathf.Max(maxX, c.topRight.x);
        }

        float width = maxX - minX;

        if (width <= 0.001f)
            return;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;

            Vector3[] vertices =
                textInfo.meshInfo[matIndex].vertices;

            Vector3 midBaseline = new Vector3(
                (vertices[vertIndex].x + vertices[vertIndex + 2].x) * 0.5f,
                charInfo.baseLine,
                0
            );

            float normalizedX =
                (midBaseline.x - minX) / width;

            float y =
                warpCurve.Evaluate(normalizedX) * curveScale;

            float sampleOffset = 0.001f;

            float y1 =
                warpCurve.Evaluate(
                    Mathf.Clamp01(normalizedX - sampleOffset))
                * curveScale;

            float y2 =
                warpCurve.Evaluate(
                    Mathf.Clamp01(normalizedX + sampleOffset))
                * curveScale;

            float angle =
                Mathf.Atan2(y2 - y1, sampleOffset * width)
                * Mathf.Rad2Deg;

            Matrix4x4 matrix =
                Matrix4x4.TRS(
                    new Vector3(0, y, 0),
                    Quaternion.Euler(0, 0, angle),
                    Vector3.one);

            for (int j = 0; j < 4; j++)
            {
                vertices[vertIndex + j] -= midBaseline;
                vertices[vertIndex + j] =
                    matrix.MultiplyPoint3x4(vertices[vertIndex + j]);
                vertices[vertIndex + j] += midBaseline;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            try
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            }
            catch (System.Exception)
            {
                
            }
        }

        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}

using UnityEngine;
using TMPro;

namespace NamPhuThuy.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]

    public class CurvedText : MonoBehaviour
    {
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 10);
        public float curveScale = 10f;

        private TextMeshProUGUI textComponent;
        private Mesh mesh;
        private Vector3[] vertices;

        void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            /*curve = new AnimationCurve(
            new Keyframe(0f, 0f),   // time=0, value=0
            new Keyframe(0.5f, 1f), // time=0.5, value=1
            new Keyframe(1f, 0f)    // time=1, value=0
        );*/
        }

        void Update()
        {
            textComponent.ForceMeshUpdate();
            mesh = textComponent.mesh;
            vertices = mesh.vertices;

            int charCount = textComponent.textInfo.characterCount;
            for (int i = 0; i < charCount; i++)
            {
                var charInfo = textComponent.textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int vertexIndex = charInfo.vertexIndex;
                float xPos = (vertices[vertexIndex].x - textComponent.bounds.min.x) / textComponent.bounds.size.x;
                float yOffset = curve.Evaluate(xPos) * curveScale;

                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j].y += yOffset;
                }
            }

            mesh.vertices = vertices;
            textComponent.canvasRenderer.SetMesh(mesh);
        }
    }
}
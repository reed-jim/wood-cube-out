using UnityEngine;

namespace WoodCubeOut
{
    public class MathUtil : MonoBehaviour
    {
        public static Vector3 EvaluateArcPosition(Vector3 start, Vector3 end, float height, float t)
        {
            t = Mathf.Clamp01(t);

            // Linearly interpolate between start and end
            Vector3 linearPos = Vector3.Lerp(start, end, t);

            // Calculate height offset using a parabolic curve: sin(pi * t)
            float arc = Mathf.Sin(t * Mathf.PI) * height;

            // Add arc height to the Y axis (adjust if you want arc on different axis)
            // linearPos += arc * Vector3.one;
            linearPos.z += arc;

            return linearPos;
        }
    }
}

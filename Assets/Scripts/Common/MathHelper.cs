using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NamPhuThuy.Common
{
    
    public static class MathHelper
    {
        /// <summary>
        /// Maps a value from range [a, b] to [c, d]. Optionally clamps output to [c, d].
        /// If a == b the method returns c to avoid division by zero.
        /// </summary>
        public static float Map(float value, float a, float b, float c, float d, bool clamp = false)
        {
            if (Mathf.Approximately(a, b)) return c;
            float t = (value - a) / (b - a);
            if (clamp) t = Mathf.Clamp01(t);
            return Mathf.Lerp(c, d, t);
        }

        /// <summary>
        /// Double-precision variant (no Unity dependency).
        /// </summary>
        public static double Map(double value, double a, double b, double c, double d, bool clamp = false)
        {
            const double REL_EPS = 1e-12;
            double diff = b - a;
            if (System.Math.Abs(diff) <= REL_EPS * System.Math.Max(1.0, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b))))
                return c;
            double t = (value - a) / diff;
            if (clamp)
            {
                if (t < 0.0) t = 0.0;
                else if (t > 1.0) t = 1.0;
            }
            return c + (d - c) * t;
        }
    }
}
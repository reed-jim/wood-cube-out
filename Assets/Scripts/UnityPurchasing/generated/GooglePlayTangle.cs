// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("5VfU9+XY09z/U51TItjU1NTQ1dZCt2NXWY17Znr60a6n8rq21FSaxCu0FAsa4nVYAqXll1wFROPoxaiYx5k3xMlg8C+O/yfG6/fcPSm4Q+S3frBx5So/qiL42JPzXY6bHbD23DNicN5z/NXzzlvGty143SNWnlm1SVokDYcSqsVWzYBdBIp2dXI0HNquyp9wP5eVtNOQ6XhzA9240G/LngKJ/plWjenOK+Qm+UGpP8cg6R6a3WrSU8GV9/kxfjFdCWS12wyB+ajjPVj+z9QwWmCN0zH103+t0cYxNVfU2tXlV9Tf11fU1NVG8+PBV7BG9jNe2zWylyFjXwS8OjK4Ab6U+9aSb8gt0TlLJWhsTFubMV2zVvNlPEdIsUqbPFxtTNfW1NXU");
        private static int[] order = new int[] { 0,4,10,7,13,8,12,11,9,11,12,13,12,13,14 };
        private static int key = 213;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("tj1KLeI5XXqfUJJN9R2Lc5Rdqi6H1sRqx0hhR3rvcgOZzGmX4irtAf3ukLkzph5x4nk06bA+wsHGgKhuad5m53UhQ02FyoXpvdABb7g1TRxXiexKe2CE7tQ5Z4VBZ8sZZXKFgfYD1+PtOc/Szk5lGhNGDgJg4C5w42BuYVHjYGtj42BgYfJHV3XjBPKfAKC/rlbB7LYRUSPosfBXXHEcLBp+K8SLIyEAZyRdzMe3aQxk238qA8oExVGeix6WTGwnR+k6L6kEQmhzLYNwfdREmzpLk3JfQ2iJnQz3UEKH6m+BBiOV1+uwCI6GDLUKIE9iJtt8mWWN/5Hc2PjvL4XpB+JH0YhR42BDUWxnaEvnKeeWbGBgYGRhYvP8Bf4viOjZ+GNiYGFg");
        private static int[] order = new int[] { 9,8,12,3,6,6,8,10,11,13,10,12,12,13,14 };
        private static int key = 97;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

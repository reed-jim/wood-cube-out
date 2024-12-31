// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("1fHcNbmQ/AeBNuAva4Tbpl9Ku8uf0t3vuegoVyotKGsLmQFf2as3+pFTQGPLfo98r84/i36q+2qJrXmeCUpg7Bm7qxcZt9lTqJe39dLNsFue0YvZetBfHtojZikqY//3m7cZfqb57n6YzsgDm4kVNYIN8WwST13IQppK5qb+aRbigjrH+o9YGt4sbJ9C8HNQQn90e1j0OvSFf3Nzc3dycctOj0xiCtDgtQjQ2Mg3Rw05U4ZeGHP9i9Db0cxuSB9Gd0jIfXYrRtlp3P7H9rKNpOoD5506m3tptK3hwkIL/+XGrLOw8mWFl7hwlLNKTp9wldfiX2qFeH038R6ivl27GfvhYaTwc31yQvBzeHDwc3NywMMpC/hXwSvVsc3ymUxlAXBxc3Jz");
        private static int[] order = new int[] { 3,13,4,9,11,11,10,9,11,12,11,11,12,13,14 };
        private static int key = 114;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

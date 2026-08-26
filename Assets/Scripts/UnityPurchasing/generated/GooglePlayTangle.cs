// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("TgkdzqY/NweUTV6cZU0ZAWYGqKmYCEov6ps831clo9uwL7zi3qbVxkxMcJh8iYogIbwFPowiaaeTC+lJLZ8cPy0QGxQ3m1Wb6hAcHBwYHR7MvwQPBdv5Q8Mi+ejfaq96eXWS1yUZzJKEFbaVgj459rSe4lGVy8nxBYSsj9fvxJvZGxXAr0+XNXOuX2dyk5PMUQ7KpQSDN+tsFtP7hhSmqzv68i6/n48FM5ZGqQgKsQCy+qGg/p4GNPzQacXtMT9dxMYGsZaVEmJ81xq5wejRhA3WHBtkBUTqWIn7vJ8cEh0tnxwXH58cHB3aEbXPPSnPND5tcSnmS+uYMg33r6gvxnUlOfCQJXN8vLxtatYY3Gl0tFNRAMlbAM0vQtdcXsxagh8eHB0c");
        private static int[] order = new int[] { 6,12,7,6,5,6,10,11,10,10,10,12,13,13,14 };
        private static int key = 29;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

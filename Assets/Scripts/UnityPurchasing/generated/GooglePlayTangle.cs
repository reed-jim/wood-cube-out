// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("wGiVNaMMrEuRVEiG/FgnAfbjtKplfA8q0U75W3yHU2m/HKnniaumONd+8v6LpmcnY5Vig40D5GEaN3AfUlKiCVRMTOMj3K+4+ZE9PQit3L2bNmV8U2pJ8sm3tONr8K/uVTAjFzm6tLuLObqxuTm6urt0gKp6wRjRyf7cHyfLbbHa1HHALkw+nxxgpcwlVke/ZMjNqn/HGE5mkuVzeYLdzWIe8KSA53K864JgcMqzpwImfAk6X9A0muGKHgbHGtastU/rLND2WvmLObqZi7a9spE98z1Mtrq6ur67uKDA1T41lM93kHatRXj6aXhTyFrUmJpTzKSv/9cFjkNKFDfFLsivbQaxp91so2TY8edkymBnSD8S5BjbNQ/7x+sHSAOB+Lm4uru6");
        private static int[] order = new int[] { 8,12,2,4,11,12,6,11,12,9,12,12,13,13,14 };
        private static int key = 187;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

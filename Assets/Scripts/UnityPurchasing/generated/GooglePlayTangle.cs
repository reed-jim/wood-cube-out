// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("zHkr+TOgTHEwH0TdAm4PxqTBcrrzcH5xQfNwe3PzcHBxsAzAVqncH7NL6EuYaCWk4PL2DVB2pmVy8MGCQfNwU0F8d3hb9zn3hnxwcHB0cXKX+ni1vtd8ahG6QfK8RNRrEBWPJRs8z5zHhy05sEx9AfT9JLF4IsvHuX5Kg5Z91bC/MGcxnrbkEs9Hu4iwB5ZdlWCBfIQPRyyEuUOWlQYkMJHfjEiGaYeDvFhmrnOwcoh8cSGYboHfAankqaye2OG/SLsJDS2yTNf5avJNrHv5DSQIQ0vs4mDq6uVx42ri2Hx7FMru0VvNKuz/uCp86zXS8XD6/emmtBqq5oBAOuBZ6b5E+jNTRMpb2Jar3UzbqwwLwUekegJQnG13iUC5Zy5L0nNycHFw");
        private static int[] order = new int[] { 5,1,7,5,4,6,10,9,8,12,13,11,12,13,14 };
        private static int key = 113;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

#if UNITY_EDITOR

using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SearchEngine.Additions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Use this functions for check all SerializeField variales
        /// </summary>
        public static void CheckSerializeFields(this object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetCustomAttributes(typeof(SerializeField), false).Any())
                {
                    obj.Check(field.Name, field.GetValue(obj) != null, LogType.Warning);
                }
            }
        }

        /// <summary>
        /// Use this functions for check editor variales
        /// </summary>
        public static bool Check(this object obj, string variableName, bool condition, LogType logType)
        {
            if (!condition)
            {
                string message = string.Format("'{0}' has invalid '{1}' value", obj, variableName);
                Log(message, obj as UnityEngine.Object, logType);
            }
            return condition;
        }
        
        private static void Log(string message, UnityEngine.Object obj, LogType logType = LogType.Log)
        {
            if (logType == LogType.Log)
                Debug.Log(message, obj);
            else if (logType == LogType.Warning)
                Debug.LogWarning(message, obj);
            else
                Debug.LogError(message, obj);
        }
    }
}

#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NamPhuThuy.UI
{
    
    public static class UIValidator
    {
        /// <summary>
        /// Checks if a UnityEngine.Object (Button, Image, etc.) is null.
        /// If null, logs an error with the object name + field name.
        /// </summary>
        public static bool CheckNull<T>(T uiElement, string fieldName, Object context = null) where T : Object
        {
            if (uiElement == null)
            {
                if (context != null)
                    Debug.LogError($"[UIValidator] {fieldName} is not assigned on {context.name}", context);
                else
                    Debug.LogError($"[UIValidator] {fieldName} is not assigned.");

                return true; // is null
            }

            return false; // is not null
        }
    }
}
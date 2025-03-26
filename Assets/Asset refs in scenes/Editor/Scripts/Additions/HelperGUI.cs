#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SearchEngine.Additions
{
    public static class HelperGUI
    {
        public const int ToggleWidth = 16;

        public static Color ProTextColor
        {
            get { return new Color(0.7f, 0.7f, 0.7f); ; }
        }
        

        private static Dictionary<string, Texture2D> Backs = new Dictionary<string, Texture2D>();
        public static GUIStyle GUIStyleWindow1()
        {
            return GetGUIStyle("Window1", new RectOffset(8, 8, 8, 8));
        }
        public static GUIStyle GUIStyleWindow2()
        {
            return GetGUIStyle("Window2", new RectOffset(8, 8, 8, 8));
        }
        public static GUIStyle GUIStyleWindowHeader()
        {
            return GetGUIStyle("WindowHeader", new RectOffset(8, 8, 8, 0));
        }
        public static GUIStyle GUIStyleTableHeader()
        {
            return GetGUIStyle("WindowHeader", new RectOffset(0, 0, 0, 0));
        }
        public static GUIStyle GUIStyleTableHeader2()
        {
            return GetGUIStyle("WindowHeader2", new RectOffset(0, 0, 0, 0));
        }
        public static GUIStyle GUIStyleWindowContent()
        {
            return GetGUIStyle("WindowContent", new RectOffset(8, 8, 0, 8));
        }
        public static GUIStyle GUIStyleTableContent()
        {
            var style = GUIStyleBack2("WindowContent");
            style.padding=new RectOffset(2, 2, 0, 2);
            return style;
        }
        public static GUIStyle GUIStyleTableRow() 
        {
            var style = GUIStyleBack2("Row");
            style.padding = new RectOffset(6, 4, 5, 4);
            return style;
        }
        public static GUIStyle GUIStyleObjectField() 
        {
            var style = GUIStyleBack2("ObjectField");
            style.alignment = TextAnchor.MiddleLeft;
            return style;
        }
        public static GUIStyle GUIStyleObjectField2() 
        {
            var style = GUIStyleBack2("ObjectField2");
            style.alignment = TextAnchor.MiddleLeft;
            return style;
        }
        public static GUIStyle GUIStyleFrame1()
        {
            return GUIStyleBack2("Frame1");
        }
        public static GUIStyle GUIStyleFrame2() 
        {
            return GetGUIStyle("Window1", new RectOffset(2,2,2,2));
        }
        private static GUIStyle GetGUIStyle(string name, RectOffset margin)
        {
            var guiStyle = GUIStyleBack2(name);
            guiStyle.margin = margin;
            guiStyle.padding = new RectOffset(8, 8, 8, 8);
            return guiStyle;
        }
        private static GUIStyle GUIStyleBack2(string name)
        {
            if (EditorGUIUtility.isProSkin)
                name+="Pro";
            GUIStyle guiStyle = new GUIStyle();
            if (!Backs.ContainsKey(name))
            {
                var v = Resources.Load(string.Format("UI/{0}", name)) as Texture2D;
                if (v != null)
                    Backs[name] = v;
            }
            if (Backs.ContainsKey(name))
                guiStyle.normal.background = Backs[name];
            guiStyle.alignment = TextAnchor.MiddleCenter;
            guiStyle.border = new RectOffset(4, 4, 4, 4);
            return guiStyle;
        }


        public static void AssetField(string path, int width = 0)
        {
            AssetField(path, Path.GetFileName(path), width);
        }
        public static void AssetField(string path, string name, int width = 0)
        {
            var objIcon = AssetDatabase.GetCachedIcon(path);
            GUILayoutOption widthLayout = width != 0 ? GUILayout.Width(width) : GUILayout.ExpandWidth(true);
            GUIStyle labelStyle = objIcon != null ? EditorStyles.label : HelperGUI.RedTxtStyle();

            var rect = EditorGUILayout.BeginHorizontal(HelperGUI.GUIStyleObjectField());
                GUILayout.Label(objIcon, GUILayout.Height(16), GUILayout.Width(18));
                GUILayout.Label(name, labelStyle, widthLayout);
            EditorGUILayout.EndHorizontal();

            if (HelperGeneral.MouseDownInRect(rect) && objIcon != null)
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }


        public static Texture2D RowsButton()
        {
            string name = "RowsButton";
            if (!Backs.ContainsKey(name))
            {
                var v = Resources.Load(string.Format("UI/{0}", name)) as Texture2D;
                if (v != null)
                    Backs[name] = v;
            }
            if (Backs.ContainsKey(name))
                return Backs[name];
            return null;
        }

        private static GUIStyle toolbarStyle;        
        public static GUIStyle ToolbarStyle
        {
            get
            {
                if (toolbarStyle == null)
                {
                    toolbarStyle = new GUIStyle(GUI.skin.button);
                    toolbarStyle.margin = new RectOffset(50, 50, 8, 8);
                }
                return toolbarStyle;
            }
        }

        private static GUIStyle skinBox1;        
        public static GUIStyle GUIStyleSkinBox1
        {
            get
            {
                if (skinBox1 == null)
                {
                    skinBox1 = CopyObject(GUI.skin.box, 1) as GUIStyle; 
                    skinBox1.stretchHeight = false;
                }
                return skinBox1;
            }
        }

        private static GUIStyle fontBold;
        public static GUIStyle GUIStyleBold
        {
            get
            {
                if (fontBold == null)
                {
                    fontBold = new GUIStyle();
                    fontBold.fontStyle = FontStyle.Bold;
                }
                return fontBold;
            }
        }
        
        
        public static GUIStyle GetGuiStyleTextBold(int size = -1, TextAnchor anchor=TextAnchor.MiddleCenter)
        {
            var style = GetGuiStyleText(size);
            style.fontStyle = FontStyle.Bold;

            style.alignment = anchor;
            return style;
        }
        public static GUIStyle GetGuiStyleText(int size = -1, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var style = new GUIStyle();
            if (size > 0)
                style.fontSize = size;
            if (EditorGUIUtility.isProSkin)
                style.normal.textColor = ProTextColor;

            style.alignment = anchor;
            return style;
        }
        public static GUIStyle GetGuiStyleText(TextAnchor anchor)
        {
            var style = GetGuiStyleText();
            style.alignment = anchor;
            return style;
        }


        private static GUIStyle buttonWithoutPadding;
        public static GUIStyle GUIStyleButtonWithoutPadding
        {
            get
            {
                if (buttonWithoutPadding == null)
                {
                    buttonWithoutPadding = CopyObject(EditorStyles.miniButton, 1) as GUIStyle;
                    buttonWithoutPadding.padding = new RectOffset(0, 0, 0, 0);
                }
                return buttonWithoutPadding;
            }
        }


        public static GUIStyle GUIStyleFoldoutElemetsOffset
        {
            get
            {
                GUIStyle guiStyle = new GUIStyle();
                guiStyle.padding = new RectOffset(14, 0, 0, 0);
                return guiStyle;
            }
        }
        

        public static GUIStyle RedTxtStyle(int fontSize=0, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor= Color.red;
            if(fontSize > 0)
                style.fontSize = fontSize;
            style.alignment = anchor;
            return style;
        }

        public static object CopyObject(object orig, int nestLevel)
        {
            if (orig == null)
                return null;
            try
            {
                Type type = orig.GetType();
                object copy = Activator.CreateInstance(type);
                if (copy == null)
                    return null;
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var getm = prop.GetGetMethod();
                    var setm = prop.GetSetMethod();
                    if (getm != null && getm.IsPublic && setm != null && setm.IsPublic)
                    {
                        object val = prop.GetValue(orig, null);
                        if (nestLevel > 0 && prop.PropertyType.IsClass && !prop.PropertyType.IsArray)
                        {
                            var v = CopyObject(val, --nestLevel);
                            if (prop != null)
                                val = v;
                        }

                        prop.SetValue(copy, val, null);
                    }
                }
                return copy;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return null;
            }
        }
    }
}

#endif
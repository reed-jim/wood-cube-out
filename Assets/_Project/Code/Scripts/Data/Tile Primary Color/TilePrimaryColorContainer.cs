using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    [CreateAssetMenu(fileName = "TilePrimaryColorContainer", menuName = "ScriptableObjects/TilePrimaryColorContainer")]
    public class TilePrimaryColorContainer : ScriptableObject
    {
        public TilePrimaryColor[] tilePrimaryColors;
    }
}

[Serializable]
public class TilePrimaryColor
{
    public string spriteName;
    public Color primaryColor;
}

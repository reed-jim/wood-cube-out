using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemeDetail", menuName = "Scriptable Objects/ThemeDetail")]
public class ThemeDetail : ScriptableObject
{
    public List<UIElementColorKeyValuePair> colorPerUIElement;
}

[Serializable]
public class UIElementColorKeyValuePair
{
    public UIElement key;
    public Color value = Color.white;
}

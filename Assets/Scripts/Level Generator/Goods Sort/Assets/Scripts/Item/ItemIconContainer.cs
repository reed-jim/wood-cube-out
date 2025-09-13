using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "ItemIconContainer", menuName = "Scriptable Objects/ItemIconContainer")]
public class ItemIconContainer : ScriptableObject
{
    public Sprite[] icons;

#if UNITY_EDITOR
    [ContextMenu("AutoFill")]
    private void AutoFill()
    {
        string folderPath = "Assets/3rd/Foodie Sizzle/BaseGame/Graphics/Sprites/GameAsset/Skewer";

        string[] assetGUIDs = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        Sprite[] sprites = assetGUIDs
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
            .ToArray();

        icons = sprites;
    }
#endif
}

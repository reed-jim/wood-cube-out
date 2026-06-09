using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class FactionMapper : Singleton<FactionMapper>
{
    public List<ColorSpriteContainerPair> mapper;

    public Sprite GetAntBoxSprite(Color faction)
    {
        return mapper.First(item => item.color == faction).spriteContainer.tileSprite;
    }

    public Sprite GetAntBoxLidSprite(Color faction)
    {
        return mapper.First(item => item.color == faction).spriteContainer.tileSprite;
    }

    public Sprite GetTileSprite(Color faction)
    {
        return mapper.First(item => item.color == faction).spriteContainer.tileSprite;
    }

    public string GetAntBoxSkin(Color faction)
    {
        Debug.Log("SAFERIO " + faction);
        return mapper.First(item => item.color == faction).spriteContainer.antBoxSkin;
    }
}

[Serializable]
public class ColorSpriteContainerPair
{
    public Color color = Color.white;
    public SpriteContainer spriteContainer;
}

[Serializable]
public class SpriteContainer
{
    // public Sprite antBoxSprite;
    // public Sprite antBoxLidSprite;
    public Sprite tileSprite;
    public string antBoxSkin;
}

// #if UNITY_EDITOR
// [CustomEditor(typeof(FactionMapper))]
// public class FactionMapperEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();

//         LevelItem levelItem = (LevelItem)target;

//         GUILayout.Space(10);
//     }

//     private void MapSpriteToColor(Sprite levelSprite)
//     {

//     }
// }
// #endif

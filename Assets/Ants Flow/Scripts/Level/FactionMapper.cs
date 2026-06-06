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
        return mapper.First(item => item.color == faction).spriteContainer.antBoxSprite;
    }

    public Sprite GetAntBoxLidSprite(Color faction)
    {
        return mapper.First(item => item.color == faction).spriteContainer.antBoxSprite;
    }

    public Sprite GetTileSprite(Color faction)
    {
        return mapper.First(item => item.color == faction).spriteContainer.antBoxSprite;
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
    public Sprite antBoxSprite;
    public Sprite antBoxLidSprite;
    public Sprite tileSprite;
}

#if UNITY_EDITOR
[CustomEditor(typeof(FactionMapper))]
public class FactionMapperEditor : Editor
{
    private Sprite levelSprite;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelItem levelItem = (LevelItem)target;

        GUILayout.Space(10);

        levelSprite = (Sprite)EditorGUILayout.ObjectField("Level Sprite", levelSprite, typeof(Sprite), true);

        if (GUILayout.Button("MapSpriteToColor"))
        {
            MapSpriteToColor(levelSprite);
        }
    }

    private void MapSpriteToColor(Sprite levelSprite)
    {

    }
}
#endif

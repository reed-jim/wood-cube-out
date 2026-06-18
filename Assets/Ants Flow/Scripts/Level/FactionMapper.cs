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

    // public Dictionary<int, Color> indexesOfColors;

    public Dictionary<int, Color> GetIndexesOfColors()
    {
        Dictionary<int, Color> indexesOfColors = new Dictionary<int, Color>();

        for (int i = 0; i < mapper.Count; i++)
        {
            indexesOfColors.Add(i, mapper[i].color);
        }

        return indexesOfColors;
    }

    public Dictionary<Color, int> GetColorsOfIndexes()
    {
        Dictionary<Color, int> indexesOfColors = new Dictionary<Color, int>();

        for (int i = 0; i < mapper.Count; i++)
        {
            indexesOfColors.Add(mapper[i].color, i);
        }

        return indexesOfColors;
    }

    public List<Color> GetPoolFactionFromPoolFactionIndex(List<int> poolFactionIndex)
    {
        List<Color> result = new List<Color>();

        Dictionary<int, Color> indexesOfColors = new Dictionary<int, Color>();

        for (int i = 0; i < mapper.Count; i++)
        {
            indexesOfColors.Add(i, mapper[i].color);
        }

        for (int i = 0; i < poolFactionIndex.Count; i++)
        {
            int factionIndex = poolFactionIndex[i];

            result.Add(indexesOfColors[factionIndex]);
        }

        return result;
    }

    public List<int> GetPoolFactionIndexFromPoolFaction(List<Color> poolFaction)
    {
        List<int> result = new List<int>();

        Dictionary<Color, int> indexesOfColors = new Dictionary<Color, int>();

        for (int i = 0; i < mapper.Count; i++)
        {
            indexesOfColors.Add(mapper[i].color, i);
        }

        for (int i = 0; i < poolFaction.Count; i++)
        {
            Color faction = poolFaction[i];

            result.Add(indexesOfColors[faction]);
        }

        return result;
    }

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

    public Sprite GetDoubleAntLinkSprite(Color faction)
    {
        return mapper.First(item => item.color == faction).spriteContainer.doubleBoxLink;
    }

    public Sprite GetDoubleAntHoleSprite(Color faction)
    {
        return mapper.First(item => item.color == faction).spriteContainer.doubleBoxHole;
    }

    public string GetAntBoxSkin(Color faction)
    {
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
    public Sprite doubleBoxLink;
    public Sprite doubleBoxHole;
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

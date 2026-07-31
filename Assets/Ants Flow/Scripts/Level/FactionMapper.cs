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

    public List<Color> allFactions;
    public Dictionary<Color, SpriteContainer> spriteContainersOfColors;
    public Dictionary<int, Color> indexesOfColors;
    public Dictionary<Color, int> colorsOfIndexes;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        allFactions = new List<Color>();

        for (int i = 0; i < mapper.Count; i++)
        {
            allFactions.Add(mapper[i].color);
        }

        BuildSpriteContainersOfColors();
    }

    void OnValidate()
    {
        if (allFactions.Count == 0)
        {
            allFactions = new List<Color>();

            for (int i = 0; i < mapper.Count; i++)
            {
                allFactions.Add(mapper[i].color);
            }
        }
    }

    public Color GetRandomFaction()
    {
        return allFactions[UnityEngine.Random.Range(0, allFactions.Count)];
    }

    public List<Color> GetRandomFactions(int maxUniqueFactions, int numFaction)
    {
        maxUniqueFactions = Mathf.Clamp(maxUniqueFactions, 1, allFactions.Count);

        List<Color> subPool = allFactions
            .OrderBy(_ => UnityEngine.Random.value)
            .Take(maxUniqueFactions)
            .ToList();

        List<Color> factions = new List<Color>(numFaction);

        for (int i = 0; i < numFaction; i++)
        {
            factions.Add(subPool[UnityEngine.Random.Range(0, subPool.Count)]);
        }

        return factions;
    }

    private void BuildSpriteContainersOfColors()
    {
        spriteContainersOfColors = new Dictionary<Color, SpriteContainer>();

        for (int i = 0; i < mapper.Count; i++)
        {
            ColorSpriteContainerPair pair = mapper[i];

            spriteContainersOfColors.Add(pair.color, pair.spriteContainer);
        }

        indexesOfColors = new Dictionary<int, Color>();

        for (int i = 0; i < mapper.Count; i++)
        {
            indexesOfColors.Add(i, mapper[i].color);
        }

        colorsOfIndexes = new Dictionary<Color, int>();

        for (int i = 0; i < mapper.Count; i++)
        {
            colorsOfIndexes.Add(mapper[i].color, i);
        }
    }

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
        return spriteContainersOfColors[faction].tileSprite;
        // return mapper.First(item => item.color == faction).spriteContainer.tileSprite;
    }

    public Sprite GetAntBoxLidSprite(Color faction)
    {
        return spriteContainersOfColors[faction].tileSprite;
        // return mapper.First(item => item.color == faction).spriteContainer.tileSprite;
    }

    public Sprite GetTileSprite(Color faction)
    {
        return spriteContainersOfColors[faction].tileSprite;
        // return mapper.First(item => item.color == faction).spriteContainer.tileSprite;
    }

    public Sprite GetDoubleAntLinkSprite(Color faction)
    {
        return spriteContainersOfColors[faction].doubleBoxLink;
        // return mapper.First(item => item.color == faction).spriteContainer.doubleBoxLink;
    }

    public Sprite GetDoubleAntHoleSprite(Color faction)
    {
        return spriteContainersOfColors[faction].doubleBoxHole;
        // return mapper.First(item => item.color == faction).spriteContainer.doubleBoxHole;
    }

    public string GetAntBoxSkin(Color faction)
    {
        return spriteContainersOfColors[faction].antBoxSkin;
        // return mapper.First(item => item.color == faction).spriteContainer.antBoxSkin;
    }

    public Sprite GetSpawningTileBoxTop(Color faction)
    {
        return spriteContainersOfColors[faction].spawningTileBoxTop;
    }

    public Color GetFaction(Sprite tileSprite)
    {
        return mapper.First(item => item.spriteContainer.tileSprite == tileSprite).color;
    }

    public Sprite GetBucketSprite(Color faction)
    {
        return spriteContainersOfColors[faction].bucketSprite;
    }

    public Sprite GetBucketTopSprite(Color faction)
    {
        return spriteContainersOfColors[faction].bucketTopSprite;
    }

    public Sprite GetGrainSprite(Color faction)
    {
        return spriteContainersOfColors[faction].grainSprite;
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
    public Sprite bucketSprite;
    public Sprite bucketTopSprite;
    public Sprite grainSprite;

    public Sprite tileSprite;
    public Sprite doubleBoxLink;
    public Sprite doubleBoxHole;
    public Sprite spawningTileBoxTop;
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

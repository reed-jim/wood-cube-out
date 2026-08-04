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

    public Dictionary<Color, SpriteContainer> spriteContainersOfColors;
    public Dictionary<int, Color> indexesOfColors;
    public Dictionary<Color, int> colorsOfIndexes;


    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        BuildSpriteContainersOfColors();
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


    private SpriteContainer ResolveContainer(Color faction)
    {
        if (spriteContainersOfColors != null && spriteContainersOfColors.TryGetValue(faction, out SpriteContainer container))
        {
            return container;
        }

        int nearestIndex = -1;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < mapper.Count; i++)
        {
            Color color = mapper[i].color;

            float distance =
                (color.r - faction.r) * (color.r - faction.r) +
                (color.g - faction.g) * (color.g - faction.g) +
                (color.b - faction.b) * (color.b - faction.b);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearestIndex = i;
            }
        }

        if (nearestIndex < 0)
        {
            Debug.LogError($"FactionMapper: mapper rỗng, không resolve được faction {faction}");
            return null;
        }

        Debug.LogWarning($"FactionMapper: faction {faction} không khớp chính xác màu nào (lệch float?), " +
                         $"dùng màu gần nhất {mapper[nearestIndex].color}. Nên chỉnh lại data cho khớp.");

        return mapper[nearestIndex].spriteContainer;
    }

    private SpriteContainer GetSpriteContainer(Color faction)
    {
        int level = 0;

        level = GameController.Instance.useProfile.CurrentLevel;
        if (level >= 200)
        {
            return ResolveContainer(faction);
        }
        else
        {
            return spriteContainersOfColors[faction];
        }
    }

    public Sprite GetAntBoxSprite(Color faction)
    {
        return GetSpriteContainer(faction).tileSprite;
    }

    public Sprite GetAntBoxLidSprite(Color faction)
    {
        return GetSpriteContainer(faction).tileSprite;
    }

    public Sprite GetTileSprite(Color faction)
    {
        return GetSpriteContainer(faction).tileSprite;
    }

    public Sprite GetSpawningBoxTileSprite(Color faction)
    {
        return ResolveContainer(faction).spawningTileBox;
    }

    public Sprite GetDoubleAntLinkSprite(Color faction)
    {
        return GetSpriteContainer(faction).doubleBoxLink;
    }

    public Sprite GetDoubleAntHoleSprite(Color faction)
    {
        return GetSpriteContainer(faction).doubleBoxHole;
    }

    public string GetAntBoxSkin(Color faction)
    {
        return GetSpriteContainer(faction).antBoxSkin;
    }

    public Sprite GetSpawningTileBoxTop(Color faction)
    {
        return ResolveContainer(faction).spawningTileBoxTop;
    }

    public Color GetFaction(Sprite tileSprite)
    {
        return mapper.First(item => item.spriteContainer.tileSprite == tileSprite).color;
    }

#if UNITY_EDITOR
    [Header("Editor Helper")]
    public List<Sprite> sourceSprites = new List<Sprite>();

    public enum TargetSpriteField
    {
        TileSprite,
        DoubleBoxLink,
        DoubleBoxHole,
        SpawningTileBoxTop,
        SpawningTileBox
    }
    public TargetSpriteField targetField = TargetSpriteField.TileSprite;

    public void MapSpritesByHexName()
    {
        if (sourceSprites == null || sourceSprites.Count == 0)
        {
            Debug.LogWarning("Source Sprites list is empty!");
            return;
        }

        int count = 0;
        foreach (var pair in mapper)
        {
            if (pair.spriteContainer == null)
            {
                pair.spriteContainer = new SpriteContainer();
            }

            string hexRGB = ColorUtility.ToHtmlStringRGB(pair.color).ToLower();
            string hexRGBA = ColorUtility.ToHtmlStringRGBA(pair.color).ToLower();

            Sprite matchedSprite = sourceSprites.Find(s =>
            {
                if (s == null) return false;
                string nameCleaned = s.name.Replace("#", "").Replace("0x", "").Trim().ToLower();
                return nameCleaned == hexRGB || nameCleaned == hexRGBA;
            });

            if (matchedSprite != null)
            {
                switch (targetField)
                {
                    case TargetSpriteField.TileSprite:
                        pair.spriteContainer.tileSprite = matchedSprite;
                        break;
                    case TargetSpriteField.DoubleBoxLink:
                        pair.spriteContainer.doubleBoxLink = matchedSprite;
                        break;
                    case TargetSpriteField.DoubleBoxHole:
                        pair.spriteContainer.doubleBoxHole = matchedSprite;
                        break;
                    case TargetSpriteField.SpawningTileBoxTop:
                        pair.spriteContainer.spawningTileBoxTop = matchedSprite;
                        break;
                    case TargetSpriteField.SpawningTileBox:
                        pair.spriteContainer.spawningTileBox = matchedSprite;
                        break;
                }
                count++;
                Debug.Log($"Mapped color {hexRGB} to sprite {matchedSprite.name} for field {targetField}");
            }
        }

        Debug.Log($"Successfully mapped {count} sprites!");
        EditorUtility.SetDirty(this);
    }
#endif
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
    public Sprite spawningTileBoxTop;
    public Sprite spawningTileBox;
    public string antBoxSkin;
}

#if UNITY_EDITOR
[CustomEditor(typeof(FactionMapper))]
public class FactionMapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FactionMapper mapperInstance = (FactionMapper)target;

        GUILayout.Space(15);
        GUILayout.Label("Auto-Map Sprites Tool", EditorStyles.boldLabel);
        if (GUILayout.Button("Map Sprites By Hex Color Name", GUILayout.Height(30)))
        {
            mapperInstance.MapSpritesByHexName();
        }
    }
}
#endif

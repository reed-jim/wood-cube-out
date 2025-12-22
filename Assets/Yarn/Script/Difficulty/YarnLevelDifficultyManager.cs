using System;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy.Common;
using UnityEngine;
using static GameEnum;
using Random = UnityEngine.Random;

public class YarnLevelDifficultyManager : Singleton<YarnLevelDifficultyManager>
{
    [SerializeField] private IntVariable currentLevelId;

    public void GenerateAddionalFaction(Transform level, BasePiece[] pieces)
    {
        DebugLogger.Log();
        GameFaction[] currentFactions = pieces.Select(item => item.Faction).ToArray();

        Dictionary<GameFaction, int> finalFactions = new Dictionary<GameFaction, int>();
        List<GameFaction> innerPieceFactions = new List<GameFaction>();

        for (int i = 0; i < currentFactions.Length; i++)
        {
            if (finalFactions.ContainsKey(currentFactions[i]))
            {
                finalFactions[currentFactions[i]]++;
            }
            else
            {
                finalFactions.Add(currentFactions[i], 1);
            }
        }

        // fix current faction not divisible by 3
        foreach (var faction in finalFactions.Keys)
        {
            if (finalFactions[faction] % 3 != 0)
            {
                int numToAdd = 3 - (finalFactions[faction] % 3);

                for (int i = 0; i < numToAdd; i++)
                {
                    innerPieceFactions.Add(faction);
                }
            }
        }

        // add new factions
        int numFactionsToAdd = Mathf.Min(3 * Random.Range(0, 1), 2 * pieces.Length);
        // numFactionsToAdd = AdjustNumToAddPieces(pieces.Length, numFactionsToAdd);
        DebugLogger.LogFrog(message: $"Num Factions to add: {numFactionsToAdd}", context: this);
        DebugLogger.LogFrog(message: $"Pieces Length: {pieces.Length}", context: this);

        numFactionsToAdd -= numFactionsToAdd % 3;

        if (pieces.Length <= 3)
        {
            numFactionsToAdd = 0;
        }

        int index = 0;
        int currentFactionIndex = 0;

        List<GameFaction> validFactionsForInnerPieces = GetEnumValuesList<GameFaction>();

        foreach (var faction in finalFactions.Keys)
        {
            validFactionsForInnerPieces.Remove(faction);
        }

        validFactionsForInnerPieces.Remove(GameFaction.None);

        for (int i = 0; i < numFactionsToAdd; i++)
        {
            innerPieceFactions.Add(validFactionsForInnerPieces[currentFactionIndex]);

            index++;

            if (index % 3 == 0)
            {
                currentFactionIndex++;

                if (currentFactionIndex >= validFactionsForInnerPieces.Count)
                {
                    currentFactionIndex = 0;
                }
            }
        }

        Dictionary<BasePiece, Transform> generatedPieces = new Dictionary<BasePiece, Transform>();

        int totalInnerPieces = innerPieceFactions.Count;
        DebugLogger.Log(message: $"innerPieceFactions.Count before expand: {innerPieceFactions.Count}", context: this);

        GameFaction currentInnerPieceFaction = GameFaction.None;

        // generate new pieces for each layer
        for (int layer = 1; layer < 30; layer++)
        {
            DebugLogger.Log(message: $"innerPieceFactions.Count = {innerPieceFactions.Count}", context: this);
            if (innerPieceFactions.Count <= 0)
            {
                break;
            }

            for (int i = 0; i < pieces.Length; i++)
            {
                if (innerPieceFactions.Count <= 0)
                {
                    break;
                }

                var rootPiece = pieces[i];
                if (rootPiece == null)
                {
                    DebugLogger.Log(message: $"rootPiece is null at index {i}", context: this);
                    continue;
                }

                currentInnerPieceFaction = pieces[i].Faction;

                int _childCount = rootPiece.transform.childCount;

                for (int j = 0; j < _childCount; j++)
                {
                    BasePiece childPiece = pieces[i].transform.GetComponent<BasePiece>();

                    if (childPiece != null)
                    {
                        currentInnerPieceFaction = childPiece.Faction;
                    }
                }

              
                BasePiece generatedPiece = Instantiate(pieces[i].gameObject, level).GetComponent<BasePiece>();
                // DebugLogger.Log(message: $"Generated new piece for layer {layer} at index {i}", context: generatedPiece);

                generatedPiece.gameObject.SetActive(false);

                generatedPiece.gameObject.name = $"Generated Piece {i} {layer}";

                generatedPieces.Add(generatedPiece, pieces[i].transform);

                GameFaction faction = innerPieceFactions.First();

                if (faction == currentInnerPieceFaction)
                {
                    if (innerPieceFactions.Any(item => item != currentInnerPieceFaction))
                    {
                        faction = innerPieceFactions.First(item => item != currentInnerPieceFaction);
                    }
                    else
                    {
                        Debug.Log("SAFERIO 0 NOTPASS");
                    }
                }

                Debug.Log("SAFERIO CHECK " + currentInnerPieceFaction + "/" + faction + "/" + i);

                currentInnerPieceFaction = faction;

                innerPieceFactions.Remove(faction);

                generatedPiece.Faction = faction;
                generatedPiece.PieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);
                generatedPiece.Lock();
            }
        }

        float scaleOffset = 0.1f; // 0.001f

        // Make new generated pieces child of original pieces
        foreach (var piece in generatedPieces.Keys)
        {
            piece.transform.SetParent(generatedPieces[piece]);

            piece.transform.localPosition = Vector3.zero;
            piece.transform.localRotation = Quaternion.identity;
            piece.transform.localScale *=
                1 - ((piece.transform.GetSiblingIndexWithComponent<BasePiece>() + 1) * scaleOffset);
        }

        int EnsureDivisibleByThree(int value, int addedValue)
        {
            int remainder = (value + addedValue) % 3;
            if (remainder != 0)
            {
                value -= remainder;
            }

            return value;
        }
    }

    (int[] expanded, int[] added) ExpandArrayEnsureCountsDivisibleBy3(int[] original, int multiply)
    {
        int targetSize = original.Length * multiply;
        List<int> result = new List<int>(original);
        List<int> added = new List<int>();

        // Count occurrences in original
        Dictionary<int, int> counts = original
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        // Make each count divisible by 3
        foreach (var kv in counts.ToList())
        {
            int remainder = kv.Value % 3;
            if (remainder != 0)
            {
                int need = 3 - remainder;
                for (int i = 0; i < need; i++)
                {
                    result.Add(kv.Key);
                    added.Add(kv.Key);
                }
            }
        }

        // Fill rest (if any) to reach target size
        System.Random rnd = new System.Random();
        int[] uniqueValues = counts.Keys.ToArray();

        while (result.Count < targetSize)
        {
            int value = Random.Range(0, 20);
            result.Add(value);
            added.Add(value);
        }

        // Trim if over target size
        if (result.Count > targetSize)
        {
            int removeCount = result.Count - targetSize;
            // Try to remove from the "added" part first
            for (int i = 0; i < removeCount && added.Count > 0; i++)
            {
                int toRemove = added[added.Count - 1];
                added.RemoveAt(added.Count - 1);
                result.Remove(toRemove);
            }

            // If still over target, trim extra from end
            if (result.Count > targetSize)
            {
                result = result.Take(targetSize).ToList();
            }
        }

        // Final fix to ensure each count divisible by 3
        var newCounts = result.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        foreach (var kv in newCounts)
        {
            int remainder = kv.Value % 3;
            if (remainder != 0)
            {
                int need = 3 - remainder;
                for (int i = 0; i < need; i++)
                {
                    result.Add(kv.Key);
                    added.Add(kv.Key);
                }
            }
        }

        return (result.ToArray(), added.ToArray());
    }

    #region Manual add pieces per level

    [Serializable]
    public struct LevelTargetPieces
    {
        [Min(1)] public int levelIndex;
        [Min(0)] public int targetPieces;
    }

    [Header("Manual Piece Add (Per Level)")]
    [Tooltip("Extra pieces to add on top of the prefab pieces for a given level.")]
    [SerializeField]
    private List<LevelTargetPieces> manualExtraPiecesByLevel = new List<LevelTargetPieces>()
    {
        new LevelTargetPieces { levelIndex = 1, targetPieces = -1 }, // Buffer
        new LevelTargetPieces { levelIndex = 1, targetPieces = 9 },
        new LevelTargetPieces { levelIndex = 2, targetPieces = 18 },
        new LevelTargetPieces { levelIndex = 3, targetPieces = 39 },
        new LevelTargetPieces { levelIndex = 4, targetPieces = 60 },
        new LevelTargetPieces { levelIndex = 5, targetPieces = 45 },
        new LevelTargetPieces { levelIndex = 6, targetPieces = 51 },
        new LevelTargetPieces { levelIndex = 7, targetPieces = 48 },
        new LevelTargetPieces { levelIndex = 8, targetPieces = 75 },
        new LevelTargetPieces { levelIndex = 9, targetPieces = 54 },
        new LevelTargetPieces { levelIndex = 10, targetPieces = 102 },
        new LevelTargetPieces { levelIndex = 11, targetPieces = 57 },
        new LevelTargetPieces { levelIndex = 12, targetPieces = 81 },
        new LevelTargetPieces { levelIndex = 13, targetPieces = 69 },
        new LevelTargetPieces { levelIndex = 14, targetPieces = 81 },
        new LevelTargetPieces { levelIndex = 15, targetPieces = 42 },
        new LevelTargetPieces { levelIndex = 16, targetPieces = 102 },
        new LevelTargetPieces { levelIndex = 17, targetPieces = 81 },
        new LevelTargetPieces { levelIndex = 18, targetPieces = 81 },
        new LevelTargetPieces { levelIndex = 19, targetPieces = 81 },
        new LevelTargetPieces { levelIndex = 20, targetPieces = 57 },
        new LevelTargetPieces { levelIndex = 21, targetPieces = 72 },
        new LevelTargetPieces { levelIndex = 22, targetPieces = 114 },
        new LevelTargetPieces { levelIndex = 23, targetPieces = 60 },
        new LevelTargetPieces { levelIndex = 24, targetPieces = 60 },
        new LevelTargetPieces { levelIndex = 25, targetPieces = 72 },
        new LevelTargetPieces { levelIndex = 26, targetPieces = 54 },
        new LevelTargetPieces { levelIndex = 27, targetPieces = 30 },
        new LevelTargetPieces { levelIndex = 28, targetPieces = 90 },
        new LevelTargetPieces { levelIndex = 29, targetPieces = 75 },
        new LevelTargetPieces { levelIndex = 30, targetPieces = 117 },
        new LevelTargetPieces { levelIndex = 31, targetPieces = 69 },
        new LevelTargetPieces { levelIndex = 32, targetPieces = 60 },
        new LevelTargetPieces { levelIndex = 33, targetPieces = 99 },
        new LevelTargetPieces { levelIndex = 34, targetPieces = 60 },
        new LevelTargetPieces { levelIndex = 35, targetPieces = 69 },
        new LevelTargetPieces { levelIndex = 36, targetPieces = 129 },
        new LevelTargetPieces { levelIndex = 37, targetPieces = 60 },
        new LevelTargetPieces { levelIndex = 38, targetPieces = 69 },
        new LevelTargetPieces { levelIndex = 39, targetPieces = 105 },
        new LevelTargetPieces { levelIndex = 40, targetPieces = 42 },
        new LevelTargetPieces { levelIndex = 41, targetPieces = 81 },
        new LevelTargetPieces { levelIndex = 42, targetPieces = 114 },
        new LevelTargetPieces { levelIndex = 43, targetPieces = 123 },
        new LevelTargetPieces { levelIndex = 44, targetPieces = 72 },
        new LevelTargetPieces { levelIndex = 45, targetPieces = 102 },
        new LevelTargetPieces { levelIndex = 46, targetPieces = 57 },
        new LevelTargetPieces { levelIndex = 47, targetPieces = 72 },
        new LevelTargetPieces { levelIndex = 48, targetPieces = 93 },
        new LevelTargetPieces { levelIndex = 49, targetPieces = 63 },
        new LevelTargetPieces { levelIndex = 50, targetPieces = 75 },
    };


    /// <summary>
    /// Returns configured extra pieces to add for this level, or 0 if not configured.
    /// </summary>
    public int GetManualExtraPieces(int levelIndex)
    {
        for (int i = manualExtraPiecesByLevel.Count - 1; i >= 0; i--)
        {
            if (manualExtraPiecesByLevel[i].levelIndex == levelIndex)
            {
                return Mathf.Max(0, manualExtraPiecesByLevel[i].targetPieces);
            }
        }

        return 0;
    }

    /// <summary>
    /// Sets/overrides the extra pieces for a level at runtime.
    /// </summary>
    public void SetManualExtraPieces(int levelIndex, int extraPiecesToAdd)
    {
        extraPiecesToAdd = Mathf.Max(0, extraPiecesToAdd);

        for (int i = 0; i < manualExtraPiecesByLevel.Count; i++)
        {
            if (manualExtraPiecesByLevel[i].levelIndex == levelIndex)
            {
                manualExtraPiecesByLevel[i] = new LevelTargetPieces
                {
                    levelIndex = levelIndex,
                    targetPieces = extraPiecesToAdd,
                };
                return;
            }
        }

        manualExtraPiecesByLevel.Add(new LevelTargetPieces
        {
            levelIndex = levelIndex,
            targetPieces = extraPiecesToAdd,
        });
    }

    private int AdjustNumToAddPieces(int origianlPieces, int numToAddPieces)
    {
        int currentTarget = manualExtraPiecesByLevel[currentLevelId.Value].targetPieces;
        DebugLogger.Log(message: $"currentTarget: {currentTarget}", context: this);
        int newNumToAdd = Mathf.Max(currentTarget - origianlPieces, numToAddPieces);

        return newNumToAdd;
    }

    #endregion
}
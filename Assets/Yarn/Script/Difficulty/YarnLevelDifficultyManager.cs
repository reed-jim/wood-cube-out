using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameEnum;

public class YarnLevelDifficultyManager : Singleton<YarnLevelDifficultyManager>
{
    // public void GenerateAddionalFaction(BasePiece[] pieces)
    // {
    //     int totalPiece = pieces.Length;

    //     int numAdditionalFactionPerPiece = Random.Range(1, 3);
    //     numAdditionalFactionPerPiece = 1;

    //     int totalAdditionalFaction = numAdditionalFactionPerPiece * totalPiece;

    //     List<GameFaction> poolFaction = new List<GameFaction>();

    //     int index = 0;
    //     GameFaction currentFaction = GameFaction.YarnBlue;

    //     for (int i = 0; i < totalAdditionalFaction; i++)
    //     {
    //         if (index % 3 == 0)
    //         {
    //             // currentFaction = GetRandomEnumValue<GameFaction>();
    //             currentFaction = (GameFaction)Random.Range(0, 8);
    //         }

    //         poolFaction.Add(currentFaction);

    //         index++;
    //     }

    //     for (int i = 0; i < totalPiece; i++)
    //     {
    //         GameFaction avoidFaction = pieces[i].Faction;

    //         PieceProperty pieceProperty = pieces[i].PieceServiceLocator.pieceProperty;

    //         pieceProperty.PoolFactions = new GameFaction[numAdditionalFactionPerPiece];

    //         // Debug.Log($"SAFERIO {i} {avoidFaction}");

    //         for (int j = 0; j < numAdditionalFactionPerPiece; j++)
    //         {
    //             BasePiece generatedPiece = Instantiate(pieces[i].gameObject, pieces[i].transform).GetComponent<BasePiece>();

    //             generatedPiece.gameObject.name = $"Generated Piece {i} {j}";

    //             GameFaction faction = GameFaction.YarnBlue;

    //             if (poolFaction.Any(item => item != avoidFaction))
    //             {
    //                 faction = poolFaction.First(item => item != avoidFaction);
    //             }
    //             else
    //             {
    //                 faction = (GameFaction)Random.Range(0, 8);
    //             }

    //             avoidFaction = faction;

    //             generatedPiece.Faction = faction;
    //             generatedPiece.transform.localPosition = Vector3.zero;
    //             generatedPiece.transform.localRotation = Quaternion.identity;
    //             generatedPiece.transform.localScale *= 1 - (j + 1) * 0.1f;

    //             generatedPiece.PieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);

    //             poolFaction.Remove(faction);

    //             pieceProperty.PoolFactions[j] = faction;

    //             generatedPiece.Lock();

    //             Debug.Log($"SAFERIO {i} {j} {faction} {generatedPiece.name} {1 - (j + 1) * 0.1f}");
    //         }
    //     }
    // }

    public void GenerateAddionalFaction(BasePiece[] pieces)
    {
        GameFaction[] currentFactions = pieces.Select(item => item.Faction).ToArray();

        int[] expanded;
        int[] added;
        (expanded, added) = ExpandArrayEnsureCountsDivisibleBy3(currentFactions.Select(item => (int)item).ToArray(), 2);

        int originalPieceIndex = 0;
        int count = 0;

        for (int i = 0; i < added.Length; i++)
        {
            BasePiece generatedPiece = Instantiate(pieces[originalPieceIndex].gameObject, pieces[originalPieceIndex].transform).GetComponent<BasePiece>();

            generatedPiece.gameObject.name = $"Generated Piece {originalPieceIndex} {count}";

            GameFaction faction = (GameFaction)added[i];

            generatedPiece.Faction = faction;
            generatedPiece.transform.localPosition = Vector3.zero;
            generatedPiece.transform.localRotation = Quaternion.identity;
            generatedPiece.transform.localScale *= 1 - (count + 1) * 0.1f;

            generatedPiece.PieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);

            generatedPiece.Lock();

            count++;

            if (count == 2)
            {
                if (originalPieceIndex < pieces.Length - 1)
                {
                    originalPieceIndex++;
                }

                count = 0;
            }

            Debug.Log($"SAFERIO {originalPieceIndex} {count} {faction} {generatedPiece.name} {1 - (count + 1) * 0.1f}");
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
                result = result.Take(targetSize).ToList();
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
}

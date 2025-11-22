using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameEnum;

public class YarnLevelDifficultyManager : Singleton<YarnLevelDifficultyManager>
{
    public void GenerateAddionalFaction(Transform level, BasePiece[] pieces)
    {
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

        int currentTotalPieces = pieces.Length;

        // fix current faction not divisible by 3
        foreach (var faction in finalFactions.Keys)
        {
            if (finalFactions[faction] % 3 != 0)
            {
                int numToAdd = 3 - finalFactions[faction] % 3;

                currentTotalPieces += numToAdd;

                for (int i = 0; i < numToAdd; i++)
                {
                    innerPieceFactions.Add(faction);
                }
            }
        }

        // add new factions
        int numFactionsToAdd = Mathf.Min(3 * Random.Range(0, 1), 2 * pieces.Length);
        numFactionsToAdd -= numFactionsToAdd % 3;
        // numFactionsToAdd = Mathf.Min(finalFactions.Keys.Count * 3 - currentTotalPieces, 0);

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

        int originalPieceIndex = 0;
        int count = 0;

        Dictionary<BasePiece, Transform> generatedPieces = new Dictionary<BasePiece, Transform>();

        int totalInnerPieces = innerPieceFactions.Count;

        GameFaction currentInnerPieceFaction = GameFaction.None;

        for (int i = 0; i < totalInnerPieces; i++)
        {
            BasePiece generatedPiece = Instantiate(pieces[originalPieceIndex].gameObject, level).GetComponent<BasePiece>();

            generatedPiece.gameObject.SetActive(false);

            generatedPiece.gameObject.name = $"Generated Piece {originalPieceIndex} {count}";

            generatedPieces.Add(generatedPiece, pieces[originalPieceIndex].transform);

            GameFaction faction = innerPieceFactions.First();

            // Debug.Log("SAFERIO " + otherInnerPieceFactionSameParent.Count + "/" + (innerPieceFactions.Any(item => !otherInnerPieceFactionSameParent.Contains(item))));

            if (currentInnerPieceFaction == GameFaction.None)
            {
                currentInnerPieceFaction = pieces[originalPieceIndex].Faction;
            }

            if (innerPieceFactions.Any(item => item != currentInnerPieceFaction))
            {
                faction = innerPieceFactions.First(item => item != currentInnerPieceFaction);
            }

            currentInnerPieceFaction = faction;

            innerPieceFactions.Remove(faction);

            generatedPiece.Faction = faction;
            generatedPiece.PieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);
            generatedPiece.Lock();

            // Debug.Log($"SAFERIO {originalPieceIndex} {count} {faction} {generatedPiece.name} {1 - (count + 1) * 0.1f}");

            count++;

            if (count == 2)
            {
                if (originalPieceIndex < pieces.Length - 1)
                {
                    originalPieceIndex++;
                }

                count = 0;
                currentInnerPieceFaction = GameFaction.None;
            }
        }

        // Make new generated pieces child of original pieces
        foreach (var piece in generatedPieces.Keys)
        {
            piece.transform.SetParent(generatedPieces[piece]);

            piece.transform.localPosition = Vector3.zero;
            piece.transform.localRotation = Quaternion.identity;
            piece.transform.localScale *= 1 - (piece.transform.GetSiblingIndex() + 1) * 0.000f;
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

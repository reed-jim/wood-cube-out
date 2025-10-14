using System.Collections.Generic;
using UnityEngine;
using static GameEnum;

public static class YarnLevelDifficultyManager
{
    public static void GenerateAddionalFaction(BasePiece[] pieces)
    {
        int totalPiece = pieces.Length;

        int numAdditionalFactionPerPiece = Random.Range(0, 2);

        int totalAdditionalFaction = numAdditionalFactionPerPiece * totalPiece;

        List<GameFaction> poolFaction = new List<GameFaction>();

        int index = 0;
        GameFaction currentFaction = GameFaction.Red;

        for (int i = 0; i < totalAdditionalFaction; i++)
        {
            if (index % 3 == 0)
            {
                currentFaction = GetRandomEnumValue<GameFaction>();
            }

            poolFaction.Add(currentFaction);

            index++;
        }

        for (int i = 0; i < totalPiece; i++)
        {
            PieceProperty pieceProperty = pieces[i].PieceServiceLocator.pieceProperty;

            pieceProperty.PoolFactions = new GameFaction[numAdditionalFactionPerPiece];

            for (int j = 0; j < numAdditionalFactionPerPiece; j++)
            {
                GameFaction faction = poolFaction[Random.Range(0, poolFaction.Count)];

                poolFaction.Remove(faction);

                pieceProperty.PoolFactions[j] = faction;
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameEnum;

public class YarnLevelDifficultyManager : Singleton<YarnLevelDifficultyManager>
{
    public void GenerateAddionalFaction(BasePiece[] pieces)
    {
        int totalPiece = pieces.Length;

        int numAdditionalFactionPerPiece = Random.Range(1, 3);
        numAdditionalFactionPerPiece = 1;

        int totalAdditionalFaction = numAdditionalFactionPerPiece * totalPiece;

        List<GameFaction> poolFaction = new List<GameFaction>();

        int index = 0;
        GameFaction currentFaction = GameFaction.Red;

        for (int i = 0; i < totalAdditionalFaction; i++)
        {
            if (index % 3 == 0)
            {
                // currentFaction = GetRandomEnumValue<GameFaction>();
                currentFaction = (GameFaction)Random.Range(0, 8);
            }

            poolFaction.Add(currentFaction);

            index++;
        }

        for (int i = 0; i < totalPiece; i++)
        {
            GameFaction avoidFaction = pieces[i].Faction;

            PieceProperty pieceProperty = pieces[i].PieceServiceLocator.pieceProperty;

            pieceProperty.PoolFactions = new GameFaction[numAdditionalFactionPerPiece];

            Debug.Log($"SAFERIO {i} {avoidFaction}");

            for (int j = 0; j < numAdditionalFactionPerPiece; j++)
            {
                BasePiece generatedPiece = Instantiate(pieces[i].gameObject, pieces[i].transform).GetComponent<BasePiece>();

                generatedPiece.gameObject.name = $"Generated Piece {i} {j}";

                GameFaction faction = poolFaction.First(item => item != avoidFaction);

                avoidFaction = faction;

                generatedPiece.Faction = faction;
                generatedPiece.transform.localPosition = Vector3.zero;
                generatedPiece.transform.localRotation = Quaternion.identity;
                generatedPiece.transform.localScale *= 1 - (j + 1) * 0.1f;

                generatedPiece.PieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);

                poolFaction.Remove(faction);

                pieceProperty.PoolFactions[j] = faction;

                generatedPiece.Lock();

                Debug.Log($"SAFERIO {i} {j} {faction}");
            }
        }
    }
}

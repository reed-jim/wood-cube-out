using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class BossArea : MonoBehaviour
    {
        public List<BaseTile> coveredTiles;

        public void Setup(List<BaseTile> tiles)
        {
            coveredTiles = tiles;

            Vector3 averagePosition = Vector3.zero;

            for (int i = 0; i < coveredTiles.Count; i++)
            {
                BaseTile coveredTile = coveredTiles[i];

                if (coveredTile == null)
                {
                    continue;
                }

                coveredTile.isLocked = true;

                averagePosition += TileManager.Ins.GetTilePosition(coveredTile);
            }

            averagePosition /= coveredTiles.Count;
            averagePosition = averagePosition.ChangeZ(transform.position.z);

            transform.position = averagePosition;
            transform.localScale = 2 * GamePersistentVariable.tileDistance;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class WarriorAntBox : WashingMachineItem
    {
        public override void CheckAntsPath(bool isAlsoCheckLose = true)
        {
            if (remainingNumber <= 0)
            {
                return;
            }

            if (isMoving || GamePersistentVariable.isLevelLost || isGhostMode || WashingMachineManager.Ins.IsAnyGhostAntBox())
            {
                return;
            }

            int addedTile = 0;

            int max = TileManager.Ins.tiles.Count(tile => tile.isDone && tile.Faction == faction);

            bool isCenter = Math.Abs(transform.position.x) < 0.1f;
            Vector3 antBoxPosition = transform.position;

            if (isCenter)
            {
                antBoxPosition = antBoxPosition.AddX(3);
            }

            List<BaseTile> foundTiles = TileManager.Ins.GetFirstNMovableTilesForWarrioAnts(max, antBoxPosition);

            foundTiles = foundTiles
                // .Where(item => item.numStepToReach < 5)
                .OrderBy(item => item.position.y)
                .ThenBy(item => item.position.x)
                .Take(remainingNumber)
                .ToList();

            for (int i = 0; i < foundTiles.Count; i++)
            {
                BaseTile tile = foundTiles[i];

                if (!tilesToMove.Contains(tile))
                {
                    tilesToMove.Add(tile);

                    tile.isAntApproaching = true;

                    addedTile++;
                }
            }

            remainingNumber -= addedTile;

            if (!GamePersistentVariable.isPendingLoseLevel && isPlayingWarningLoseAnimation)
            {
                TurnOffWarningLoseAnimation();
            }

            if (tilesToMove.Count > 0)
            {
                isStucking = false;

                if (spawningAntCoroutine == null)
                {
                    spawningAntCoroutine = StartCoroutine(SpawningAnts());
                }
            }
            else if (isAlsoCheckLose)
            {
                isStucking = true;

                WashingMachineManager.Ins.CheckLoseLevel();
            }
        }
    }
}

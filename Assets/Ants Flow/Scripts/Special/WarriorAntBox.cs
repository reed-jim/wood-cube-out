using System;
using System.Collections.Generic;
using System.Linq;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class WarriorAntBox : WashingMachineItem
    {
        public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
        {
            base.Init(washingMachineStartSlot, faction, number, onCompleteAction);

            this.number = SpecialElementManager.Ins.bossArea.GetNextAntNumberOfBox();
            remainingNumber = this.number;

            numberText.text = $"{this.number}";
        }

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

            int max = remainingNumber;

            bool isCenter = Math.Abs(transform.position.x) < 0.1f;
            Vector3 antBoxPosition = transform.position;

            if (isCenter)
            {
                antBoxPosition = antBoxPosition.AddX(3);
            }

            List<BaseTile> foundTiles = TileManager.Ins.GetFirstNMovableTilesForWarriorAnts(max, antBoxPosition);

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
                    spawningAntCoroutine = StartCoroutine(SpawningAnts(antType: GameConstants.warriorAnt));
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

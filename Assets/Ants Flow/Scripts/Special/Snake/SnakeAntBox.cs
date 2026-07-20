using System;
using System.Collections.Generic;
using System.Linq;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class SnakeAntBox : WashingMachineItem
    {
        public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
        {
            base.Init(washingMachineStartSlot, faction, number, onCompleteAction);

            this.number = SpecialElementManager.Ins.snake.GetNextAntNumberOfBox();
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

            TileManager.Ins.FindSingleAntPath(SpecialElementManager.Ins.snake.snakeNestTile);

            for (int i = 0; i < remainingNumber; i++)
            {
                BaseTile tile = SpecialElementManager.Ins.snake.snakeNestTile;

                tilesToMove.Add(tile);
            }

            remainingNumber -= tilesToMove.Count;

            if (!GamePersistentVariable.isPendingLoseLevel && isPlayingWarningLoseAnimation)
            {
                TurnOffWarningLoseAnimation();
            }

            if (tilesToMove.Count > 0)
            {
                isStucking = false;

                if (spawningAntCoroutine == null)
                {
                    spawningAntCoroutine = StartCoroutine(SpawningAnts(antType: GameConstants.snakeAnt));
                }
            }
        }
    }
}

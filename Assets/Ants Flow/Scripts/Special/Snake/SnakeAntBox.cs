using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class SnakeAntBox : WashingMachineItem
    {
        private const string snakeAntBoxName = "box_snake";
        public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
        {
            base.Init(washingMachineStartSlot, faction, number, onCompleteAction);

            this.number = SpecialElementManager.Ins.snake.GetNextAntNumberOfBox();
            remainingNumber = this.number;
            skeletonAnimation.Skeleton.SetSkin(snakeAntBoxName);
            numberText.text = $"{this.number}";
        }

        public override void OpenTheBox()
        {
            Tween.LocalPositionY(numberText.transform, numberText.transform.localPosition.y - 1f, duration: 0.3f);

            doubleAntBoxHole.gameObject.SetActive(true);

            if (isDouble)
            {
                // doubleAntBoxHole.gameObject.SetActive(true);
                // doubleAntBoxHole.sprite = FactionMapper.Ins.GetDoubleAntHoleSprite(faction);

                skeletonAnimation.AnimationState.SetAnimation(0, doubleBoxAnimation, false);
            }
            else
            {
                skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, false);
            }
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

            BaseTile snakeNestTile = SpecialElementManager.Ins.snake.snakeNestTile;
            TileManager.Ins.FindSingleAntPath(snakeNestTile);

            if (snakeNestTile.path != null && snakeNestTile.path.Count > 0)
            {
                for (int i = 0; i < remainingNumber; i++)
                {
                    tilesToMove.Add(snakeNestTile);
                }

                remainingNumber -= tilesToMove.Count;
            }

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
            else
            {
                if (isInSlot && WashingMachineManager.Ins.isLevelContainingNewSpecialElements)
                {
                    isStucking = true;
                }
            }
        }

        public override void CheckAntsPathGhostMode()
        {
            if (isMoving)
            {
                return;
            }

            List<AntsFlow.BaseTile> tilesForGhostAnt = new List<AntsFlow.BaseTile>();
            for (int i = 0; i < number; i++)
            {
                tilesForGhostAnt.Add(SpecialElementManager.Ins.snake.snakeNestTile);
            }

            for (int i = 0; i < tilesForGhostAnt.Count; i++)
            {
                tilesForGhostAnt[i].isAntApproaching = true;
            }

            StartCoroutine(SpawningSnakeGhostAnts(tilesForGhostAnt));

            isGhostMode = true;
        }

        private System.Collections.IEnumerator SpawningSnakeGhostAnts(List<AntsFlow.BaseTile> tilesForGhostAnts)
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.25f);

            List<AntsFlow.BaseTile> tilesToMove = new List<AntsFlow.BaseTile>();
            tilesToMove.AddRange(tilesForGhostAnts);

            isStucking = false;

            while (tilesToMove.Count > 0)
            {
                BaseAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(GameConstants.snakeAnt);

                ant.transform.position = transform.position.AddY(1.2f).ChangeZ(-0.1f);
                ant.transform.eulerAngles = Vector3.zero;

                ant.Init(faction);
                ant.SetGhostMode();
                ant.MoveToTileGhostMode(tilesToMove[0]);

                SoundManager.Instance.PlaySoundAntPop();

                number--;
                numberText.text = $"{number}";

                if (!isAnimating)
                {
                    SaferioTween.Punch(numberText.transform, 1.1f, onCompleteAction: () =>
                    {
                        isAnimating = false;
                    });

                    isAnimating = true;
                }

                tilesToMove.RemoveAt(0);

                GamePersistentVariable.numActiveAnt++;

                yield return waitForSeconds;
            }

            if (number == 0)
            {
                SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
                {
                    if (washingMachineSlot.washingMachineItem != null)
                    {
                        washingMachineSlot.washingMachineItem = null;
                    }

                    InvokeAntBoxDoneEvent(this);
                });

                isInSlot = false;

                WashingMachineManager.Ins.CompleteGhostAntBox(this);
            }

            spawningAntCoroutine = null;
        }
    }
}

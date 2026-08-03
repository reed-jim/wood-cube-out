using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class WarriorAntBox : WashingMachineItem
    {
        private const string warriorAntBoxName = "box_spider";

        public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
        {
            base.Init(washingMachineStartSlot, faction, number, onCompleteAction);

            this.number = SpecialElementManager.Ins.bossArea.GetNextAntNumberOfBox();
            remainingNumber = this.number;
            skeletonAnimation.Skeleton.SetSkin(warriorAntBoxName);
            numberText.text = $"{this.number}";
        }

        public override void OpenTheBox()
        {
            Tween.LocalPositionY(numberText.transform, numberText.transform.localPosition.y - 1f, duration: 0.3f);

            doubleAntBoxHole.gameObject.SetActive(true);

            if (isDouble)
            {
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
                    if (gameObject.activeInHierarchy)
                        spawningAntCoroutine = StartCoroutine(SpawningAnts(antType: GameConstants.warriorAnt));
                }
            }
            else if (isAlsoCheckLose)
            {
                isStucking = true;

                WashingMachineManager.Ins.CheckLoseLevel();
            }
        }

        // Warrior ant chỉ dùng tile "done" làm waypoint để bắn boss rồi quay về (không ăn mất tile),
        // nên waypoint tái sử dụng được. GetFirstNMovableTilesForWarriorAnts mỗi lần chỉ trả về tối đa
        // số tile done trong hành lang tới boss, nên 1 lần CheckAntsPath chỉ nhả được 1 đợt bằng độ dài
        // hành lang. Nếu vẫn còn count, tự re-check để nhả tiếp cho hết thay vì phải bốc hộp khác lên bàn.
        protected override void OnFinishSpawningAntsBatch()
        {
            if (remainingNumber <= 0)
            {
                return;
            }

            if (isMoving || GamePersistentVariable.isLevelLost || isGhostMode ||
                WashingMachineManager.Ins.IsAnyGhostAntBox())
            {
                return;
            }

            CheckAntsPath();
        }

        public override void CheckAntsPathGhostMode()
        {
            if (isMoving)
            {
                return;
            }

            bool isCenter = Math.Abs(transform.position.x) < 0.1f;
            Vector3 antBoxPosition = transform.position;

            if (isCenter)
            {
                antBoxPosition = antBoxPosition.AddX(3);
            }

            List<BaseTile> tilesForGhostAnt = TileManager.Ins.GetFirstNMovableTilesForWarriorAnts(number, antBoxPosition);

            for (int i = 0; i < tilesForGhostAnt.Count; i++)
            {
                tilesForGhostAnt[i].isAntApproaching = true;
            }

            StartCoroutine(SpawningWarriorGhostAnts(tilesForGhostAnt));

            isGhostMode = true;
        }

        private IEnumerator SpawningWarriorGhostAnts(List<BaseTile> tilesForGhostAnts)
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.25f);

            List<BaseTile> tilesToMove = new List<BaseTile>();
            tilesToMove.AddRange(tilesForGhostAnts);

            isStucking = false;

            while (tilesToMove.Count > 0)
            {
                BaseAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(GameConstants.warriorAnt);

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

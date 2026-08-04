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

            for (int i = 0; i < foundTiles.Count; i++)
            {
                BaseTile tile = foundTiles[i];

                tilesToMove.Add(tile);

                tile.isAntApproaching = true;

                addedTile++;
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

         
            StartCoroutine(SpawningWarriorGhostAnts());

            isGhostMode = true;
        }

        private IEnumerator SpawningWarriorGhostAnts()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.25f);

            isStucking = false;

            Vector3 standBasePosition = transform.position;
            int minY = 1;
            int minX = 6;
            List<Node> targetNodes = new List<Node>();
            if (TileManager.Ins.nodes != null && TileManager.Ins.nodes.Count > 0)
            {
                targetNodes = TileManager.Ins.nodes
                    .Where(node => (node.y == minY && node.x >= minX) || (node.x == minX && node.y >= minY))
                    .ToList();
            }

            if (targetNodes.Count == 0)
            {
                if (TileManager.Ins.bottomMostNodes != null)
                {
                    targetNodes.AddRange(TileManager.Ins.bottomMostNodes);
                }
                if (TileManager.Ins.leftMostNodes != null)
                {
                    targetNodes.AddRange(TileManager.Ins.leftMostNodes);
                }
            }

            if (targetNodes.Count > 0)
            {
                Node edgeNode = targetNodes
                    .OrderBy(node => ((Vector2)node.initialPosition - (Vector2)transform.position).sqrMagnitude)
                    .First();

                standBasePosition = edgeNode.initialPosition;
            }

            int spawnCount = number;

            for (int i = 0; i < spawnCount; i++)
            {
                WarriorAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(GameConstants.warriorAnt) as WarriorAnt;

                ant.transform.position = transform.position.AddY(1.2f).ChangeZ(-0.1f);
                ant.transform.eulerAngles = Vector3.zero;

                ant.Init(faction);
                ant.SetGhostMode();

                // Tản nhẹ theo trục X để nhiều warrior không chồng khít lên nhau.
                Vector3 standPosition = standBasePosition.AddX((i - (spawnCount - 1) * 0.5f) * 0.25f);

                ant.GhostAttackBoss(standPosition, onComplete: null);

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

                yield return waitForSeconds;
            }

            if (number == 0)
            {
                SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
                {
                    if (washingMachineSlot != null && washingMachineSlot.washingMachineItem != null)
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

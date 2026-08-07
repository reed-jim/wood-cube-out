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

        public void ShuffleBox(int newNumber)
        {
            this.number = newNumber;
            this.remainingNumber = newNumber;

            numberText.text = $"{this.number}";

            isShuffled = true;
            isMoving = false;
            isInSlot = false;
            isGhostMode = false;
            isStucking = false;
            isPlayingWarningLoseAnimation = false;

            transform.localScale = Vector3.zero;
            Tween.Scale(transform, 2.3f, duration: 0.3f)
            .OnComplete(() =>
            {
                isAnimating = false;
            });

            isAnimating = true;

            shadow.sprite = shadows[0];
            ApplyShadowDepth();

            skeletonAnimation.Skeleton.SetSkin(warriorAntBoxName);
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
            skeletonAnimation.AnimationState.SetAnimation(0, beginAnimation, false);

            if (washingMachineStartSlot != null)
            {
                Setup(washingMachineStartSlot);
            }
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

            Vector3 antBoxPosition = transform.position;

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
                        spawningAntCoroutine = StartCoroutine(SpawningWarriorAnts());
                }
            }
            else if (isAlsoCheckLose)
            {
                isStucking = true;

                WashingMachineManager.Ins.CheckLoseLevel();
            }
        }

        private IEnumerator SpawningWarriorAnts()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.35f);

            int currentAntIndex = 0;

            while (tilesToMove.Count > 0)
            {
                if (GamePersistentVariable.isLevelLost)
                {
                    for (int i = 0; i < tilesToMove.Count; i++)
                    {
                        tilesToMove[i].isAntApproaching = false;
                    }

                    remainingNumber += tilesToMove.Count;
                    tilesToMove.Clear();
                    break;
                }

                if (GamePersistentVariable.isPauseGame)
                {
                    yield return waitForSeconds;
                    continue;
                }

                WarriorAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(GameConstants.warriorAnt) as WarriorAnt;

                ant.transform.position = transform.position.AddY(1.2f).ChangeZ(-0.1f);
                ant.transform.eulerAngles = Vector3.zero;

                ant.Init(faction);
                ant.MoveTo(tilesToMove[currentAntIndex], tilesToMove.Count);

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

                tilesToMove.RemoveAt(currentAntIndex);
                currentAntIndex = 0;

                GamePersistentVariable.numActiveAnt++;

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
            }

            spawningAntCoroutine = null;

            OnFinishSpawningAntsBatch();
        }


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


            float? safeHalfWidth = null;
            float cameraX = 0f;

            if (WashingMachineManager.Ins != null && WashingMachineManager.Ins.mainCamera != null)
            {
                cameraX = WashingMachineManager.Ins.mainCamera.transform.position.x;
                safeHalfWidth = GamePersistentVariable.screenSizeWorld.x * 0.5f * 0.85f;

                List<Node> onScreenNodes = targetNodes
                    .Where(node => Mathf.Abs(node.initialPosition.x - cameraX) <= safeHalfWidth.Value)
                    .ToList();

                if (onScreenNodes.Count > 0)
                {
                    targetNodes = onScreenNodes;
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

                // Chặn cứng lần cuối: dù standBasePosition đã lọc theo vùng an toàn, offset tản đội hình
                // ở trên vẫn có thể đẩy vài warrior ở rìa đội hình lố ra khỏi vùng camera nhìn thấy.
                if (safeHalfWidth.HasValue)
                {
                    standPosition.x = Mathf.Clamp(standPosition.x, cameraX - safeHalfWidth.Value, cameraX + safeHalfWidth.Value);
                }

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

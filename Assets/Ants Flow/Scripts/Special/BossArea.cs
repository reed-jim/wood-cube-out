using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace AntsFlow
{
    public class BossArea : MonoBehaviour
    {
        public List<BaseTile> coveredTiles;
        public List<int> numAntPerBox;


        public HashSet<Node> borderNodes;

        public TMP_Text healthText;
        public LazyHealthBar lazyHealthBar;

        [Header("Spine Animation")]
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SpineAnimation][SerializeField] private string idleAnimation = "idle";
        [SpineAnimation][SerializeField] private string idle2Animation = "idle2";
        [SpineAnimation][SerializeField] private string hitAnimation = "hit";
        [SpineAnimation][SerializeField] private string dieAnimation = "die";

        [Header("Hit FX Spine Animation")]
        [SerializeField] private List<SkeletonAnimation> hitFxList = new List<SkeletonAnimation>();
        [SpineAnimation][SerializeField] private string hitFxAnimation1 = "hit";
        [SpineAnimation][SerializeField] private string hitFxAnimation2 = "hit2";

        private int maxHealth;
        private int currentHealth;
        private bool isAnimating;
        private bool isDead;
        private bool isPlayingIdle2;

        // Level còn boss sống thì chưa được tính win (kể cả khi đã clear hết tile màu thường
        // bằng booster). Boss chỉ hết chặn win khi đã chết.
        public bool BlocksWin => gameObject.activeInHierarchy && !isDead;

        public void Setup(List<Node> nodes, BossAreaConfig bossAreaConfig)
        {
            coveredTiles = new List<BaseTile>();

            BuildBorderNodes(nodes);

            Vector3 averagePosition = Vector3.zero;

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                if (node == null)
                {
                    continue;
                }

                averagePosition += node.initialPosition;

                if (node.tile != null)
                {
                    node.tile.isLocked = true;
                    coveredTiles.Add(node.tile);
                }
            }

            if (nodes.Count > 0)
            {
                averagePosition /= nodes.Count;
            }
            averagePosition = averagePosition.ChangeZ(transform.position.z);

            transform.position = averagePosition;

            float cellWidth = GamePersistentVariable.tileDistance.x > 0 ? GamePersistentVariable.tileDistance.x : GamePersistentVariable.tileMapScaleFactor;
            float cellHeight = GamePersistentVariable.tileDistance.y > 0 ? GamePersistentVariable.tileDistance.y : GamePersistentVariable.tileMapScaleFactor;

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                if (spriteRenderer.drawMode == SpriteDrawMode.Sliced || spriteRenderer.drawMode == SpriteDrawMode.Tiled)
                {
                    spriteRenderer.size = new Vector2(bossAreaConfig.numCoverColumn * cellWidth, bossAreaConfig.numCoverRow * cellHeight);
                    transform.localScale = Vector3.one;
                }
                else if (spriteRenderer.sprite != null)
                {
                    Vector2 spriteSize = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;
                    if (spriteSize.x > 0 && spriteSize.y > 0)
                    {
                        transform.localScale = new Vector3(
                            (bossAreaConfig.numCoverColumn * cellWidth) / spriteSize.x,
                            (bossAreaConfig.numCoverRow * cellHeight) / spriteSize.y,
                            1f
                        );
                    }
                    else
                    {
                        transform.localScale = new Vector3(bossAreaConfig.numCoverColumn * cellWidth, bossAreaConfig.numCoverRow * cellHeight, 1f);
                    }
                }
                else
                {
                    transform.localScale = new Vector3(bossAreaConfig.numCoverColumn * cellWidth, bossAreaConfig.numCoverRow * cellHeight, 1f);
                }
            }
            else
            {
                transform.localScale = new Vector3(bossAreaConfig.numCoverColumn * cellWidth, bossAreaConfig.numCoverRow * cellHeight, 1f);
            }

            maxHealth = bossAreaConfig.bossHealth;
            currentHealth = maxHealth;
            isDead = false;
            isPlayingIdle2 = false;
            healthText.text = $"{currentHealth}";

            lazyHealthBar.SetProgress((float)currentHealth / maxHealth);

            GenerateAntNumberPerBox(bossAreaConfig.spawnOrder.Count, currentHealth);

            DeactivateAllHitFx();

            PlayIdle();
        }

        private void DeactivateAllHitFx()
        {
            if (hitFxList != null)
            {
                for (int i = 0; i < hitFxList.Count; i++)
                {
                    if (hitFxList[i] != null)
                    {
                        if (hitFxList[i].AnimationState != null)
                        {
                            hitFxList[i].AnimationState.ClearTracks();
                        }
                        if (hitFxList[i].Skeleton != null)
                        {
                            hitFxList[i].Skeleton.SetToSetupPose();
                            hitFxList[i].Skeleton.SetSlotsToSetupPose();
                        }
                        hitFxList[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        private void PlayRandomHitFx()
        {
            if (hitFxList == null || hitFxList.Count == 0)
            {
                return;
            }

            List<SkeletonAnimation> availableFx = new List<SkeletonAnimation>();
            List<SkeletonAnimation> allValidFx = new List<SkeletonAnimation>();

            for (int i = 0; i < hitFxList.Count; i++)
            {
                SkeletonAnimation fx = hitFxList[i];
                if (fx != null)
                {
                    allValidFx.Add(fx);
                    if (!fx.gameObject.activeSelf)
                    {
                        availableFx.Add(fx);
                    }
                }
            }

            if (allValidFx.Count == 0)
            {
                return;
            }

            SkeletonAnimation selectedFx = availableFx.Count > 0
                ? availableFx[Random.Range(0, availableFx.Count)]
                : allValidFx[Random.Range(0, allValidFx.Count)];

            List<string> animOptions = new List<string>();
            if (!string.IsNullOrEmpty(hitFxAnimation1))
            {
                animOptions.Add(hitFxAnimation1);
            }
            if (!string.IsNullOrEmpty(hitFxAnimation2))
            {
                animOptions.Add(hitFxAnimation2);
            }

            if (animOptions.Count == 0)
            {
                return;
            }

            string chosenAnim = animOptions[Random.Range(0, animOptions.Count)];

            selectedFx.gameObject.SetActive(true);
            selectedFx.Initialize(false);
            if (selectedFx.Skeleton != null)
            {
                selectedFx.Skeleton.SetToSetupPose();
                selectedFx.Skeleton.SetSlotsToSetupPose();
            }
            if (selectedFx.AnimationState != null)
            {
                selectedFx.AnimationState.ClearTracks();
                TrackEntry trackEntry = selectedFx.AnimationState.SetAnimation(0, chosenAnim, false);
                if (trackEntry != null)
                {
                    trackEntry.MixDuration = 0f;
                    trackEntry.Complete += (entry) =>
                    {
                        if (selectedFx != null && selectedFx.gameObject != null)
                        {
                            selectedFx.gameObject.SetActive(false);
                        }
                    };
                }
                if (selectedFx.Skeleton != null)
                {
                    selectedFx.AnimationState.Apply(selectedFx.Skeleton);
                    selectedFx.Skeleton.UpdateWorldTransform();
                }
            }
            selectedFx.LateUpdate();
        }

        private void PlayIdle()
        {
            if (isDead || skeletonAnimation == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(idleAnimation) && !string.IsNullOrEmpty(idle2Animation))
            {
                string animToPlay = isPlayingIdle2 ? idle2Animation : idleAnimation;
                isPlayingIdle2 = !isPlayingIdle2;

                TrackEntry trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, animToPlay, false);
                trackEntry.Complete += OnIdleComplete;
            }
            else if (!string.IsNullOrEmpty(idleAnimation))
            {
                skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, true);
            }
            else if (!string.IsNullOrEmpty(idle2Animation))
            {
                skeletonAnimation.AnimationState.SetAnimation(0, idle2Animation, true);
            }
        }

        private void OnIdleComplete(TrackEntry trackEntry)
        {
            trackEntry.Complete -= OnIdleComplete;

            if (isDead || skeletonAnimation == null)
            {
                return;
            }

            if (skeletonAnimation.AnimationState.GetCurrent(0) == trackEntry)
            {
                PlayIdle();
            }
        }

        // Rìa boss = các node kề (4 hướng) của node trong vùng boss NHƯNG không thuộc vùng boss.
        private void BuildBorderNodes(List<Node> coveredNodes)
        {
            borderNodes = new HashSet<Node>();

            HashSet<Node> coveredNodeSet = new HashSet<Node>();

            for (int i = 0; i < coveredNodes.Count; i++)
            {
                if (coveredNodes[i] != null)
                {
                    coveredNodeSet.Add(coveredNodes[i]);
                }
            }

            foreach (Node coveredNode in coveredNodeSet)
            {
                if (coveredNode.neighbors == null)
                {
                    continue;
                }

                foreach (Node neighbor in coveredNode.neighbors)
                {
                    if (neighbor != null && !coveredNodeSet.Contains(neighbor))
                    {
                        borderNodes.Add(neighbor);
                    }
                }
            }
        }

        private void UpdateHealth()
        {
            healthText.text = $"{currentHealth}";

            if (!isAnimating)
            {
                SaferioTween.Punch(healthText.transform, 1.1f, onCompleteAction: () =>
                {
                    isAnimating = false;
                });

                isAnimating = true;
            }
        }

        public void Hit()
        {
            if (isDead)
            {
                return;
            }

            currentHealth--;
            SoundManager.Instance.PlaySound(GameConstants.bossAreaHitSound);

            UpdateHealth();

            DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

            damageText.ShowDamage(1, transform.position.MinusZALittle());
            lazyHealthBar.SetProgress((float)currentHealth / maxHealth);

            PlayRandomHitFx();

            if (currentHealth <= 0)
            {
                isDead = true;
                SoundManager.Instance.PlaySound(GameConstants.bossAreaDieSound);

                DeactivateAllHitFx();

                for (int i = 0; i < coveredTiles.Count; i++)
                {
                    coveredTiles[i].isLocked = false;
                }

                // Boss vừa bị hạ: mở khóa các tile dưới boss và kích hoạt các hộp đang chờ trên slot kiểm tra lại đường đi
                RecheckInSlotAntBoxes();

                // Boss vừa bị hạ: tile màu thường có thể đã được clear hết từ trước (vd booster
                // clear-one-color) nên win đang bị chặn chờ boss. Re-check để win ngay bây giờ.
                TileManager.Ins.CheckWinLevel();

                if (skeletonAnimation != null && !string.IsNullOrEmpty(dieAnimation))
                {
                    TrackEntry trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, dieAnimation, false);
                    trackEntry.Complete += OnDieComplete;
                }
                else
                {
                    SaferioTween.ScaleDownWithPunch(transform, 0);
                }
            }
            else
            {
                if (skeletonAnimation != null && !string.IsNullOrEmpty(hitAnimation))
                {
                    TrackEntry trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, hitAnimation, false);
                    trackEntry.Complete += OnHitComplete;
                }
            }
        }

        private void RecheckInSlotAntBoxes()
        {
            if (WashingMachineManager.Ins == null || WashingMachineManager.Ins.washingMachineSlots == null)
            {
                return;
            }

            var slots = WashingMachineManager.Ins.washingMachineSlots;

            for (int i = 0; i < slots.Count; i++)
            {
                WashingMachineItem item = slots[i].washingMachineItem;

                if (item != null && item.isInSlot)
                {
                    item.CheckAntsPath();
                }
            }
        }

        private void OnHitComplete(TrackEntry trackEntry)
        {
            trackEntry.Complete -= OnHitComplete;

            if (isDead || skeletonAnimation == null)
            {
                return;
            }

            if (skeletonAnimation.AnimationState.GetCurrent(0) == trackEntry)
            {
                PlayIdle();
            }
        }

        private void OnDieComplete(TrackEntry trackEntry)
        {
            trackEntry.Complete -= OnDieComplete;
            SaferioTween.ScaleDownWithPunch(transform, 0);
        }

        public void GenerateAntNumberPerBox(int totalBox, int maxHealth)
        {
            numAntPerBox = new List<int>();

            if (totalBox <= 0)
            {
                return;
            }

            const int minAllowed = 5;
            const int maxAllowed = 95;

            if (totalBox == 1)
            {
                numAntPerBox.Add(Mathf.Clamp(maxHealth, 1, maxAllowed));
                return;
            }

            int remainingHealth = maxHealth;

            for (int i = 0; i < totalBox; i++)
            {
                int remainingBoxes = totalBox - i;

                // Hộp cuối cùng: lấy toàn bộ số count còn lại để đảm bảo chắc chắn tổng bằng maxHealth (clear được boss)
                if (remainingBoxes == 1)
                {
                    numAntPerBox.Add(Mathf.Clamp(remainingHealth, 1, maxAllowed));
                    break;
                }

                // Nếu máu còn lại đủ để chia tối thiểu 5 cho mỗi hộp
                if (remainingHealth >= remainingBoxes * minAllowed)
                {
                    // Để các hộp phía sau không bị vượt quá maxAllowed (95), hộp này phải gánh tối thiểu lượng dư
                    int requiredMin = remainingHealth - (remainingBoxes - 1) * maxAllowed;
                    int minUnits = Mathf.Max(minAllowed, requiredMin);
                    minUnits = Mathf.CeilToInt(minUnits / 5f) * 5;

                    // Để các hộp phía sau có tối thiểu minAllowed (5), hộp này có thể lấy tối đa:
                    int maxUnits = remainingHealth - (remainingBoxes - 1) * minAllowed;
                    maxUnits = Mathf.Min(maxUnits, maxAllowed);
                    int maxUnitsMultipleOf5 = (maxUnits / 5) * 5;

                    if (maxUnitsMultipleOf5 < minUnits)
                    {
                        int units = Mathf.Clamp(minUnits, minAllowed, maxAllowed);
                        numAntPerBox.Add(units);
                        remainingHealth -= units;
                    }
                    else
                    {
                        int stepCount = ((maxUnitsMultipleOf5 - minUnits) / 5) + 1;
                        int units = minUnits + UnityEngine.Random.Range(0, stepCount) * 5;
                        numAntPerBox.Add(units);
                        remainingHealth -= units;
                    }
                }
                else
                {
                    // Fallback trong trường hợp máu quá ít (không đủ 5 cho mỗi hộp)
                    int units = Mathf.Max(1, remainingHealth / remainingBoxes);
                    numAntPerBox.Add(units);
                    remainingHealth -= units;
                }
            }
        }

        public int GetNextAntNumberOfBox()
        {
            if (numAntPerBox == null || numAntPerBox.Count == 0)
            {
                return currentHealth > 0 ? currentHealth : 5;
            }

            int antNumber = numAntPerBox[0];

            numAntPerBox.RemoveAt(0);

            return antNumber;
        }
    }
}

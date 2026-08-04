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
            selectedFx.Skeleton.SetToSetupPose();
            TrackEntry trackEntry = selectedFx.AnimationState.SetAnimation(0, chosenAnim, false);
            selectedFx.AnimationState.Apply(selectedFx.Skeleton);

            trackEntry.Complete += (entry) =>
            {
                if (selectedFx != null && selectedFx.gameObject != null)
                {
                    selectedFx.gameObject.SetActive(false);
                }
            };
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

            UpdateHealth();

            DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

            damageText.ShowDamage(1, transform.position.MinusZALittle());
            lazyHealthBar.SetProgress((float)currentHealth / maxHealth);

            PlayRandomHitFx();

            if (currentHealth <= 0)
            {
                isDead = true;

                DeactivateAllHitFx();

                for (int i = 0; i < coveredTiles.Count; i++)
                {
                    coveredTiles[i].isLocked = false;
                }

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

            int totalUnits = maxHealth;

            // Base units per number
            int baseUnits = totalUnits / totalBox;
            int remainder = totalUnits % totalBox;

            for (int i = 0; i < totalBox; i++)
            {
                int units = baseUnits;

                // Distribute remaining units
                if (i < remainder)
                    units++;

                numAntPerBox.Add(units);
            }
        }

        public int GetNextAntNumberOfBox()
        {
            int antNumber = numAntPerBox[0];

            numAntPerBox.RemoveAt(0);

            return antNumber;
        }
    }
}

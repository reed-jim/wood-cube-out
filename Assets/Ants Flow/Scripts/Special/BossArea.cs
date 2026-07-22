using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

namespace AntsFlow
{
    public class BossArea : MonoBehaviour
    {
        public List<BaseTile> coveredTiles;
        public List<int> numAntPerBox;

        public TMP_Text healthText;
        public LazyHealthBar lazyHealthBar;

        private int maxHealth;
        private int currentHealth;
        private bool isAnimating;

        public void Setup(List<Node> nodes, BossAreaConfig bossAreaConfig)
        {
            coveredTiles = new List<BaseTile>();

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
            healthText.text = $"{currentHealth}";

            lazyHealthBar.SetProgress((float)currentHealth / maxHealth);

            GenerateAntNumberPerBox(bossAreaConfig.spawnOrder.Count, currentHealth);
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
            currentHealth--;

            UpdateHealth();

            DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

            damageText.ShowDamage(1, transform.position.MinusZALittle());
            lazyHealthBar.SetProgress((float)currentHealth / maxHealth);

            if (currentHealth <= 0)
            {
                SaferioTween.ScaleDownWithPunch(transform, 0);

                for (int i = 0; i < coveredTiles.Count; i++)
                {
                    coveredTiles[i].isLocked = false;
                }
            }
        }

        public void GenerateAntNumberPerBox(int totalBox, int maxHealth)
        {
            numAntPerBox = new List<int>();

            int totalUnits = maxHealth / 5;

            // Base units per number
            int baseUnits = totalUnits / totalBox;
            int remainder = totalUnits % totalBox;

            for (int i = 0; i < totalBox; i++)
            {
                int units = baseUnits;

                // Distribute remaining units
                if (i < remainder)
                    units++;

                numAntPerBox.Add(units * 5);
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

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

        public void Setup(List<BaseTile> tiles, BossAreaConfig bossAreaConfig)
        {
            coveredTiles = tiles;

            Vector3 averagePosition = Vector3.zero;

            for (int i = 0; i < coveredTiles.Count; i++)
            {
                BaseTile coveredTile = coveredTiles[i];

                if (coveredTile == null)
                {
                    continue;
                }

                coveredTile.isLocked = true;

                averagePosition += TileManager.Ins.GetTilePosition(coveredTile);
            }

            averagePosition /= coveredTiles.Count;
            averagePosition = averagePosition.ChangeZ(transform.position.z);

            transform.position = averagePosition;
            transform.localScale = GamePersistentVariable.tileDistance * new Vector2(bossAreaConfig.numCoverColumn, bossAreaConfig.numCoverRow);

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

            // damageText.ShowDamage(1, transform.position.MinusZALittle());
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

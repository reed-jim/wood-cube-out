using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

namespace AntsFlow
{
    public class SpawningTileBox : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer icon;
        [SerializeField] private SpriteRenderer top;
        [SerializeField] private TMP_Text numberText;

        public Color faction;
        public int totalNumber;
        public int numberLeft;

        public Dictionary<Color, int> spawnNumberOfFactions;

        private bool isAnimating;

        void Awake()
        {
            // BaseTile.antStartMoveToTileEvent += OnAntStartMoveToAnt;
            // BaseTile.tileMoveToAntEvent += OnTileMoveToAnt;
            BaseTile.tileCompleteEvent += OnTileCompleted;
        }

        void OnDestroy()
        {
            // BaseTile.antStartMoveToTileEvent -= OnAntStartMoveToAnt;
            // BaseTile.tileMoveToAntEvent -= OnTileMoveToAnt;
            BaseTile.tileCompleteEvent -= OnTileCompleted;
        }

        public void Setup(Color faction, int totalNumber)
        {
            this.faction = faction;
            this.totalNumber = totalNumber;

            numberLeft = totalNumber;

            icon.sprite = FactionMapper.Ins.GetTileSprite(faction);
            top.sprite = FactionMapper.Ins.GetSpawningTileBoxTop(faction);

            UpdateNumberText();
        }

        public void UpdateNumberText()
        {
            numberText.text = $"{numberLeft}";

            if (!isAnimating)
            {
                SaferioTween.Punch(numberText.transform, 1.1f, onCompleteAction: () =>
                {
                    isAnimating = false;
                });

                isAnimating = true;
            }
        }

        public bool NextSpawningTileBoxes()
        {
            if (spawnNumberOfFactions.Count == 0)
            {
                return false;
            }

            Vector2 screenSizeWorld = GamePersistentVariable.screenSizeWorld;

            KeyValuePair<Color, int> spawnNumberOfFaction = spawnNumberOfFactions.ElementAt(0);

            spawnNumberOfFactions.Remove(spawnNumberOfFaction.Key);

            gameObject.SetActive(true);

            transform.position = new Vector3(0.4f * screenSizeWorld.x, 0.38f * screenSizeWorld.y, -5);

            Setup(spawnNumberOfFaction.Key, spawnNumberOfFaction.Value);

            SaferioTween.ScaleUpWithPunch(transform, 1.1f);

            return true;
        }

        public void OnAntStartMoveToAnt(BaseTile tile)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (numberLeft <= 0)
            {
                return;
            }

            if (tile.Faction != faction)
            {
                return;
            }

            tile.Reset();

            numberLeft--;
        }

        public void SpawnNewTileAt(BaseTile tile)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (numberLeft <= 0)
            {
                return;
            }

            if (tile.Faction != faction)
            {
                return;
            }

            tile.isSpawningAgain = true;
            tile.Reset();
            SoundManager.Instance.PlaySound(GameConstants.spawnPopSound);


            FakeTile fakeTile = ObjectPoolingEverything.GetFromPool<FakeTile>(GameConstants.fakeTile);

            fakeTile.Init(tile);

            Vector3 tilePosition = TileManager.Ins.GetTilePosition(tile);

            Vector3 start = top.transform.position.ChangeZ(TileManager.Ins.tilemap.transform.position.z - 1);
            Vector3 end = tilePosition.ChangeZ(start.z);
            Vector3 control = (start + end) * 0.5f;
            control = control.ChangeY(start.y + 1.5f);

            fakeTile.transform.position = start;

            fakeTile.transform.DOPath(
                new Vector3[] { start, control, end },
                1f,
                PathType.CatmullRom
            ).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                tile.Show();

                ObjectPoolingEverything.ReturnToPool(GameConstants.fakeTile, fakeTile.gameObject);
            });

            numberLeft--;

            UpdateNumberText();

            if (numberLeft <= 0)
            {
                bool isNextBoxAvailable = NextSpawningTileBoxes();

                if (!isNextBoxAvailable)
                {
                    SaferioTween.ScaleDownWithPunch(transform);
                }
            }
        }

        public void OnTileCompleted(BaseTile tile)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            for (int i = 0; i < spawnNumberOfFactions.Count; i++)
            {
                KeyValuePair<Color, int> spawnNumberOfFaction = spawnNumberOfFactions.ElementAt(i);

                if (spawnNumberOfFaction.Key == tile.Faction)
                {
                    spawnNumberOfFactions[tile.Faction]--;

                    if (spawnNumberOfFactions[tile.Faction] <= 0)
                    {
                        spawnNumberOfFactions.Remove(tile.Faction);

                        break;
                    }
                }
            }
        }

        public void ClearSpawningTileBoxesOnUsingClearOneColorBooster(List<BaseTile> tilesToClear)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            for (int i = 0; i < tilesToClear.Count; i++)
            {
                BaseTile tileToClear = tilesToClear[i];

                if (tileToClear.isSpawningAgain)
                {
                    continue;
                }

                if (faction == tileToClear.Faction)
                {
                    totalNumber--;
                    numberLeft--;
                }

                for (int j = 0; j < spawnNumberOfFactions.Count; j++)
                {
                    KeyValuePair<Color, int> spawnNumberOfFaction = spawnNumberOfFactions.ElementAt(j);

                    if (spawnNumberOfFaction.Key == tileToClear.Faction)
                    {
                        spawnNumberOfFactions[tileToClear.Faction]--;

                        if (spawnNumberOfFactions[tileToClear.Faction] <= 0)
                        {
                            spawnNumberOfFactions.Remove(tileToClear.Faction);

                            break;
                        }
                    }
                }
            }

            if (numberLeft <= 0)
            {
                bool isNextBoxAvailable = NextSpawningTileBoxes();

                if (!isNextBoxAvailable)
                {
                    SaferioTween.ScaleDownWithPunch(transform);
                }
            }
            else
            {
                UpdateNumberText();
            }
        }
    }
}

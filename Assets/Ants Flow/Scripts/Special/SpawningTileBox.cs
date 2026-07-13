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

        private bool isAnimating;

        void Awake()
        {
            BaseTile.tileMoveToAntEvent += OnTileMoveToAnt;
        }

        void OnDestroy()
        {
            BaseTile.tileMoveToAntEvent -= OnTileMoveToAnt;
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

        public void OnTileMoveToAnt(BaseTile tile)
        {
            if (numberLeft <= 0)
            {
                return;
            }

            if (tile.Faction != faction)
            {
                return;
            }

            tile.isSpawningAgain = true;

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
                tile.Reset();
                tile.Show();

                ObjectPoolingEverything.ReturnToPool(GameConstants.fakeTile, fakeTile.gameObject);

                // float fakeTileScaleY = fakeTile.transform.localScale.y;

                // fakeTile.transform.DOScaleY(1.15f * fakeTileScaleY, duration: 0.2f);
                // fakeTile.transform.DOMoveY(end.y + 0.15f, 0.2f).OnComplete(() =>
                // {
                //     fakeTile.transform.DOScaleY(fakeTileScaleY, duration: 0.2f);
                //     fakeTile.transform.DOMoveY(end.y, 0.1f).OnComplete(() =>
                //     {
                //         tile.Reset();
                //         tile.Show();

                //         ObjectPoolingEverything.ReturnToPool(GameConstants.fakeTile, fakeTile.gameObject);
                //     });
                // });
            });

            numberLeft--;

            UpdateNumberText();

            if (numberLeft <= 0)
            {
                bool isNextBoxAvailable = SpecialElementManager.Ins.NextSpawningTileBoxes();

                if (!isNextBoxAvailable)
                {
                    SaferioTween.ScaleDownWithPunch(transform);
                }
            }
        }
    }
}

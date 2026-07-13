using DG.Tweening;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

namespace AntsFlow
{
    public class SpawningTileBox : MonoBehaviour
    {
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

            tile.isSpawningAgain = true;

            FakeTile fakeTile = ObjectPoolingEverything.GetFromPool<FakeTile>(GameConstants.fakeTile);

            fakeTile.Init(tile);

            Vector3 start = transform.position;
            Vector3 end = TileManager.Ins.GetTilePosition(tile).ChangeZ(TileManager.Ins.tilemap.transform.position.z - 1);
            start.z = end.z;
            Vector3 control = (start + end) * 0.5f + Vector3.up * 2f;

            fakeTile.transform.position = start;

            fakeTile.transform.DOPath(
                new Vector3[] { start, control, end },
                0.6f,
                PathType.CatmullRom
            ).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                tile.Reset();
                tile.Show();

                ObjectPoolingEverything.ReturnToPool(GameConstants.fakeTile, fakeTile.gameObject);
            });

            numberLeft--;

            UpdateNumberText();

            if (numberLeft <= 0)
            {
                SaferioTween.ScaleDownWithPunch(transform);
            }
        }
    }
}

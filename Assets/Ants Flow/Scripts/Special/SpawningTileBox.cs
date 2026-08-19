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

        private bool isNumberAnimating;
        private float numberTextInitialScale;

        // Các tile đang "bay" từ box về map (anim spawn-new-at 1s chưa xong). Trong khoảng đó tile đã bị
        // Hide() và đang chờ Show() ở OnComplete. Phải theo dõi để nếu box bị ForceComplete giữa chừng
        // (vd booster clear-one-color cùng màu) thì huỷ anim, tránh việc Show() dựng lại tile vừa bị clear
        // -> tile mồ côi kẹt trên map mãi mãi.
        private readonly List<PendingSpawn> pendingSpawns = new List<PendingSpawn>();
        private bool isForceCompleted;

        private class PendingSpawn
        {
            public BaseTile tile;
            public Tween tween;
            public FakeTile fakeTile;
        }

        void Awake()
        {
            BaseTile.tileMoveToAntEvent += OnTileMoveToAnt;
            numberTextInitialScale = numberText.transform.localScale.x;
        }

        void OnDestroy()
        {
            BaseTile.tileMoveToAntEvent -= OnTileMoveToAnt;
        }

        [System.NonSerialized] public List<Node> occupiedNodes = new List<Node>();

        public void Setup(Color faction, int totalNumber, List<Node> nodes = null)
        {
            this.faction = faction;
            this.totalNumber = totalNumber;
            this.numberLeft = totalNumber;
            this.occupiedNodes = nodes != null ? nodes : new List<Node>();
            isForceCompleted = false;

            icon.sprite = FactionMapper.Ins.GetSpawningBoxTileSprite(faction);
            top.sprite = FactionMapper.Ins.GetSpawningTileBoxTop(faction);

            UpdateNumberText();
        }

        public void UpdateNumberText()
        {
            numberText.text = $"{numberLeft}";

            if (!isNumberAnimating)
            {
                SaferioTween.Punch(numberText.transform, 1.2f, initScale: numberTextInitialScale, onCompleteAction: () =>
                {
                    isNumberAnimating = false;
                });

                isNumberAnimating = true;
            }
        }

        public void OnTileMoveToAnt(BaseTile tile)
        {
            if (!gameObject.activeSelf) return;
            if (isForceCompleted) return;
            if (numberLeft <= 0) return;
            if (tile.Faction != faction) return;

            SpawnNewTileAt(tile);
        }

        public void SpawnNewTileAt(BaseTile tile, float delay = 0f)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (isForceCompleted)
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

            // Chặn double-spawn: nếu tile này đang có 1 animation respawn treo (fakeTile đang bay), không
            // mở thêm animation thứ 2 cho cùng 1 tile.
            if (pendingSpawns.Any(p => p.tile == tile))
            {
                return;
            }

            tile.isSpawningAgain = true;
            tile.Hide();



            numberLeft--;

            UpdateNumberText();

            if (delay > 0f)
            {
                DOVirtual.DelayedCall(delay, () =>
                {
                    if (this != null && gameObject != null && gameObject.activeSelf && !isForceCompleted)
                    {
                        DoSpawnTileAnimation(tile);
                    }
                });
            }
            else
            {
                DoSpawnTileAnimation(tile);
            }

            if (numberLeft <= 0)
            {
                if (occupiedNodes != null)
                {
                    foreach (var node in occupiedNodes)
                    {
                        node.isObstacle = false;
                    }
                }
                SaferioTween.ScaleDownWithPunch(transform);
            }
        }

        public void ForceComplete()
        {
            isForceCompleted = true;
            numberLeft = 0;
            UpdateNumberText();


            CancelPendingSpawns();

            if (occupiedNodes != null)
            {
                foreach (var node in occupiedNodes)
                {
                    node.isObstacle = false;
                }
            }
            SaferioTween.ScaleDownWithPunch(transform);
        }


        private void CancelPendingSpawns()
        {
            for (int i = 0; i < pendingSpawns.Count; i++)
            {
                PendingSpawn pending = pendingSpawns[i];

                pending.tween?.Kill();

                if (pending.fakeTile != null)
                {
                    ObjectPoolingEverything.ReturnToPool(GameConstants.fakeTile, pending.fakeTile.gameObject);
                }

                if (pending.tile != null)
                {
                    pending.tile.isDone = true;
                    pending.tile.Hide();
                }
            }

            pendingSpawns.Clear();
        }

        private void DoSpawnTileAnimation(BaseTile tile)
        {
            SoundManager.Instance.PlaySound(GameConstants.spawnPopSound);

            FakeTile fakeTile = ObjectPoolingEverything.GetFromPool<FakeTile>(GameConstants.fakeTile);

            fakeTile.Init(tile);

            Vector3 tilePosition = TileManager.Ins.GetTilePosition(tile);

            Vector3 start = top.transform.position.ChangeZ(TileManager.Ins.tilemap.transform.position.z - 1);
            Vector3 end = tilePosition.ChangeZ(start.z);
            Vector3 control = (start + end) * 0.5f;
            control = control.ChangeY(start.y + 1.5f);

            fakeTile.transform.position = start;

            PendingSpawn pending = new PendingSpawn { tile = tile, fakeTile = fakeTile };
            pendingSpawns.Add(pending);

            pending.tween = fakeTile.transform.DOPath(
                new Vector3[] { start, control, end },
                1f,
                PathType.CatmullRom
            ).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                pendingSpawns.Remove(pending);

                // Box đã bị ForceComplete (booster clear-one-color) hoặc tile đã bị clear thì tuyệt đối
                // không Show() lại, nếu không sẽ tạo ra tile mồ côi kẹt trên map.
                if (isForceCompleted || tile.isDone)
                {
                    ObjectPoolingEverything.ReturnToPool(GameConstants.fakeTile, fakeTile.gameObject);

                    return;
                }

                tile.Reset();
                tile.Show();

                ObjectPoolingEverything.ReturnToPool(GameConstants.fakeTile, fakeTile.gameObject);

                if (WashingMachineManager.Ins != null && WashingMachineManager.Ins.washingMachineSlots != null)
                {
                    for (int i = 0; i < WashingMachineManager.Ins.washingMachineSlots.Count; i++)
                    {
                        if (WashingMachineManager.Ins.washingMachineSlots[i].washingMachineItem != null)
                        {
                            WashingMachineManager.Ins.washingMachineSlots[i].washingMachineItem.CheckAntsPath();
                        }
                    }
                }
            });
        }
    }
}

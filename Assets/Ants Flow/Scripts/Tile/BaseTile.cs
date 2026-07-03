using System;
using System.Collections.Generic;
using System.Linq;
using CandyCoded.HapticFeedback;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AntsFlow
{
    [Serializable]
    public class BaseTile
    {
        public Tilemap tilemap;
        public Vector3Int tilemapPosition;
        public TileObstacle tileObstacle;

        public int tileName;
        public Vector3 position;
        public bool isMovable;
        public bool isDone;
        public bool isAntApproaching;
        public bool isTempAntApproaching;
        public bool isAnimating;
        public bool isMovedToAnt;
        public bool isPrepareComplete;
        public bool isLocked;
        public bool isHidden;
        public int numStepToReach;
        [NonSerialized] public List<BaseTile> blockages;
        [NonSerialized] public HashSet<BaseTile> blockedByMe;
        [NonSerialized] public List<BaseTile> actualBlockages;
        [NonSerialized] public List<Node> path;

        public int ring;
        public int priority;

        [SerializeField] private Color faction;
        public float worldSize;

        public Color Faction
        {
            get => faction;
            set
            {
                // spriteRenderer.color = faction;

                faction = value;
            }
        }

        public bool IsMovable()
        {
            return path != null && path.Count > 0;
        }

        public float GetWorldSize()
        {
            // Vector2 spriteSize = outline.sprite.bounds.size;

            // Vector2 worldSize = new Vector2(
            //     spriteSize.x * outline.transform.lossyScale.x,
            //     spriteSize.y * outline.transform.lossyScale.y
            // );

            // return worldSize;

            return worldSize;
        }

        public float GetTotalPathDistance(Vector3 start)
        {
            if (path == null || path.Count == 0)
            {
                return 0;
            }

            List<Vector3> points = path.Select(item => item.initialPosition).ToList();

            float distance = 0;

            distance += Vector2.Distance(points[0], start);

            for (int i = 0; i < points.Count - 1; i++)
            {
                distance += Vector2.Distance(points[i], points[i + 1]);
            }

            return distance;
        }

        // public GameSetting gameSetting;

        public static event Action<BaseTile> removeTileAsBlockageEvent;
        public static event Action<BaseTile> tileCompleteEvent;
        public static event Action<Color> useClearOneColorBoosterEvent;
        public static event Action<BaseTile> antStartMoveToTileEvent;
        public static event Action<BaseTile> tileMoveToAntEvent;

        void Awake()
        {
            tileCompleteEvent += OnTileCompleted;
            antStartMoveToTileEvent += OnAntStartedMoveToTile;
            tileMoveToAntEvent += OnTileMoveToAnt;
            removeTileAsBlockageEvent += OnRemoveTileAsBlockage;

            // actualBlockages = new List<BaseTile>();
            // actualBlockages.AddRange(blockages);

            // Vector2 spriteSize = outline.sprite.bounds.size;

            // Vector2 worldSize = new Vector2(
            //     spriteSize.x * outline.transform.lossyScale.x,
            //     spriteSize.y * outline.transform.lossyScale.y
            // );

            // this.worldSize = worldSize.x;
        }

        void OnDestroy()
        {
            tileCompleteEvent -= OnTileCompleted;
            antStartMoveToTileEvent -= OnAntStartedMoveToTile;
            tileMoveToAntEvent -= OnTileMoveToAnt;
            removeTileAsBlockageEvent -= OnRemoveTileAsBlockage;
        }

        public void Init()
        {
            // spriteRenderer.sprite = FactionMapper.Ins.GetTileSprite(faction);
            // spriteRenderer.color = Color.white;
        }

        public void Reset()
        {
            isAntApproaching = false;
            isAnimating = false;

            actualBlockages.Clear();
            path.Clear();
        }

        public void SetNoLongerBlockages()
        {
            foreach (var tile in blockedByMe)
            {
                tile.blockages.Remove(this);

                if (tile.isPrepareComplete)
                {
                    tile.ActualComplete();
                }
            }
        }

        public void RevealHiddenTiles()
        {
            foreach (var tile in blockedByMe)
            {
                if (tile.isHidden && !tile.isMovedToAnt)
                {
                    TileManager.Ins.RevealTile(tile);

                    tile.isHidden = false;
                }
            }
        }

        public void ActualComplete()
        {
            foreach (var tile in blockedByMe)
            {
                tile.blockages.Remove(this);
            }

            isDone = true;
        }

        public void MoveToAnt(BaseAnt ant, Action onCompleteAction = null)
        {
            FakeTile fakeTile = ObjectPoolingEverything.GetFromPool<FakeTile>(GameConstants.fakeTile);

            fakeTile.Init(ant, faction);

            fakeTile.transform.position = TileManager.Ins.GetTilePosition(this);
            fakeTile.transform.localScale = GamePersistentVariable.tileMapScaleFactor * Vector3.one;
            fakeTile.transform.localRotation = Quaternion.identity;

            Tween.LocalPosition(fakeTile.transform, fakeTile.positionInAnt, duration: 0.3f);
            Tween.Scale(fakeTile.transform, fakeTile.scaleInAnt, duration: 0.3f);
            Tween.LocalRotation(fakeTile.transform, Quaternion.identity, duration: 0.3f)
            .OnComplete(() =>
            {
                onCompleteAction?.Invoke();
            });

            ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX);
            fx.transform.position = TileManager.Ins.GetTilePosition(this).AddZ(-9.1f);
            var main = fx.main;
            main.startColor = faction;
            fx.Play();

            TileRemainingManager.Ins.SpawnTileRemaining(this);
            SoundManager.Instance.PlaySoundCubeEat();

            if (GamePersistentVariable.IsVibrate)
            {
                HapticFeedback.LightFeedback();
            }

            isMovedToAnt = true;

            Hide();
            RevealHiddenTiles();

            tileMoveToAntEvent?.Invoke(this);
        }

        public void Hide()
        {
            tilemap.SetTile(tilemapPosition, null);
            TileManager.Ins.shadowTilemap.SetTile(tilemapPosition, null);
        }

        public void Complete()
        {
            isDone = true;

            tileCompleteEvent.Invoke(this);
        }

        public void HideObstacle()
        {
            if (tileObstacle != null)
            {
                tileObstacle.Hide();
                tileObstacle = null;
            }
        }

        public void InvokeAntStartMoveToTileEvent()
        {
            HideObstacle();

            // SetNoLongerBlockages();

            antStartMoveToTileEvent?.Invoke(this);
        }

        public void InvokeRemoveTileAsBlockageEvent()
        {
            removeTileAsBlockageEvent?.Invoke(this);
        }

        private void OnTileCompleted(BaseTile tile)
        {

        }

        private void OnRemoveTileAsBlockage(BaseTile tile)
        {
            blockages.Remove(tile);
        }

        private void OnAntStartedMoveToTile(BaseTile tile)
        {
            blockages.Remove(tile);

            // faster ant spawning
            // if (actualBlockages.Contains(tile))
            // {
            //     actualBlockages.Remove(tile);
            // }
        }

        private void OnTileMoveToAnt(BaseTile tile)
        {
            // more accurate but slower ant spawning
            // if (actualBlockages.Contains(tile))
            // {
            //     actualBlockages.Remove(tile);
            // }
        }

        public void ClearAllTilesOfTheSameColor()
        {
            useClearOneColorBoosterEvent?.Invoke(Faction);
        }

        public int GetNumBlockages()
        {
            return blockages.Where(tile => !tile.isDone && !tile.isAntApproaching).Count();
        }

        public bool IsBlocked()
        {
            return GetNumBlockages() >= 4;
        }
    }
}

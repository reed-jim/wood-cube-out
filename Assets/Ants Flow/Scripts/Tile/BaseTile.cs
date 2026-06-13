using System;
using System.Collections.Generic;
using CandyCoded.HapticFeedback;
using EPOOutline;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace AntsFlow
{
    public class BaseTile : MonoBehaviour
    {
        public Transform coordinate;
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer shadow;
        public SpriteRenderer outline;
        public BoxCollider2D boxCollider;
        public NavMeshModifierVolume navMeshModifierVolume;

        public bool isAntApproaching;
        public bool isAnimating;
        public bool isLocked;
        public List<BaseTile> blockages;
        public List<BaseTile> actualBlockages;

        public int ring;
        public int priority;

        [SerializeField] private Color faction;

        public Color Faction
        {
            get => faction;
            set
            {
                spriteRenderer.color = faction;

                faction = value;
            }
        }

        public Vector2 GetWorldSize()
        {
            Vector2 spriteSize = outline.sprite.bounds.size;

            Vector2 worldSize = new Vector2(
                spriteSize.x * outline.transform.lossyScale.x,
                spriteSize.y * outline.transform.lossyScale.y
            );

            return worldSize;
        }

        public GameSetting gameSetting;

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

            actualBlockages = new List<BaseTile>();
            actualBlockages.AddRange(blockages);

            Init();
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
            spriteRenderer.sprite = FactionMapper.Ins.GetTileSprite(faction);
            spriteRenderer.color = Color.white;
        }

        public void MoveToAnt(BaseAnt ant)
        {
            boxCollider.enabled = false;
            navMeshModifierVolume.enabled = false;
            outline.gameObject.SetActive(false);
            shadow.gameObject.SetActive(false);

            Tween.Color(outline, CommonUtil.GetHighContrastComplementary(spriteRenderer.color), duration: 0.3f);

            transform.SetParent(ant.transform);
            Tween.LocalPosition(transform, new Vector3(0, 0.6f, -1), duration: 0.3f);
            // Tween.Scale(transform, 0.45f, duration: 0.3f);
            Tween.LocalEulerAngles(transform, transform.localEulerAngles, Vector3.zero, duration: 0.3f);

            TileRemainingManager.Ins.SpawnTileRemaining(this);
            SoundManager.Instance.PlaySoundCubeEat();

            if (gameSetting.IsVibrate)
            {
                HapticFeedback.LightFeedback();
            }

            tileMoveToAntEvent?.Invoke(this);
        }

        public void Highlight(Action onComplete)
        {
            outline.gameObject.SetActive(true);

            outline.color = ColorUtil.WithAlpha(Color.white, 0);

            Tween.Alpha(outline, 0.75f, duration: 0.3f)
            .OnComplete(() =>
            {
                Tween.Alpha(outline, 0, duration: 0.5f)
                .OnComplete(() =>
                {
                    outline.gameObject.SetActive(false);

                    onComplete?.Invoke();
                });
            });
        }

        public void Complete()
        {
            gameObject.SetActive(false);

            // TileManager.Ins.UpdateNavMesh(boxCollider);

            tileCompleteEvent.Invoke(this);
        }

        public void InvokeAntStartMoveToTileEvent()
        {
            navMeshModifierVolume.enabled = false;

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
            if (actualBlockages.Contains(tile))
            {
                actualBlockages.Remove(tile);
            }
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
    }
}

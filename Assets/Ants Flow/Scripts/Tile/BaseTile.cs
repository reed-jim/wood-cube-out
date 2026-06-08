using System;
using System.Collections.Generic;
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
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer shadow;
        public SpriteRenderer outline;
        public BoxCollider2D boxCollider;
        public NavMeshModifierVolume navMeshModifierVolume;

        public bool isAntApproaching;
        public bool isLocked;
        public List<BaseTile> blockages;

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

        public static event Action<BaseTile> tileCompleteEvent;
        public static event Action<Color> useClearOneColorBoosterEvent;
        public static event Action<BaseTile> antStartMoveToTileEvent;

        void Awake()
        {
            tileCompleteEvent += OnTileCompleted;
            antStartMoveToTileEvent += OnAntStartedMoveToTile;

            Init();
        }

        void OnDestroy()
        {
            tileCompleteEvent -= OnTileCompleted;
            antStartMoveToTileEvent -= OnAntStartedMoveToTile;
        }

        public void Init()
        {
            faction = spriteRenderer.color;

            spriteRenderer.sprite = FactionMapper.Ins.GetTileSprite(Faction);
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
            Tween.LocalPosition(transform, new Vector3(0, 0.64f, -1), duration: 0.3f);
            // Tween.Scale(transform, 0.45f, duration: 0.3f);
            Tween.LocalEulerAngles(transform, transform.localEulerAngles, Vector3.zero, duration: 0.3f);

            SoundManager.Instance.PlaySoundCubeEat();
        }

        public void Complete()
        {
            gameObject.SetActive(false);

            // TileManager.Ins.UpdateNavMesh(boxCollider);

            tileCompleteEvent.Invoke(this);
        }

        public void InvokeAntStartMoveToTileEvent()
        {
            antStartMoveToTileEvent?.Invoke(this);
        }

        private void OnTileCompleted(BaseTile tile)
        {

        }

        private void OnAntStartedMoveToTile(BaseTile tile)
        {
            blockages.Remove(tile);
        }

        public void ClearAllTilesOfTheSameColor()
        {
            useClearOneColorBoosterEvent?.Invoke(Faction);
        }
    }
}

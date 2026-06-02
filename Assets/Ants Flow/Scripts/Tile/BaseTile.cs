using System;
using System.Collections.Generic;
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
        public BoxCollider2D boxCollider;
        [SerializeField] private SpriteRenderer border;
        [SerializeField] private NavMeshObstacle navMeshObstacle;

        public bool isAntApproaching;
        public bool isLocked;
        public List<BaseTile> blockages;

        public Color Faction => spriteRenderer.color;

        public static event Action<BaseTile> tileCompleteEvent;

        void Awake()
        {
            tileCompleteEvent += OnTileCompleted;
        }

        void OnDestroy()
        {
            tileCompleteEvent -= OnTileCompleted;
        }

        public void MoveToAnt(BaseAnt ant)
        {
            boxCollider.enabled = false;

            border.color = CommonUtil.GetHighContrastComplementary(spriteRenderer.color);
            SaferioTween.AlphaShow(border);

            transform.SetParent(ant.transform);
            Tween.LocalPosition(transform, new Vector3(0, 0.7f, -1), duration: 0.3f);
            Tween.Scale(transform, 0.45f, duration: 0.3f);
            Tween.LocalEulerAngles(transform, transform.localEulerAngles, Vector3.zero, duration: 0.3f);

            SoundManager.Instance.PlaySoundCubeEat();
        }

        public void Complete()
        {
            gameObject.SetActive(false);

            // TileManager.Ins.UpdateNavMesh(boxCollider);

            tileCompleteEvent.Invoke(this);
        }

        private void OnTileCompleted(BaseTile tile)
        {
            blockages.Remove(tile);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace AntsFlow
{
    public class TileManager : Singleton<TileManager>
    {
        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private AstarPath astarPath;

        [SerializeField] private TMP_Text tileCountText;

        public List<BaseTile> tiles;

        public Transform antNest;

        private bool isUpdatingNavMesh;
        private Coroutine delayedUpdateNavmeshCoroutine;

        public static event Action winLevelEvent;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            BaseTile.tileCompleteEvent += OnTileCompleted;
        }

        void OnDestroy()
        {
            BaseTile.tileCompleteEvent -= OnTileCompleted;
        }

        public BaseTile GetAvailableTile(Color faction)
        {
            bool isAvailable = tiles.Any(
                i => i.gameObject.activeInHierarchy &&
                i.blockages.Count < 4 &&
                i.Faction == faction &&
                !i.isAntApproaching
                && !i.isLocked
            );

            if (isAvailable)
            {
                BaseTile availableTile =
                    tiles.First(i => i.gameObject.activeInHierarchy && i.blockages.Count < 4 && i.Faction == faction && !i.isAntApproaching && !i.isLocked);

                return availableTile;
            }
            else
            {
                return null;
            }
        }

        private void OnTileCompleted(BaseTile tile)
        {
            // UpdateNavMesh(tile);
            UpdateTileCount();
            CheckWinLevel();
        }

        public void UpdateNavMesh(BaseTile tile)
        {
            if (isUpdatingNavMesh)
            {
                if (delayedUpdateNavmeshCoroutine != null)
                {
                    StopCoroutine(delayedUpdateNavmeshCoroutine);
                }

                delayedUpdateNavmeshCoroutine = StartCoroutine(DelayedUpdateNavmesh());

                return;
            }

            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

            isUpdatingNavMesh = true;

            Tween.Delay(2).OnComplete(() =>
            {
                isUpdatingNavMesh = false;
            });
        }

        private IEnumerator DelayedUpdateNavmesh()
        {
            yield return new WaitForSeconds(2);

            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        }

        public void UpdateTileCount()
        {
            int tileLeft = tiles.Count(item => item.gameObject.activeInHierarchy);

            tileCountText.text = $"{tileLeft}/{tiles.Count()}";
        }

        private void CheckWinLevel()
        {
            bool isWon = !tiles.Any(tile => tile.gameObject.activeInHierarchy);

            if (isWon)
            {
                winLevelEvent?.Invoke();
            }
        }
    }
}

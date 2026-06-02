using System;
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
            );

            if (isAvailable)
            {
                BaseTile availableTile =
                    tiles.First(i => i.gameObject.activeInHierarchy && i.blockages.Count < 4 && i.Faction == faction && !i.isAntApproaching);

                return availableTile;
            }
            else
            {
                return null;
            }
        }

        private void OnTileCompleted(BaseTile tile)
        {
            UpdateNavMesh(tile);
            UpdateTileCount();
            CheckWinLevel();
        }

        public void UpdateNavMesh(BaseTile tile)
        {
            // navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

            // var sources = new List<NavMeshBuildSource>();

            // NavMeshBuilder.CollectSources(
            //     navMeshSurface.transform,
            //     LayerMask.GetMask("Default"),
            //     NavMeshCollectGeometry.PhysicsColliders,
            //     0,
            //     new List<NavMeshBuildMarkup>(),
            //     sources
            // );

            // NavMeshBuildSettings navMeshBuildSettings = NavMesh.GetSettingsByID(0);
            // navMeshBuildSettings.preserveTilesOutsideBounds = true;

            // NavMeshBuilder.UpdateNavMeshData(navMeshSurface.navMeshData, navMeshBuildSettings, sources, tile.boxCollider.bounds);

            // astarPath.UpdateGraphs(collider.bounds);

            if (isUpdatingNavMesh)
            {
                return;
            }

            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

            isUpdatingNavMesh = true;

            Tween.Delay(5).OnComplete(() =>
            {
                isUpdatingNavMesh = false;
            });
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

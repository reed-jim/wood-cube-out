using System;
using System.Collections.Generic;
using System.Linq;
using NavMeshPlus.Components;
using TMPro;
using UnityEngine;

namespace AntsFlow
{
    public class TileManager : Singleton<TileManager>
    {
        [SerializeField] private NavMeshSurface navMeshSurface;

        [SerializeField] private TMP_Text tileCountText;

        public List<BaseTile> tiles;

        public Transform antNest;

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
            UpdateNavMesh();
            UpdateTileCount();
            CheckWinLevel();
        }

        private void UpdateNavMesh()
        {
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

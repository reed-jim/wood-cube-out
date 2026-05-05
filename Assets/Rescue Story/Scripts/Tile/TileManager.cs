using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace RescueStory
{
    public class TileManager : Singleton<TileManager>
    {
        [SerializeField] private List<BaseTile> tiles;
        [SerializeField] private Grid[] grids;

        [SerializeField] private TileIconContainer tileIconContainer;

        private float highestYGridPosition;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            SpawnTiles();
        }

        private void DropTileDown(BaseTile movedTile)
        {
            Grid selectedTileGrid = grids.First(item => item.tile == movedTile);
            List<Grid> affectedGrids =
                grids.Where(item => item.IsSameColumn(selectedTileGrid) && item.topBound > selectedTileGrid.topBound).ToList();

            affectedGrids.Add(selectedTileGrid);
            selectedTileGrid.tile = null;
            affectedGrids = affectedGrids.OrderBy(item => item.bottomBound).ToList();

            for (int i = 0; i < affectedGrids.Count; i++)
            {
                if (affectedGrids[i].tile == null && i + 1 < affectedGrids.Count)
                {
                    Tween.LocalPositionY(affectedGrids[i + 1].tile.transform, affectedGrids[i].position.y, duration: 0.3f);

                    affectedGrids[i].tile = affectedGrids[i + 1].tile;
                    affectedGrids[i + 1].tile = null;
                }
            }

            SpawnTilesForEmptyGrid();
        }

        private void SpawnTilesForEmptyGrid()
        {
            for (int i = 0; i < grids.Length; i++)
            {
                if (grids[i].tile == null)
                {
                    // BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);

                    // Vector3 spawnPos = grids[i].position.ChangeY(highestYGridPosition + 200);

                    // tile.transform.SetParent(tileContainer);
                    // tile.transform.localPosition = spawnPos;
                    // tile.Init(ItemCategory.Ball, tileIconContainer.categoryIconSets[0].icons[0]);
                    // // tile.Init(categoryIconSet.itemCategory, categoryIconSet.icons[categoryIconIndex]);

                    // Tween.LocalPositionY(tile.transform, grids[i].position.y, duration: 0.3f);

                    // grids[i].tile = tile;
                }
            }
        }

        private void SpawnTiles()
        {
            int col = 5;
            int row = 5;
            float tileSpacing = 0;

            float totalWidth = 0;
            float totalHeight = 0;

            int currentCategoryIndex = 0;
            int categoryIconIndex = tileIconContainer.categoryIconSets[currentCategoryIndex].icons.Length;

            grids = new Grid[col * row];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    int index = j + i * col;

                    BaseTile spawnedTile = SpawnTile(j, i);
                }
            }

            BaseTile SpawnTile(int xIndex, int yIndex)
            {
                if (categoryIconIndex >= tileIconContainer.categoryIconSets[currentCategoryIndex].icons.Length)
                {
                    currentCategoryIndex = UnityEngine.Random.Range(0, tileIconContainer.categoryIconSets.Length);
                    categoryIconIndex = 0;
                }

                CategoryIconSet categoryIconSet = tileIconContainer.categoryIconSets[currentCategoryIndex];

                BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);

                if (tileSpacing == 0)
                {
                    tileSpacing = 1.1f * tile.Size;
                    totalWidth = (col - 1) * tileSpacing;
                    totalHeight = (row - 1) * tileSpacing;
                }

                float xPos = xIndex * tileSpacing - totalWidth / 2f;
                float yPos = yIndex * tileSpacing - totalHeight / 2f;

                Vector3 spawnPos = new Vector3(xPos, yPos, 0);

                tile.transform.localPosition = spawnPos;
                // tile.Init(categoryIconSet.itemCategory, categoryIconSet.icons[categoryIconIndex]);

                categoryIconIndex++;

                tiles.Add(tile);

                return tile;
            }
        }
    }

    [Serializable]
    public class Grid
    {
        public float leftBound;
        public float rightBound;
        public float topBound;
        public float bottomBound;
        public Vector3 position;
        public BaseTile tile;

        public bool IsSameColumn(Grid other)
        {
            return Mathf.Approximately(leftBound, other.leftBound) && Mathf.Approximately(rightBound, other.rightBound);
        }
    }
}

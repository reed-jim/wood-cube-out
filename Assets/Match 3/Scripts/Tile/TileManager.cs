using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class TileManager : Singleton<TileManager>
    {
        int width = 5;
        int height = 5;

        public List<BaseTile> tiles;

        public void CheckMerge()
        {

        }

        List<int> FindAllMatches()
        {
            HashSet<int> matched = new HashSet<int>();

            // Horizontal
            for (int y = 0; y < height; y++)
            {
                int matchCount = 1;

                for (int x = 1; x < width; x++)
                {
                    BaseTile current = GetTile(x, y);
                    BaseTile previous = GetTile(x - 1, y);

                    if (current != null && previous != null && current.faction == previous.faction)
                    {

                    }

                    if (current != null &&
                        previous != null &&
                        current.faction == previous.faction)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int i = 0; i < matchCount; i++)
                                matched.Add(GetIndex(x - 1 - i, y));
                        }

                        matchCount = 1;
                    }
                }

                // Edge case
                if (matchCount >= 3)
                {
                    for (int i = 0; i < matchCount; i++)
                        matched.Add(GetIndex(width - 1 - i, y));
                }
            }

            // Vertical
            for (int x = 0; x < width; x++)
            {
                int matchCount = 1;

                for (int y = 1; y < height; y++)
                {
                    BaseTile current = GetTile(x, y);
                    BaseTile previous = GetTile(x, y - 1);

                    if (current != null &&
                        previous != null &&
                        current.faction == previous.faction)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int i = 0; i < matchCount; i++)
                                matched.Add(GetIndex(x, y - 1 - i));
                        }

                        matchCount = 1;
                    }
                }

                // Edge case
                if (matchCount >= 3)
                {
                    for (int i = 0; i < matchCount; i++)
                        matched.Add(GetIndex(x, height - 1 - i));
                }
            }

            return new List<int>(matched);
        }

        int GetIndex(int x, int y)
        {
            return y * width + x;
        }

        BaseTile GetTile(int x, int y)
        {
            return tiles[GetIndex(x, y)];
        }

        Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y, 0);
        }

        BaseTile SpawnRandomTile(int x, int y)
        {
            // Replace with your own spawning system
            GameObject go = new GameObject($"Tile_{x}_{y}");

            BaseTile tile = go.AddComponent<BaseTile>();

            string[] types = { "Red", "Blue", "Green", "Yellow" };
            // tile.faction = types[Random.Range(0, types.Length)];

            go.transform.position = GetWorldPosition(x, y);

            return tile;
        }
    }
}

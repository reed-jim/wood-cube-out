using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AntsFlow
{
    public class TileManager : Singleton<TileManager>
    {
        public List<BaseTile> tiles;

        public Transform antNest;

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
    }
}

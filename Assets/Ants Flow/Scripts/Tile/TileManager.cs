using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AntsFlow
{
    public class TileManager : Singleton<TileManager>
    {
        public List<BaseTile> tiles;

        public BaseTile GetAvailableTile(Color faction)
        {
            BaseTile availableTile = tiles.First(i => i.gameObject.activeInHierarchy && i.Faction == faction && !i.isAntApproaching);

            availableTile.isAntApproaching = false;

            return availableTile;
        }
    }
}

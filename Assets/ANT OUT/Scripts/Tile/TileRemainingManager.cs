using UnityEngine;

namespace AntsFlow
{
    public class TileRemainingManager : Singleton<TileRemainingManager>
    {
        public void SpawnTileRemaining(BaseTile tile)
        {
            TileRemaining tileRemaining = ObjectPoolingEverything.GetFromPool<TileRemaining>(GameConstants.tileRemaining);

            tileRemaining.Spawn(tile);
        }
    }
}

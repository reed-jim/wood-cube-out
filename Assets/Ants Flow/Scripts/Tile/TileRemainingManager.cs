using UnityEngine;

namespace AntsFlow
{
    public class TileRemainingManager : Singleton<TileRemainingManager>
    {
        // Scene-specific - khong DontDestroyOnLoad
        protected override bool ShouldPersist() => false;

        public void SpawnTileRemaining(BaseTile tile)
        {
            TileRemaining tileRemaining = ObjectPoolingEverything.GetFromPool<TileRemaining>(GameConstants.tileRemaining);

            tileRemaining.Spawn(tile);
        }
    }
}

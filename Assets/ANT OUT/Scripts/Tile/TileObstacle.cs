using UnityEngine;

namespace AntsFlow
{
    public class TileObstacle : MonoBehaviour
    {
        void Awake()
        {
            SettingPopup.replayLevelEvent += OnLevelReplayed;
            LosePopup.replayLevelEvent += OnLevelReplayed;
            DebugPopup.switchLevelEvent += OnLevelReplayed;
        }

        void OnDestroy()
        {
            SettingPopup.replayLevelEvent -= OnLevelReplayed;
            LosePopup.replayLevelEvent -= OnLevelReplayed;
            DebugPopup.switchLevelEvent -= OnLevelReplayed;
        }

        public void Init(BaseTile tile)
        {
            transform.position = TileManager.Ins.GetTilePosition(tile);
            gameObject.SetActive(true);
            // transform.position = tile.position;
            transform.localScale = GamePersistentVariable.obstacleScaleFactor * Vector3.one;

            tile.tileObstacle = this;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnLevelReplayed()
        {
            Hide();
        }
    }
}

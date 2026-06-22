using System.Collections.Generic;
using AntsFlow;
using UnityEngine;

namespace AntsFlow
{
    public class TileObstacleManager : Singleton<TileObstacleManager>
    {
        public List<TileObstacle> tileObstacles;

        int currentIndex;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            SettingPopup.replayLevelEvent += OnLevelReplayed;
            LosePopup.replayLevelEvent += OnLevelReplayed;
            DebugPopup.switchLevelEvent += OnLevelReplayed;
            WashingMachineManager.winLevelEvent += OnLevelReplayed;

            Init();
        }

        void OnDestroy()
        {
            SettingPopup.replayLevelEvent -= OnLevelReplayed;
            LosePopup.replayLevelEvent -= OnLevelReplayed;
            DebugPopup.switchLevelEvent -= OnLevelReplayed;
            WashingMachineManager.winLevelEvent -= OnLevelReplayed;
        }

        public void Init()
        {
            tileObstacles = TransformUtil.GetComponentsFromAllChildren<TileObstacle>(transform);

            for (int i = 0; i < tileObstacles.Count; i++)
            {
                tileObstacles[i].gameObject.SetActive(false);
            }
        }

        public void SpawnObstacleAt(BaseTile tile)
        {
            if (currentIndex >= tileObstacles.Count)
            {
                currentIndex = 0;
            }

            tileObstacles[currentIndex].Init(tile);

            currentIndex++;
        }

        private void OnLevelReplayed()
        {
            currentIndex = 0;
        }
    }
}

using AntsFlow;
using NamPhuThuy.GamePlay;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class FakeTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Color faction;

        public Vector3 positionInAnt;
        public float scaleInAnt;

        void Awake()
        {
            LevelLoader.startLevelEvent += OnLevelReplayed;
            SettingPopup.replayLevelEvent += OnLevelReplayed;
            LosePopup.replayLevelEvent += OnLevelReplayed;
            DebugPopup.switchLevelEvent += OnLevelReplayed;
        }

        void OnDestroy()
        {
            LevelLoader.startLevelEvent += OnLevelReplayed;
            SettingPopup.replayLevelEvent += OnLevelReplayed;
            LosePopup.replayLevelEvent += OnLevelReplayed;
            DebugPopup.switchLevelEvent += OnLevelReplayed;
        }

        public void Init(BaseAnt ant, Color faction)
        {
            spriteRenderer.sprite = FactionMapper.Ins.GetTileSprite(faction);

            transform.transform.SetParent(ant.transform);
            transform.localPosition = positionInAnt;
            transform.localScale = scaleInAnt * Vector3.one;
            transform.localRotation = Quaternion.identity;

            ant.fakeTile = this;
        }

        public void Init(BaseTile tile)
        {
            spriteRenderer.sprite = FactionMapper.Ins.GetTileSprite(tile.Faction);

            Vector3 position = (Vector3)tile.tilemapPosition;

            transform.position = TileManager.Ins.GetTilePosition(tile).AddZ(-0.0001f * (100 - position.y) - 6);
            transform.localScale = GamePersistentVariable.tileMapScaleFactor * Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        private void OnLevelReplayed()
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.washingMachine, gameObject);
        }
    }
}

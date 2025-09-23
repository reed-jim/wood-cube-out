using System.Linq;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

namespace WoodCubeOut
{
    public class LockScrewColorLevelManager : Singleton<LockScrewColorLevelManager>
    {
        [SerializeField] private GameFaction[] _currentLockedFaction;
        public GameFaction[] CurrentLockedFaction => _currentLockedFaction;

        private int _screwLeftToUnlock;

        [Header("UI")]
        [SerializeField] private RectTransform container;
        [SerializeField] private Image[] lockedColorImages;
        [SerializeField] private TMP_Text[] conditionTexts;

        protected override void MoreActionOnAwake()
        {
            LevelLoader.startLevelEvent += OnLevelLoaded;
            BaseScrew.screwLoosenedEvent += OnScrewLoosed;
        }

        private void OnDestroy()
        {
            LevelLoader.startLevelEvent -= OnLevelLoaded;
            BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
        }

        private void LockRandomColor()
        {
            _currentLockedFaction = new GameFaction[1];

            _currentLockedFaction[0] = CommonUtil.RandomizeFaction();

            _screwLeftToUnlock = Random.Range(5, 10);

            lockedColorImages[0].color = FactionUtility.GetColorForFaction(_currentLockedFaction[0]);

            conditionTexts[0].text = $"Unscrew {_screwLeftToUnlock} screws to unlock";
        }

        public bool IsFactionLocked(GameFaction faction)
        {
            if (_currentLockedFaction == null)
            {
                return false;
            }

            return _currentLockedFaction.Contains(faction);
        }

        #region EVENT
        private void OnLevelLoaded()
        {
            LockRandomColor();
        }

        private void OnScrewLoosed()
        {
            _screwLeftToUnlock--;

            conditionTexts[0].text = $"Unscrew {_screwLeftToUnlock} screws to unlock";

            if (_screwLeftToUnlock == 0)
            {
                Tween.Scale(container, 1.1f, duration: 0.3f).OnComplete(() =>
                {
                    Tween.Scale(container, 0f, duration: 0.3f);
                });

                _currentLockedFaction = null;
            }
        }
        #endregion
    }
}

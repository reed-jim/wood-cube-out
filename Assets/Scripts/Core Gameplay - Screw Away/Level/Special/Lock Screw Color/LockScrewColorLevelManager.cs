using System.Collections;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

namespace WoodCubeOut
{
    public class LockScrewColorLevelManager : BaseSpecialLevelManager
    {
        [SerializeField] private GameFaction[] _currentLockedFaction;
        public GameFaction[] CurrentLockedFaction => _currentLockedFaction;

        private BaseScrew[] _screws;
        private int _requiredScrewToUnlock;
        private int _screwLeftToUnlock;
        private Coroutine _delayedLockNextScrew;

        [Header("UI")]
        [SerializeField] private RectTransform container;
        [SerializeField] private CanvasGroup lockedGroup;
        [SerializeField] private Image[] lockedColorImages;
        [SerializeField] private TMP_Text[] conditionTexts;
        [SerializeField] private TMP_Text timeCountingText;
        [SerializeField] private Image tickIcon;

        #region Life Cycle
        protected override void Awake()
        {
            LevelLoader.startLevelEvent += OnLevelLoaded;
            GameStateLose.loseLevelEvent += OnLevelLost;
            ScrewManager.setScrewsEvent += OnScrewsSet;
            BaseScrew.screwLoosenedEvent += OnScrewLoosed;

            container.gameObject.SetActive(false);
            timeCountingText.gameObject.SetActive(false);
            tickIcon.gameObject.SetActive(false);

            _isFirstTime = true;
        }

        private void OnDestroy()
        {
            LevelLoader.startLevelEvent -= OnLevelLoaded;
            GameStateLose.loseLevelEvent -= OnLevelLost;
            ScrewManager.setScrewsEvent -= OnScrewsSet;
            BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
        }
        #endregion

        #region Lock
        protected virtual IEnumerator DelayedLockNextScrew()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1);

            int remaningTime = 10;

            if (_isFirstTime)
            {
                yield return new WaitForSeconds(3);

                ShowUI();

                LockSingleColor();

                _isFirstTime = false;

                yield break;
            }

            while (remaningTime > 0)
            {
                remaningTime--;

                timeCountingText.text = $"Lock in {remaningTime}s";

                yield return waitForSeconds;
            }

            foreach (var text in conditionTexts)
            {
                SaferioTween.ScaleUpWithPunch(text.transform, 1, delay: 0.3f);
            }

            SaferioTween.ScaleUpWithPunch(lockedGroup.transform, 1, delay: 0.3f);

            SaferioTween.ScaleDownWithPunch(tickIcon.transform);
            SaferioTween.ScaleDownWithPunch(timeCountingText.transform);

            LockSingleColor();
        }

        private void LockSingleColor()
        {
            _currentLockedFaction = new GameFaction[1];

            BaseScrew[] validScrews = _screws.Where(item => !item.IsDone && !item.IsInScrewPort).ToArray();
            BaseScrew targetScrew = CommonUtil.GetRandomValueFromArray(validScrews);

            if (targetScrew == null)
            {
                return;
            }

            _currentLockedFaction[0] = targetScrew.Faction;

            _requiredScrewToUnlock = Random.Range(5, 10);
            _screwLeftToUnlock = _requiredScrewToUnlock;

            lockedColorImages[0].color = FactionUtility.GetColorForFaction(_currentLockedFaction[0]);

            conditionTexts[0].text = $"{_screwLeftToUnlock}/{_requiredScrewToUnlock}";
        }

        private void LockRandomColor()
        {
            _currentLockedFaction = new GameFaction[1];

            _currentLockedFaction[0] = CommonUtil.RandomizeFaction();

            _requiredScrewToUnlock = Random.Range(5, 10);
            _screwLeftToUnlock = _requiredScrewToUnlock;

            lockedColorImages[0].color = FactionUtility.GetColorForFaction(_currentLockedFaction[0]);

            conditionTexts[0].text = $"{_screwLeftToUnlock}/{_requiredScrewToUnlock}";
        }

        public bool IsFactionLocked(GameFaction faction)
        {
            if (_currentLockedFaction == null)
            {
                return false;
            }

            return _currentLockedFaction.Contains(faction);
        }
        #endregion

        #region UI
        private void ShowUI()
        {
            SaferioTween.ScaleUpWithPunch(container, 1);
        }
        #endregion

        #region EVENT
        private void OnLevelLoaded()
        {
            // LockSingleColor();
        }

        protected override void OnLevelLost()
        {
            if (_delayedLockNextScrew != null)
            {
                StopCoroutine(_delayedLockNextScrew);
            }
        }

        protected override void OnScrewsSet(BaseScrew[] screws)
        {
            _screws = screws;

            StartCoroutine(DelayedLockNextScrew());
        }

        protected override void OnScrewLoosed()
        {
            _screwLeftToUnlock--;

            conditionTexts[0].text = $"{_screwLeftToUnlock}/{_requiredScrewToUnlock}";

            if (_screwLeftToUnlock == 0)
            {
                foreach (var text in conditionTexts)
                {
                    SaferioTween.ScaleDownWithPunch(text.transform);
                }

                SaferioTween.ScaleDownWithPunch(lockedGroup.transform);

                SaferioTween.ScaleUpWithPunch(tickIcon.transform, 1, delay: 0.3f);
                SaferioTween.ScaleUpWithPunch(timeCountingText.transform, 1, delay: 0.3f);

                _currentLockedFaction = null;

                _delayedLockNextScrew = StartCoroutine(DelayedLockNextScrew());
            }
        }
        #endregion
    }
}

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static GameEnum;

namespace NamPhuThuy.GamePlay
{
    public class BaseSpecialLevelManager : MonoBehaviour
    {
        [SerializeField] public LevelMode levelMode;

        protected BaseScrew[] _screws;
        protected Coroutine _delayedLockNextScrew;

        protected SpecialLevelTaskRecord _specialLevelTaskRecord;
        protected bool _isFirstTime;

        public static event Action<LevelMode, SpecialLevelUIState, object[]> updateUI;

        #region Life Cycle

        protected virtual void Awake()
        {
            GameStateLose.loseLevelEvent += OnLevelLost;
            ScrewManager.setScrewsEvent += OnScrewsSet;
            ScrewManager.switchScrewsToSpecialTypeCompleteEvent += OnSwitchScrewsToSpecialTypeCompleted;
            BaseScrew.screwLoosenedEvent += OnScrewLoosed;

            _isFirstTime = true;
        }

        private void OnDestroy()
        {
            GameStateLose.loseLevelEvent -= OnLevelLost;
            ScrewManager.setScrewsEvent -= OnScrewsSet;
            ScrewManager.switchScrewsToSpecialTypeCompleteEvent -= OnSwitchScrewsToSpecialTypeCompleted;
            BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
        }

        #endregion

        public virtual void Run()
        {
            Debug.Log("SAFERIO run" + name + "/" + levelMode);
            UpdateUI(SpecialLevelUIState.Setup, new object[1] { levelMode });
        }

        protected virtual void LockSingleColor()
        {
            BaseScrew[] validScrews = _screws.Where(item => !item.IsDone && !item.IsInScrewPort).ToArray();
            BaseScrew targetScrew = CommonUtil.GetRandomValueFromArray(validScrews);

            if (targetScrew == null)
            {
                return;
            }

            _specialLevelTaskRecord = SpecialLevelTaskRecord.GenerateTask(SpecialLevelTaskType.ScrewComplete);

            _specialLevelTaskRecord.requiredComponents[0].faction = targetScrew.Faction;

            UpdateUI(SpecialLevelUIState.UpdateProgress, new object[1] { _specialLevelTaskRecord });

            // Update screw shader
            BaseScrew[] lockedScrew = validScrews.Where(item => item.Faction == targetScrew.Faction).ToArray();

            foreach (var item in lockedScrew)
            {
                item.ScrewServiceLocator.screwMaterialPropertyBlock.SetScrewCondition(ScrewCondition.Rusty);
            }
        }

        protected virtual IEnumerator DelayedLockNextScrew()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1);

            int remaningTime = 10;

            if (_isFirstTime)
            {
                yield return new WaitForSeconds(3);

                UpdateUI(SpecialLevelUIState.Showing);

                LockSingleColor();

                _isFirstTime = false;

                yield break;
            }

            while (remaningTime > 0)
            {
                remaningTime--;

                UpdateUI(SpecialLevelUIState.UpdateTimeCount, new object[] { remaningTime });

                yield return waitForSeconds;
            }

            UpdateUI(SpecialLevelUIState.ReActive);

            LockSingleColor();
        }

        public virtual bool IsFactionLocked(GameFaction faction)
        {
            if (_specialLevelTaskRecord == null)
            {
                return false;
            }

            return _specialLevelTaskRecord.requiredComponents
                        .Any(item => item.componentType == GameComponentType.Screw && item.faction == faction);
        }

        protected virtual void OnLevelLost()
        {

        }

        protected virtual void OnScrewLoosed(BaseScrew screw)
        {
            if (_specialLevelTaskRecord == null)
            {
                return;
            }

            if (_specialLevelTaskRecord.taskType == SpecialLevelTaskType.ScrewComplete)
            {
                SpecialLevelTaskComponentAmount item =
                    _specialLevelTaskRecord.requiredComponents.First(item => item.componentType == GameComponentType.Screw);

                item.remainingAmount--;

                UpdateUI(SpecialLevelUIState.UpdateProgress, new object[1] { _specialLevelTaskRecord });

                if (_specialLevelTaskRecord.IsTaskCompleted())
                {
                    _specialLevelTaskRecord = null;

                    _delayedLockNextScrew = StartCoroutine(DelayedLockNextScrew());
                }
            }
        }

        protected virtual void OnScrewsSet(BaseScrew[] screws)
        {
            // if (!gameObject.activeInHierarchy)
            // {
            //     return;
            // }

            // _screws = screws;

            // StartCoroutine(DelayedLockNextScrew());
        }

        protected virtual void OnSwitchScrewsToSpecialTypeCompleted(BaseScrew[] screws)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            _screws = screws;

            StartCoroutine(DelayedLockNextScrew());
        }

        protected void UpdateUI(SpecialLevelUIState state, object[] parameters = null)
        {
            if (gameObject.activeInHierarchy)
            {
                updateUI?.Invoke(levelMode, state, parameters);
            }
        }
    }
}

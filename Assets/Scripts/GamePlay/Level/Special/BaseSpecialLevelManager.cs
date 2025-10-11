using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
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
        protected bool _isDoneCompletely;

        public static event Action<LevelMode, SpecialLevelUIState, object[]> updateUI;
        

        #region Life Cycle

        protected virtual void Awake()
        {
            GameStateLose.loseLevelEvent += OnLevelLost;
            ScrewManager.setScrewsEvent += OnScrewsSet;
            ScrewManager.switchScrewsToSpecialTypeCompleteEvent += OnSwitchScrewsToSpecialTypeCompleted;
            BaseScrew.screwLoosenedEvent += OnScrewLoosed;
            ScrewManager.winLevelEvent += Disable;
            LevelLoader.endCurrentLevelEvent += Disable;

            _isFirstTime = true;
        }

        protected virtual void OnDestroy()
        {
            GameStateLose.loseLevelEvent -= OnLevelLost;
            ScrewManager.setScrewsEvent -= OnScrewsSet;
            ScrewManager.switchScrewsToSpecialTypeCompleteEvent -= OnSwitchScrewsToSpecialTypeCompleted;
            BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
            ScrewManager.winLevelEvent -= Disable;
            LevelLoader.endCurrentLevelEvent -= Disable;
        }

        protected virtual void Reset()
        {
            _isFirstTime = true;
            _isDoneCompletely = false;
        }

        #endregion

        public virtual void Run()
        {
            UpdateUI(SpecialLevelUIState.Setup, new object[2] { levelMode, _specialLevelTaskRecord });

            Reset();
        }

        public virtual void Disable()
        {
            if (gameObject.activeInHierarchy)
            {
                UpdateUI(SpecialLevelUIState.Hide);

                gameObject.SetActive(false);
            }
        }

        protected virtual void LockSingleColor()
        {
            BaseScrew[] validScrews = _screws.Where(item => !item.IsDone && !item.IsInScrewPort).ToArray();

            if (_isDoneCompletely)
            {
                return;
            }
            else
            {
                if (validScrews.Length < 0.35f * _screws.Length)
                {
                    HideSpecialTask();
                    return;
                }
            }

            BaseScrew targetScrew = CommonUtil.GetRandomValueFromArray(validScrews);

            if (targetScrew == null)
            {
                return;
            }

            _specialLevelTaskRecord = SpecialLevelTaskRecord.GenerateTask(SpecialLevelTaskType.ScrewComplete);
            DebugLogger.LogSimple(message:$"generated new record {_specialLevelTaskRecord}");

            _specialLevelTaskRecord.requiredComponents[0].faction = targetScrew.Faction;
            _specialLevelTaskRecord.requiredComponents[0].Amount = UnityEngine.Random.Range(3, 9);

            UpdateUI(SpecialLevelUIState.UpdateProgress, new object[1] { _specialLevelTaskRecord });

            void HideSpecialTask()
            {
                UpdateUI(SpecialLevelUIState.Hide);
                _isDoneCompletely = true;
            }
        }

        protected virtual IEnumerator DelayedLockNextScrew()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1);

            int remaningTime = 25;

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

        #region CALLBACK
        protected virtual void OnLevelLost()
        {

        }

        protected virtual void OnScrewLoosed(BaseScrew screw)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (_specialLevelTaskRecord == null)
            {
                return;
            }

            if (screw.IsInScrewPort)
            {
                return;
            }

            if (_specialLevelTaskRecord.taskType == SpecialLevelTaskType.ScrewComplete)
            {
                SpecialLevelTaskComponentAmount item =
                    _specialLevelTaskRecord.requiredComponents.First(item => item.componentType == GameComponentType.Screw);

                item.remainingAmount--;
                SoundManager.Instance.PlaySoundGenerateTask();

                UpdateUI(SpecialLevelUIState.UpdateProgress, new object[1] { _specialLevelTaskRecord });

                if (_specialLevelTaskRecord.IsTaskCompleted())
                {
                    SoundManager.Instance.PlaySoundCompleteTask();
                    HandleTaskCompleted();
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

        protected virtual void HandleTaskCompleted()
        {
            _specialLevelTaskRecord = null;

            BaseScrew[] validScrews = _screws.Where(item => !item.IsDone && !item.IsInScrewPort).ToArray();

            if (validScrews.Length < 0.3f * _screws.Length)
            {
                UpdateUI(SpecialLevelUIState.Hide);

                _isDoneCompletely = true;

                return;
            }

            _delayedLockNextScrew = StartCoroutine(DelayedLockNextScrew());
        }
        #endregion

        protected void UpdateUI(SpecialLevelUIState state, object[] parameters = null)
        {
            if (gameObject.activeInHierarchy)
            {
                updateUI?.Invoke(levelMode, state, parameters);
            }
        }
    }
}

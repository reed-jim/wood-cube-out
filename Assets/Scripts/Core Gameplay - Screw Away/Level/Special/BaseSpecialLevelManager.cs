using UnityEngine;
using static GameEnum;

namespace NamPhuThuy.GamePlay
{
    public class BaseSpecialLevelManager : MonoBehaviour
    {
        protected bool _isFirstTime;

        #region Life Cycle

        protected virtual void Awake()
        {
            GameStateLose.loseLevelEvent += OnLevelLost;
            ScrewManager.setScrewsEvent += OnScrewsSet;
            BaseScrew.screwLoosenedEvent += OnScrewLoosed;

            _isFirstTime = true;
        }

        private void OnDestroy()
        {
            GameStateLose.loseLevelEvent -= OnLevelLost;
            ScrewManager.setScrewsEvent -= OnScrewsSet;
            BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
        }

        #endregion

        public virtual void Run()
        {

        }

        public bool IsFactionLocked(GameFaction faction)
        {
            return false;
        }

        protected virtual void OnLevelLost()
        {

        }

        protected virtual void OnScrewLoosed()
        {

        }

        protected virtual void OnScrewsSet(BaseScrew[] screws)
        {

        }
    }

    public enum SpecialLevelUIState
    {
        None = 0,
        Setup = 1,
        Showing = 2,
        ReActive = 3,
        UpdateProgress = 4,
        UpdateTimeCount = 5,
    }
}
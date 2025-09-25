using UnityEngine;

public class BaseSpecialLevelManager : Singleton<BaseSpecialLevelManager>
{
    protected bool _isFirstTime;

    #region Life Cycle
    protected override void MoreActionOnAwake()
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

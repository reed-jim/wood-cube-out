using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.GamePlay;
using UnityEngine;
using UnityEngine.EventSystems;


public class UpSpeedAnt : Singleton<UpSpeedAnt>
{
    public int speedUpMultiplier = 1;
    public int nearEndLevelSpeedUpMultiplier = 1;

    private bool isHolding;
    private float timeHold;

    public GameSetting gameSetting;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        LevelLoader.startLevelEvent += OnLevelStarted;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
    }

    // void Update()
    // {
    //     if (Input.GetMouseButton(0) && !isHolding && !GamePersistentVariable.isGameSpeedChangeInDebug)
    //     {
    //         if (timeHold < 0.5f)
    //         {
    //             timeHold += Time.unscaledDeltaTime;
    //         }
    //         else
    //         {
    //             Time.timeScale = 2;

    //             isHolding = true;
    //         }
    //     }

    //     if (Input.GetMouseButtonUp(0) && isHolding)
    //     {
    //         if (!GamePersistentVariable.isGameSpeedChangeInDebug)
    //         {
    //             Time.timeScale = 1;
    //         }

    //         isHolding = false;
    //         timeHold = 0;
    //     }
    // }

    // public void OnMouseDown()
    // {
    //     EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_START_UP_SPEED_ANT, 2f);
    // }

    // public void OnMouseUp()
    // {
    //     EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_END_UP_SPEED_ANT);
    // }

    private bool IsClickedOnUI()
    {
        bool res;
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count > 0)
        {
            res = true;
        }
        else
        {
            res = false;
        }

        return res;
    }

    public void ChangeGameSpeed()
    {
        int totalSpeedUpMultiplier = speedUpMultiplier * nearEndLevelSpeedUpMultiplier;

        totalSpeedUpMultiplier = Mathf.Min(totalSpeedUpMultiplier, 3);

        Time.timeScale = totalSpeedUpMultiplier;
    }

    public int SwitchGameSpeed()
    {
        if (speedUpMultiplier == 1)
        {
            speedUpMultiplier = 2;
        }
        else
        {
            speedUpMultiplier = 1;
        }

        ChangeGameSpeed();

        return speedUpMultiplier;
    }

    public void UpSpeedNearEndLevel()
    {
        GameplayScreen.Ins.HideUINearWinLevel(onCompleteAction: () =>
        {
            nearEndLevelSpeedUpMultiplier = 3;

            ChangeGameSpeed();
        });
    }

    public void ResetSpeedOnWinLevel()
    {
        speedUpMultiplier = 1;
        nearEndLevelSpeedUpMultiplier = 1;

        GameplayScreen.Ins.UpdateUpSpeedState(speedUpMultiplier);

        ChangeGameSpeed();
    }

    private void OnLevelStarted()
    {
        speedUpMultiplier = 1;
        nearEndLevelSpeedUpMultiplier = 1;

        ChangeGameSpeed();

        GameplayScreen.Ins.UpdateUpSpeedState(speedUpMultiplier);
    }
}

using System.Collections;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class GameTimeManager : Singleton<GameTimeManager>
{
    private int _remainingSecond;
    private bool _isFreezed;
    private Coroutine _countTimeCoroutine;

    public void StartCountingTime()
    {
        if (_countTimeCoroutine != null)
        {
            StopCoroutine(_countTimeCoroutine);
        }

        _countTimeCoroutine = StartCoroutine(CountTime());
    }

    private IEnumerator CountTime()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        _remainingSecond = 3 * 60;

        GameplayUI.Instance.UpdateRemainingTime(_remainingSecond);

        while (_remainingSecond >= 0)
        {
            if (!_isFreezed)
            {
                _remainingSecond--;

                GameplayUI.Instance.UpdateRemainingTime(_remainingSecond);
            }

            yield return waitForSeconds;
        }
    }

    // public void FreezeTime(bool isFreezed)
    // {
    //     _isFreezed = isFreezed;

    //     if (isFreezed)
    //     {
    //         Tween.Delay(10).OnComplete(() =>
    //         {

    //         })
    //     }
    // }

    public void AddMoreTime(int addedSeconds)
    {
        _remainingSecond += addedSeconds;

        GameplayUI.Instance.UpdateRemainingTime(_remainingSecond);
        GameplayUI.Instance.AnimateRemainingTimeText();
    }
}

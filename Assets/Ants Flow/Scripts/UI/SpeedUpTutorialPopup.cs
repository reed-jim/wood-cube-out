using System;
using UnityEngine;

public class SpeedUpTutorialPopup : BasePopup
{
    private bool isHolding;
    private float timeHold;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (timeHold < 0.5f)
            {
                timeHold += Time.unscaledDeltaTime;
            }
            else
            {
                Hide();
            }
        }
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction: () =>
        {
            Time.timeScale = 0;
        });
    }
}

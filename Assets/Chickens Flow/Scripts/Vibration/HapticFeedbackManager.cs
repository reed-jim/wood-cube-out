using CandyCoded.HapticFeedback;
using UnityEngine;

public class HapticFeedbackManager : Singleton<HapticFeedbackManager>
{
    public GameSetting gameSetting;

    public void MediumFeedback()
    {
        if(gameSetting.IsVibrate)
        {
            HapticFeedback.MediumFeedback();
        }
    }
}

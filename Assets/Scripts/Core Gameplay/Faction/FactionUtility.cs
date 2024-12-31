using UnityEngine;
using static GameEnum;

public static class FactionUtility
{
    public static Color GetColorForFaction(GameFaction faction)
    {
        if (faction == GameFaction.Red)
        {
            return GameConstants.SAFERIO_RED;
        }
        else if (faction == GameFaction.Green)
        {
            return GameConstants.SAFERIO_GREEN;
        }
        else if (faction == GameFaction.Orange)
        {
            return GameConstants.SAFERIO_ORANGE;
        }
        else if (faction == GameFaction.Purple)
        {
            return GameConstants.SAFERIO_PURPLE;
        }
        else if (faction == GameFaction.Blue)
        {
            return GameConstants.SAFERIO_BLUE;
        }
        else if (faction == GameFaction.Yellow)
        {
            return GameConstants.SAFERIO_YELLLOW;
        }
        else if (faction == GameFaction.LightBlue)
        {
            return GameConstants.SAFERIO_LIGHT_BLUE;
        }
        else if (faction == GameFaction.Pink)
        {
            return GameConstants.SAFERIO_PINK;
        }

        return GameConstants.SAFERIO_RED;
    }
}

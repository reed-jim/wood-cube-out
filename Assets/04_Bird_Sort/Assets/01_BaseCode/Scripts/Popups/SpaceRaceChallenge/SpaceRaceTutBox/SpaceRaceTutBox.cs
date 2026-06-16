using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRaceTutBox : BaseBox
{
    private static SpaceRaceTutBox instance;

    public static SpaceRaceTutBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SpaceRaceTutBox>(PathPrefabs.SPACE_RACE_TUT_BOX));
        }

        return instance;
    }
}

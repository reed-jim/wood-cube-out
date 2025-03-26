using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum TypeMiniGame
{
    ConnectBird = 1,
    TripleBird = 2,
    //....
}

public class MiniGameController : MonoBehaviour
{
    public static int Version_Except = 0;
    public ConnectBirdMGController connectBirdMG;
    public TripleBirdMGController tripleBirdMG;
    public int testConfig;
    //....
    public void Init()
    {
        testConfig = 1;
        var currentMiniGame = RemoteConfigController.GetIntConfig(StringHelper.CURRENT_ID_MINI_GAME, testConfig);
        switch (currentMiniGame)
        {
            case (int)TypeMiniGame.ConnectBird:
                connectBirdMG.Init();
                break;
            case (int)TypeMiniGame.TripleBird:
                tripleBirdMG.Init();
                break;
        }

    }

    public void ResetDay()
    {

        int currentMiniGame = RemoteConfigController.GetIntConfig(StringHelper.CURRENT_ID_MINI_GAME, testConfig);

        switch (currentMiniGame)
        {
            case (int)TypeMiniGame.ConnectBird:
                connectBirdMG.ResetDay();
                break;
            case (int)TypeMiniGame.TripleBird:
                tripleBirdMG.ResetDay();
                break;
        }
    }

    public MiniGameRegister GetControllerMG()
    {
        int currentMiniGame = RemoteConfigController.GetIntConfig(StringHelper.CURRENT_ID_MINI_GAME, testConfig);

        switch (currentMiniGame)
        {
            case (int)TypeMiniGame.ConnectBird:
                return connectBirdMG;
            case (int)TypeMiniGame.TripleBird:
                return tripleBirdMG;
        }

        return null;
    }

    public void OpenMiniGameBox()
    {
        int currentMiniGame = RemoteConfigController.GetIntConfig(StringHelper.CURRENT_ID_MINI_GAME, testConfig);

        switch (currentMiniGame)
        {
            case (int)TypeMiniGame.ConnectBird:
                connectBirdMG.OpenBox();
                break;
            case (int)TypeMiniGame.TripleBird:
                tripleBirdMG.OpenBox();
                break;
        }
    }

    public void OpenMiniGameBox(TypeMiniGame typeMiniGame)
    {
        switch (typeMiniGame)
        {
            case TypeMiniGame.ConnectBird:
                connectBirdMG.OpenBox();
                break;
            case TypeMiniGame.TripleBird:
                tripleBirdMG.OpenBox();
                break;
        }
    }

}

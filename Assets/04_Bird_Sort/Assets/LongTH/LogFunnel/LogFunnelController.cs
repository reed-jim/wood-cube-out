using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogFunnelController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void LogFunnel(string funnel, string checkpoint, int piority, int level)
    {
        if (PlayerPrefs.HasKey($"LogFunnel_{funnel}") && PlayerPrefs.GetInt($"LogFunnel_{funnel}") >= piority) return;
        try
        {
            Debug.Log($"LogFunnel {funnel} {checkpoint} {piority}");
            new FFunnelLog(funnel, checkpoint, piority, level).Send();
            PlayerPrefs.SetInt($"LogFunnel_{funnel}", piority);
        }
        catch
        {
        }
    }

    public static int GetFunnelPiority(string funnel)
    {
        return PlayerPrefs.GetInt($"LogFunnel_{funnel}", 0);
    }
}

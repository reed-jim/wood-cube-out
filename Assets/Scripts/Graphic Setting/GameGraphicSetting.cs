using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameGraphicSetting : MonoBehaviour
{
    public UniversalRenderPipelineAsset urpAsset;

    void Awake()
    {
        SettingGraphic();
    }

    private void SettingGraphic()
    {
        string deviceModel = SystemInfo.deviceModel;

        urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

        if (deviceModel.ContainsInsensitive("OPPO") || deviceModel.ContainsInsensitive("CPH"))
        {
            urpAsset.msaaSampleCount = 0;
        }
        else
        {
            urpAsset.msaaSampleCount = 8;
        }
    }
}

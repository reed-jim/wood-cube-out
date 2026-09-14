using UnityEngine;
using UnityEngine.UI;

public class NodeGroupUI : MonoBehaviour
{
    public NodeGroupServiceLocator nodeGroupServiceLocator;

    [Header("UI")]
    public Button selectButton;

    private void Select()
    {
        if (IdleGameInputManager.inputMode == IdleGameInputMode.SelectNodeGroup)
        {

        }
    }
}

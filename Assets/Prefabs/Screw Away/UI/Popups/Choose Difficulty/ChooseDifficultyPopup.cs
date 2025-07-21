using UnityEngine;
using UnityEngine.UI;

public class ChooseDifficultyPopup : BasePopup
{
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button refuseButton;

    private void Accept()
    {
        GamePersistentVariable.bannerCooldown
    }

    private void Refuse()
    {
        
    }
}

using UnityEngine;
using static GameEnum;

public class PassengerFaction : MonoBehaviour
{
    [SerializeField] private CharacterMaterialPropertyBlock characterMaterialPropertyBlock;

    [SerializeField] private GameFaction _faction;

    public GameFaction Faction
    {
        get => _faction;
    }

    private void Awake()
    {
        PassengerQueue.setPassengerFactionEvent += SetFaction;
    }

    private void OnDestroy()
    {
        PassengerQueue.setPassengerFactionEvent -= SetFaction;
    }

    private void SetFaction(int instanceId, GameFaction faction)
    {
        if (instanceId != gameObject.GetInstanceID())
        {
            return;
        }

        if (faction == GameFaction.Red)
        {
            characterMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_RED);
        }
        else if (faction == GameFaction.Green)
        {
            characterMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_GREEN);
        }
        else if (faction == GameFaction.Orange)
        {
            characterMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_ORANGE);
        }
        else if (faction == GameFaction.Purple)
        {
            characterMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_PURPLE);
        }
        else if (faction == GameFaction.Blue)
        {
            characterMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_BLUE);
        }

        _faction = faction;
    }
}

using System;
using UnityEngine;
using static GameEnum;

public class VehicleFaction : MonoBehaviour
{
    [SerializeField] private VehicleServiceLocator vehicleServiceLocator;
    [SerializeField] private VehicleMaterialPropertyBlock vehicleMaterialPropertyBlock;

    [SerializeField] private GameFaction _faction;

    public GameFaction Faction
    {
        get => _faction;
    }

    public static event Action<GameFaction, int> vehicleFactionSetEvent;

    private void Start()
    {
        SetFaction(_faction);
    }

    public void SetRandomFaction()
    {
        SetFaction((GameFaction)UnityEngine.Random.Range(0, 4));
    }

    private void SetFaction(GameFaction faction)
    {
        if (faction == GameFaction.Red)
        {
            vehicleMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_RED);
        }
        else if (faction == GameFaction.Green)
        {
            vehicleMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_GREEN);
        }
        else if (faction == GameFaction.Orange)
        {
            vehicleMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_ORANGE);
        }
        else if (faction == GameFaction.Purple)
        {
            vehicleMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_PURPLE);
        }
        else if (faction == GameFaction.Blue)
        {
            vehicleMaterialPropertyBlock.SetColor(GameConstants.SAFERIO_BLUE);
        }

        _faction = faction;

        vehicleFactionSetEvent?.Invoke(faction, vehicleServiceLocator.Vehicle.NumberSeat);
    }
}

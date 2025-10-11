using UnityEngine;
using static GameEnum;

public class PieceProperty : MonoBehaviour
{
    [SerializeField] private PieceServiceLocator pieceServiceLocator;

    [SerializeField] private GameFaction faction;

    public GameFaction Faction
    {
        get => faction;
        set => faction = value;
    }

    void Awake()
    {
        pieceServiceLocator = GetComponent<PieceServiceLocator>();
        
        faction = (GameFaction)Random.Range(0, 8);

        pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);
    }

    public void SetColorByFaction()
    {
        pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);
    }
}

using UnityEngine;
using static GameEnum;

public class PieceProperty : MonoBehaviour
{
    [SerializeField] private PieceServiceLocator pieceServiceLocator;

    [SerializeField] private GameFaction faction;

    private Vector3 _boundSize;

    public GameFaction Faction
    {
        get => faction;
        set => faction = value;
    }

    public Vector3 BoundSize
    {
        get => _boundSize;
    }

    void Awake()
    {
        pieceServiceLocator = GetComponent<PieceServiceLocator>();

        _boundSize = pieceServiceLocator.pieceMeshRenderer.bounds.size;
    }

    void Start()
    {
        faction = (GameFaction)Random.Range(0, 8);

        SetColorByFaction();
    }

    public void SetColorByFaction()
    {
        pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);
    }
}

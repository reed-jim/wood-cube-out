using UnityEngine;
using static GameEnum;

public class PieceProperty : MonoBehaviour
{
    [SerializeField] public PieceServiceLocator pieceServiceLocator;

    [SerializeField] private GameFaction faction;
    private GameFaction[] poolFactions;

    private bool _isLocked;
    private Vector3 _boundSize;

    #region PROPERTY
    public GameFaction Faction
    {
        get => faction;
        set => faction = value;
    }

    public GameFaction[] PoolFactions
    {
        get => poolFactions;
        set => poolFactions = value;
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => _isLocked = value;
    }

    public Vector3 BoundSize
    {
        get => _boundSize;
    }
    #endregion

    void Awake()
    {
        pieceServiceLocator = GetComponent<PieceServiceLocator>();

        _boundSize = pieceServiceLocator.pieceMeshRenderer.bounds.size;
    }

    // void Start()
    // {
    //     faction = (GameFaction)Random.Range(0, 8);

    //     SetColorByFaction();
    // }

    void OnValidate()
    {
        pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);
    }

    public void SetColorByFaction()
    {
        pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);
    }
}

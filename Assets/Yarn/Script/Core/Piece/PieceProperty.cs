using UnityEngine;
using static GameEnum;

public class PieceProperty : MonoBehaviour
{
    [SerializeField] public PieceServiceLocator pieceServiceLocator;

    [SerializeField] private GameFaction faction;
    private GameFaction[] poolFactions;

    private bool _isLocked;
    private bool _isRewinding;
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

    public bool IsRewinding
    {
        get => _isRewinding;
        set => _isRewinding = value;
    }

    public Vector3 BoundSize
    {
        get => _boundSize;
    }

    public Vector3 MeshBoundSize
    {
        get => pieceServiceLocator.pieceMeshFilter.mesh.bounds.size;
    }

    public Vector3 MeshMin
    {
        get
        {
            return pieceServiceLocator.pieceMeshFilter.mesh.bounds.min;
        }
    }

    public Vector3 MeshMax
    {
        get
        {
            return pieceServiceLocator.pieceMeshFilter.mesh.bounds.max;
        }
    }

    public float MaxBoundSize
    {
        get
        {
            float max = MeshBoundSize.x;

            if (MeshBoundSize.y > MeshBoundSize.x)
            {
                max = MeshBoundSize.y;
            }

            if (MeshBoundSize.z > MeshBoundSize.y)
            {
                max = MeshBoundSize.z;
            }

            return max;
        }
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
        try
        {
            pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);
        }
        catch (System.Exception)
        {

        }
    }

    public void SetColorByFaction()
    {
        pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(faction);
    }
}

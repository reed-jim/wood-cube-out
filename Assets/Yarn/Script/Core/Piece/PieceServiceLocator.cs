using UnityEngine;

public class PieceServiceLocator : MonoBehaviour
{
    [SerializeField] public BasePiece piece;
    [SerializeField] public PieceProperty pieceProperty;
    [SerializeField] public PieceMaterialPropertyBlock pieceMaterialPropertyBlock;
    // [SerializeField] public ScrewOutline screwOutline;
    [SerializeField] public MeshFilter pieceMeshFilter;
    [SerializeField] public MeshRenderer pieceMeshRenderer;
    [SerializeField] public Collider pieceCollider;
    public Collider[] allColliders;

    void Awake()
    {
        piece = GetComponent<BasePiece>();
        pieceProperty = GetComponent<PieceProperty>();
        pieceMaterialPropertyBlock = GetComponent<PieceMaterialPropertyBlock>();
        pieceMeshFilter = GetComponent<MeshFilter>();
        pieceMeshRenderer = GetComponent<MeshRenderer>();
        pieceCollider = GetComponent<Collider>();
    }

    public void AssignComponents()
    {
        piece = GetComponent<BasePiece>();
        pieceProperty = GetComponent<PieceProperty>();
        pieceMaterialPropertyBlock = GetComponent<PieceMaterialPropertyBlock>();
        pieceMeshFilter = GetComponent<MeshFilter>();
        pieceMeshRenderer = GetComponent<MeshRenderer>();
        pieceCollider = GetComponent<Collider>();

        pieceProperty.pieceServiceLocator = this;

        pieceMaterialPropertyBlock.Renderer = GetComponent<Renderer>();
    }
}

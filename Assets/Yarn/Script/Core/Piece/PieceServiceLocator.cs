using UnityEngine;

public class PieceServiceLocator : MonoBehaviour
{
    [SerializeField] public BasePiece piece;
    [SerializeField] public PieceProperty pieceProperty;
    [SerializeField] public PieceMaterialPropertyBlock pieceMaterialPropertyBlock;
    // [SerializeField] public ScrewOutline screwOutline;
    [SerializeField] public MeshRenderer pieceMeshRenderer;

    void Awake()
    {
        piece = GetComponent<BasePiece>();
        pieceProperty = GetComponent<PieceProperty>();
        pieceMaterialPropertyBlock = GetComponent<PieceMaterialPropertyBlock>();
        pieceMeshRenderer = GetComponent<MeshRenderer>();
    }
}

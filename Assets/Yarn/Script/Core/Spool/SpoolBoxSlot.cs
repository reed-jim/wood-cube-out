using System;
using UnityEngine;
using static GameEnum;

public class SpoolBoxSlot : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private bool isAlternative;

    private BasePiece _piece;
    private bool _isFilled;

    public BasePiece Piece
    {
        get => _piece;
        set => _piece = value;
    }

    public bool IsFilled
    {
        get => _isFilled;
        set => _isFilled = value;
    }

    public bool IsAlternative
    {
        get => isAlternative;
        set => isAlternative = value;
    }

    #region EVENT
    public static event Action<int> spoolBoxCompleteEvent;
    #endregion

    public void Fill(BasePiece piece)
    {
        _piece = piece;

        _isFilled = true;
    }

    public void CompleteFill()
    {
        if (transform.GetSiblingIndex() == 2)
        {
            spoolBoxCompleteEvent?.Invoke(transform.parent.gameObject.GetInstanceID());
        }
    }
}

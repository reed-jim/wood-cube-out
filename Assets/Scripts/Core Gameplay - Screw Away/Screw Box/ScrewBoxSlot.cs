using System;
using BirdSort2;
using UnityEngine;
using static GameEnum;

public class ScrewBoxSlot : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private bool isScrewPort;

    private BaseScrew _screw;
    private bool _isFilled;

    public BaseScrew Screw
    {
        get => _screw;
        set => _screw = value;
    }

    public bool IsFilled
    {
        get => _isFilled;
        set => _isFilled = value;
    }

    public bool IsScrewPort
    {
        get => isScrewPort;
        set => isScrewPort = value;
    }
    
    public void Fill(BaseScrew screw)
    {
        _screw = screw;

        _isFilled = true;
    }

    public void CompleteFill()
    {
        if (transform.GetSiblingIndex() == 2)
        {
            
            foreach (var i in GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs)
            {
                if (i != null) i.OnScrewBoxCompleted((transform.parent.gameObject.GetInstanceID()));
            }
        }
    }
}

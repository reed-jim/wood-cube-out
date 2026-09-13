using UnityEngine;

public enum IdleGameInputMode
{
    Normal,
    SelectNode,
    SelectNodeGroup
}

public class IdleGameInputManager : Singleton<IdleGameInputManager>
{
    public static IdleGameInputMode inputMode;
}

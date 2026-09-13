using UnityEngine;

public enum IdleGameInputMode
{
    Normal,
    SelectNode
}

public class IdleGameInputManager : Singleton<IdleGameInputManager>
{
    public static IdleGameInputMode inputMode;
}

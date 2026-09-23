using UnityEngine;

public enum IdleGameInputMode
{
    Normal,
    SelectNode,
    SelectNodeGroup,
    RemoveLeadershipCharacter
}

public class IdleGameInputManager : Singleton<IdleGameInputManager>
{
    public static IdleGameInputMode inputMode;
}

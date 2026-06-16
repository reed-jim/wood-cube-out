public class GameplayAction
{
    public GAMEPLAY_ACTION_TYPE gameplayActionType;
    public string description;

    public GameplayAction(GAMEPLAY_ACTION_TYPE gameplayActionType, string description)
    {
        this.gameplayActionType = gameplayActionType;
        this.description = description;
    }
}

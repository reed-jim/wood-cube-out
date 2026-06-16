namespace Falcon.FalconCore.Scripts.Services.GameObjs
{
    public interface IPioneerService
    {
        /// <summary>
        /// Called before all other FMainObj.OnGameContinue.
        /// Called in main thread.
        /// </summary>
        void OnPreContinue();
    }
}
using MoreMountains.Tools;

namespace NamPhuThuy.Tutorial
{
    public struct ETutorialActivate
    {
        public readonly int LevelId;
        public ETutorialActivate(int levelId) { LevelId = levelId; }
    }

    public struct ETutorialStepStarted
    {
        public readonly int LevelId;
        public readonly TutorialStepRecord Step;
        public readonly int StepIndex;

        public ETutorialStepStarted(int levelId, TutorialStepRecord step, int stepIndex)
        {
            LevelId = levelId;
            Step = step;
            StepIndex = stepIndex;
        }
    }

    public struct ETutorialStepCompleted
    {
        public readonly int LevelId;
        public readonly TutorialStepRecord Step;
        public readonly int StepIndex;

        public ETutorialStepCompleted(int levelId, TutorialStepRecord step, int stepIndex)
        {
            LevelId = levelId;
            Step = step;
            StepIndex = stepIndex;
        }
    }

    public struct ETutorialFinished
    {
        public readonly int LevelId;
        public ETutorialFinished(int levelId) { LevelId = levelId; }
    }
}
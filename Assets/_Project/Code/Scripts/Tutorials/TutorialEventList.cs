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
        public readonly TutorialStepData Step;
        public readonly int StepIndex;

        public ETutorialStepStarted(int levelId, TutorialStepData step, int stepIndex)
        {
            LevelId = levelId;
            Step = step;
            StepIndex = stepIndex;
        }
    }

    public struct ETutorialStepCompleted
    {
        public readonly int LevelId;
        public readonly TutorialStepData Step;
        public readonly int StepIndex;

        public ETutorialStepCompleted(int levelId, TutorialStepData step, int stepIndex)
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
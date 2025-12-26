using System;

namespace F1Manager.Core.RnD
{
    [Serializable]
    public struct ActiveResearchState
    {
        public string teamId;
        public string projectId;

        public int weeksRemaining;
        public float budgetCommitted;
        public float accumulatedRisk;

        public bool IsActive =>
            weeksRemaining > 0 &&
            !string.IsNullOrEmpty(teamId) &&
            !string.IsNullOrEmpty(projectId);
    }
}

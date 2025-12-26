using System;
using UnityEngine;

namespace F1Manager.Core.Calendar
{
    [Serializable]
    public class CalendarFormatRules
    {
        [Header("Rounds Options")]
        public int[] allowedRounds = new[] { 10, 16, 24 };

        [Header("Spacing")]
        [Min(3)] public int minDaysBetweenRounds = 7;
        [Min(3)] public int maxDaysBetweenRounds = 21;

        [Header("Sprint")]
        public bool allowSprint = true;

        // chance base de sprint num round (se pista não for “fixa sprint”)
        [Range(0f, 1f)] public float sprintChance = 0.25f;
    }
}

using System;
using F1Manager.Core.Season;

namespace F1Manager.Core.Save
{
    [Serializable]
    public class SaveGameData
    {
        public int version = 1;
        public SeasonState seasonState;
    }
}

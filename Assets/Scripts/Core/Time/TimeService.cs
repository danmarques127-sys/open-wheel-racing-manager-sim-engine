using System;
using F1Manager.Core.Manufacturing;
using F1Manager.Core.RnD;
using F1Manager.Core.Save;

namespace F1Manager.Core.Time
{
    public class TimeService
    {
        private readonly SaveData save;
        private readonly RnDService rnd;
        private readonly ManufacturingService mfg;

        public event Action<int> OnWeekAdvanced;

        public TimeService(SaveData save, RnDService rnd, ManufacturingService mfg)
        {
            this.save = save;
            this.rnd = rnd;
            this.mfg = mfg;
        }

        public void AdvanceOneWeek()
        {
            save.time.AdvanceWeeks(1);
            rnd.ProcessWeek();
            mfg.ProcessWeek();
            OnWeekAdvanced?.Invoke(save.time.currentWeek);
        }
    }
}

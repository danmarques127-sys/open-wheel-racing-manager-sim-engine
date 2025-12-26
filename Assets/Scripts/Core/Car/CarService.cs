using F1Manager.Core.Save;
using F1Manager.Data;

namespace F1Manager.Core.Car
{
    public class CarService
    {
        private readonly SaveData save;

        public CarService(SaveData save)
        {
            this.save = save;
        }

        // =========================
        // MANUFACTURED UPGRADE
        // =========================
        public void ApplyManufacturedUpgrade(
            string teamId,
            CarPartType partType,
            float perfDelta,
            float relDelta,
            string issue = null)
        {
            var car = save.GetCar(teamId);
            if (car == null) return;

            var part = car.GetPart(partType);
            if (part == null) return;

            part.performanceLevel += perfDelta;
            part.reliabilityLevel += relDelta;

            if (!string.IsNullOrEmpty(issue))
                part.knownIssues.Add(issue);
        }

        // =========================
        // WEAR FROM RACE
        // =========================
        public void ApplyWearFromRace(string teamId, CarPartBalancePreset balance)
        {
            var car = save.GetCar(teamId);
            if (car == null) return;

            for (int i = 0; i < car.parts.Count; i++)
            {
                var p = car.parts[i];
                var def = balance != null ? balance.Get(p.partType) : null;

                float wearAdd = def != null
                    ? def.baseWearPerRace
                    : 2f;

                p.wearPercent = Clamp0_100(p.wearPercent + wearAdd);
            }
        }

        // =========================
        // UTILS
        // =========================
        private float Clamp0_100(float v)
        {
            if (v < 0f) return 0f;
            if (v > 100f) return 100f;
            return v;
        }
    }
}

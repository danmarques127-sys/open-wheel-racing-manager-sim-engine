using System.Collections.Generic;
using F1Manager.Core.Car;
using F1Manager.Core.Save;

namespace F1Manager.Core.Manufacturing
{
    public class ManufacturingService
    {
        private readonly SaveData save;
        private readonly CarService carService;

        // Para “carregar” issue junto ao item sem inventar novo campo
        private readonly Dictionary<int, string> pendingIssueByIndex = new Dictionary<int, string>();

        public ManufacturingService(SaveData save, CarService carService)
        {
            this.save = save;
            this.carService = carService;
        }

        public void Enqueue(ManufacturingQueueItem item, string issue = null)
        {
            save.manufacturingQueue.Add(item);
            if (!string.IsNullOrEmpty(issue))
            {
                // salva issue referenciado pelo index atual
                pendingIssueByIndex[save.manufacturingQueue.Count - 1] = issue;
            }
        }

        public void ProcessWeek()
        {
            for (int i = save.manufacturingQueue.Count - 1; i >= 0; i--)
            {
                var q = save.manufacturingQueue[i];
                q.weeksRemaining -= 1;

                if (q.weeksRemaining <= 0)
                {
                    // Apply upgrade
                    string issue = null;
                    if (pendingIssueByIndex.TryGetValue(i, out var s)) issue = s;

                    carService.ApplyManufacturedUpgrade(q.teamId, q.partType, q.performanceLevelDelta, q.reliabilityDelta, issue);
                    save.manufacturingQueue.RemoveAt(i);
                }
                else
                {
                    save.manufacturingQueue[i] = q;
                }
            }
        }
    }
}

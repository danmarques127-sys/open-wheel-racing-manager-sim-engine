using System;
using System.Collections.Generic;

namespace F1Manager.Core.Staff
{
    [Serializable]
    public class StaffState
    {
        // staffIds por role (se você quiser travar 1 por role).
        public Dictionary<string, string> assignedByRole = new Dictionary<string, string>();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using F1Manager.Core.Drivers;

namespace F1Manager.Core.Market
{
    [Serializable]
    public class DriverMarketState
    {
        // “banco” vivo de pilotos (inclui regen)
        public List<DriverState> drivers = new List<DriverState>();

        // lista de mercado: free agents + jovens
        public List<DriverMarketEntry> marketEntries = new List<DriverMarketEntry>();

        // controle de IDs para regen
        [Min(0)] public int regenCounter = 0;

        public DriverState GetDriver(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) return null;
            return drivers.Find(d => d != null && d.driverId == driverId);
        }

        public DriverMarketEntry GetEntry(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) return null;
            return marketEntries.Find(e => e != null && e.driverId == driverId);
        }
    }
}

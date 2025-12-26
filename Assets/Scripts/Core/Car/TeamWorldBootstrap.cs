using System;
using UnityEngine;
using F1Manager.Core.Save;
using F1Manager.Core.Car;
using F1Manager.Core.Finance;
using F1Manager.Core.Facilities;

public static class TeamWorldBootstrap
{
    // Gera estados iniciais a partir do TeamData.
    public static void EnsureTeamInitialized(SaveData save, TeamData team, int specYear = 2026)
    {
        // CAR
        var car = save.GetCar(team.teamId);
        if (car == null)
        {
            car = TeamCarInitializer.BuildFromTeamData(team, specYear);
            save.carByTeam.Add(car);
        }

        // FACILITIES
        var fac = save.GetFacilities(team.teamId);
        if (fac == null)
        {
            fac = new FacilityState
            {
                teamId = team.teamId,
                hqLevel = team.hqLevel,
                aeroDeptLevel = team.aeroDepartment,
                puDeptLevel = team.powerUnitDept,
                strategyTeamLevel = team.strategyTeam
            };
            save.facilitiesByTeam.Add(fac);
        }

        // FINANCE
        var fin = save.GetFinance(team.teamId);
        if (fin == null)
        {
            // startingBudgetMillions provavelmente é float/int.
            // Convertemos para USD em long.
            long budgetUsd = (long)Math.Round(team.startingBudgetMillions * 1_000_000d);

            if (budgetUsd < 0L) budgetUsd = 0L;

            fin = new FinanceState
            {
                teamId = team.teamId,
                currentBudget = budgetUsd
            };
            save.financeByTeam.Add(fin);
        }
    }
}

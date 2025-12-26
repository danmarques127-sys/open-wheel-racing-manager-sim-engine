using System;
using UnityEngine;

namespace F1Manager.Data
{
    public enum ContractStatus
    {
        FreeAgent = 0,
        Signed = 1
    }

    public enum TerminationType
    {
        None = 0,
        TeamTerminates = 1,
        DriverTerminates = 2,
        Mutual = 3
    }

    public enum OptionType
    {
        None = 0,
        TeamOption = 1,
        DriverOption = 2,
        MutualOption = 3
    }

    [Serializable]
    public class DriverContract
    {
        [Header("Identity / Status")]
        public ContractStatus status = ContractStatus.FreeAgent;

        [Header("Team Link")]
        [Tooltip("TeamId do time que possui contrato com o piloto. Se vazio e status=FreeAgent, é agente livre.")]
        public string teamId;

        [Header("Terms")]
        [Min(0)] public int salaryPerYear;          // salário anual base
        [Min(0)] public int signingBonus;          // bônus de assinatura (pago ao assinar)
        [Min(0)] public int contractStartYear;     // ex: 2026
        [Min(0)] public int contractEndYear;       // ex: 2027 (inclusive)

        [Header("Clauses - Buyout (compra do contrato por terceiros)")]
        public bool hasBuyoutClause;
        [Min(0)] public int buyoutValue;           // valor para comprar/liberar o piloto

        [Header("Clauses - Early Termination (rescisão antecipada)")]
        public bool allowEarlyTermination = true;

        [Tooltip("Se true, o time pode rescindir pagando multa.")]
        public bool teamCanTerminate = true;

        [Tooltip("Se true, o piloto pode rescindir pagando multa (ex: insatisfação/saída).")]
        public bool driverCanTerminate = false;

        [Min(0)] public int terminationFeeTeam;    // multa se o TIME rescindir
        [Min(0)] public int terminationFeeDriver;  // multa se o PILOTO rescindir

        [Tooltip("Se true, a multa varia por ano restante (mais realista).")]
        public bool terminationFeeScalesWithRemainingYears = true;

        [Range(0f, 5f)]
        [Tooltip("Multiplicador por ano restante (ex: 1.0 = 1x por ano, 1.5 = 1.5x por ano).")]
        public float terminationFeePerRemainingYearMultiplier = 1.0f;

        [Header("Clauses - Options (renovação/saída no fim)")]
        public OptionType optionType = OptionType.None;

        [Min(0)]
        [Tooltip("Ano em que a opção pode ser exercida (normalmente contractEndYear).")]
        public int optionDecisionYear; // se 0, assume contractEndYear

        [Min(0)]
        [Tooltip("Quantos anos adiciona se a opção for exercida.")]
        public int optionExtraYears = 1;

        [Tooltip("Se true, salário pode ser reajustado ao exercer opção (percentual).")]
        public bool optionHasSalaryIncrease = true;

        [Range(0f, 1f)]
        [Tooltip("Aumento percentual ao exercer opção. Ex: 0.10 = +10%.")]
        public float optionSalaryIncreasePct = 0.10f;

        [Header("Performance Bonuses (por ano)")]
        [Min(0)] public int bonusPerWin;
        [Min(0)] public int bonusPerPodium;
        [Min(0)] public int bonusPerPoint;

        [Header("Behavior / Negotiation (opcional, mas útil para AI)")]
        [Range(0, 100)] public int loyalty = 50;         // tendência a ficar
        [Range(0, 100)] public int ambition = 50;        // busca time melhor
        [Range(0, 100)] public int riskTolerance = 50;   // aceita contratos curtos/variáveis

        public bool IsActive(int seasonYear)
        {
            return status == ContractStatus.Signed &&
                   !string.IsNullOrEmpty(teamId) &&
                   seasonYear >= contractStartYear &&
                   seasonYear <= contractEndYear;
        }

        public int RemainingYears(int seasonYear)
        {
            if (seasonYear > contractEndYear) return 0;
            return Mathf.Max(0, contractEndYear - seasonYear + 1);
        }

        public int GetTeamTerminationCost(int seasonYear)
        {
            if (!allowEarlyTermination || !teamCanTerminate) return int.MaxValue;
            int baseFee = Mathf.Max(0, terminationFeeTeam);

            if (!terminationFeeScalesWithRemainingYears) return baseFee;

            int yearsLeft = RemainingYears(seasonYear);
            float scaled = baseFee * yearsLeft * Mathf.Max(0f, terminationFeePerRemainingYearMultiplier);
            return Mathf.RoundToInt(scaled);
        }

        public int GetDriverTerminationCost(int seasonYear)
        {
            if (!allowEarlyTermination || !driverCanTerminate) return int.MaxValue;
            int baseFee = Mathf.Max(0, terminationFeeDriver);

            if (!terminationFeeScalesWithRemainingYears) return baseFee;

            int yearsLeft = RemainingYears(seasonYear);
            float scaled = baseFee * yearsLeft * Mathf.Max(0f, terminationFeePerRemainingYearMultiplier);
            return Mathf.RoundToInt(scaled);
        }

        public int GetBuyoutCost()
        {
            if (!hasBuyoutClause) return int.MaxValue;
            return Mathf.Max(0, buyoutValue);
        }

        public int GetOptionDecisionYear()
        {
            return optionDecisionYear > 0 ? optionDecisionYear : contractEndYear;
        }

        public bool CanExerciseOption(int seasonYear, bool isTeamDecision, bool isDriverDecision)
        {
            if (optionType == OptionType.None) return false;
            if (seasonYear != GetOptionDecisionYear()) return false;

            switch (optionType)
            {
                case OptionType.TeamOption:
                    return isTeamDecision;
                case OptionType.DriverOption:
                    return isDriverDecision;
                case OptionType.MutualOption:
                    return isTeamDecision && isDriverDecision;
                default:
                    return false;
            }
        }

        public void ExerciseOption()
        {
            if (optionType == OptionType.None) return;

            int extra = Mathf.Max(0, optionExtraYears);
            if (extra <= 0) return;

            contractEndYear += extra;

            if (optionHasSalaryIncrease)
            {
                salaryPerYear = Mathf.RoundToInt(salaryPerYear * (1f + Mathf.Clamp01(optionSalaryIncreasePct)));
            }
        }

        public int CalculateAnnualBonuses(int wins, int podiums, int points)
        {
            wins = Mathf.Max(0, wins);
            podiums = Mathf.Max(0, podiums);
            points = Mathf.Max(0, points);

            return (wins * Mathf.Max(0, bonusPerWin))
                 + (podiums * Mathf.Max(0, bonusPerPodium))
                 + (points * Mathf.Max(0, bonusPerPoint));
        }

        public void Sign(string newTeamId, int startYear, int endYear, int salary, int signingBonusValue = 0)
        {
            teamId = newTeamId;
            status = ContractStatus.Signed;
            contractStartYear = Mathf.Max(0, startYear);
            contractEndYear = Mathf.Max(contractStartYear, endYear);
            salaryPerYear = Mathf.Max(0, salary);
            signingBonus = Mathf.Max(0, signingBonusValue);
        }

        public void ReleaseToFreeAgent()
        {
            teamId = "";
            status = ContractStatus.FreeAgent;
        }
    }
}

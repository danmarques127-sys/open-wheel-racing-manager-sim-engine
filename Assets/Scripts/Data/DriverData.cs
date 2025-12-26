using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "Driver_", menuName = "F1 Manager/Data/Driver")]
    public class DriverData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("ID único (ex: 'max_verstappen'). Não pode repetir.")]
        public string driverId;

        public string firstName;
        public string lastName;

        [Tooltip("Nome exibido (se vazio, será gerado automaticamente no inspector via OnValidate).")]
        public string displayName;

        [Min(14)] public int age;
        public string nationality;

        [Header("Ratings")]
        [Range(1, 100)] public int overall;

        [Range(1, 100)] public int racePace;
        [Range(1, 100)] public int qualifying;
        [Range(1, 100)] public int consistency;
        [Range(1, 100)] public int aggression;
        [Range(1, 100)] public int tireManagement;
        [Range(1, 100)] public int wetSkill;

        [Header("Mental / Growth")]
        [Range(0, 100)] public int morale;
        [Range(1, 100)] public int potential;

        // ===============================
        // ROSTER / CAREER (NEW)
        // ===============================

        [Header("Roster Role (NEW)")]
        public DriverRole role = DriverRole.Starter;

        [Header("Career Status (NEW)")]
        public DriverCareerStatus careerStatus = DriverCareerStatus.Active;

        // ===============================
        // LEGACY STATUS (mantido para compatibilidade)
        // ===============================

        [Header("Roster Status (LEGACY)")]
        [Tooltip("LEGACY: mantenha temporariamente se você já usa em UI/lógica. Será sincronizado automaticamente com role/contract.")]
        public DriverStatus status = DriverStatus.FreeAgent;

        [Header("Contract")]
        public DriverContract contract = new DriverContract();

        [Header("Optional Visuals")]
        public Sprite portrait;

        // ===============================
        // DERIVED HELPERS
        // ===============================

        private static string NormId(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? "" : s.Trim().ToLowerInvariant();
        }

        public bool IsFreeAgent
        {
            get
            {
                if (contract == null) return true;

                if (contract.status == ContractStatus.FreeAgent) return true;
                if (string.IsNullOrWhiteSpace(contract.teamId)) return true;

                return false;
            }
        }

        public string CurrentTeamId
        {
            get
            {
                if (IsFreeAgent) return "";
                return NormId(contract.teamId);
            }
        }

        public bool CanRace
        {
            get
            {
                if (IsFreeAgent) return false;
                if (careerStatus != DriverCareerStatus.Active) return false;
                return role == DriverRole.Starter;
            }
        }

        public float RoleModifier
        {
            get
            {
                switch (role)
                {
                    case DriverRole.Reserve: return 0.95f;
                    case DriverRole.Test:    return 0.85f;
                    default:                 return 1.0f;
                }
            }
        }

        public float CareerStatusModifier
        {
            get
            {
                switch (careerStatus)
                {
                    case DriverCareerStatus.Injured:   return 0.70f;
                    case DriverCareerStatus.Suspended: return 0.0f;
                    case DriverCareerStatus.Retired:   return 0.0f;
                    default:                           return 1.0f;
                }
            }
        }

        public int EffectiveRacePace => Mathf.Clamp(Mathf.RoundToInt(racePace * RoleModifier * CareerStatusModifier), 0, 100);
        public int EffectiveQualifying => Mathf.Clamp(Mathf.RoundToInt(qualifying * RoleModifier * CareerStatusModifier), 0, 100);
        public int EffectiveConsistency => Mathf.Clamp(Mathf.RoundToInt(consistency * CareerStatusModifier), 0, 100);

        private void OnValidate()
        {
            // 1) Normalizações básicas
            driverId = NormId(driverId);
            nationality = string.IsNullOrWhiteSpace(nationality) ? "" : nationality.Trim();
            firstName = string.IsNullOrWhiteSpace(firstName) ? "" : firstName.Trim();
            lastName = string.IsNullOrWhiteSpace(lastName) ? "" : lastName.Trim();

            // idade mínima
            age = Mathf.Max(14, age);

            // 2) Gera displayName automaticamente se estiver vazio
            if (string.IsNullOrWhiteSpace(displayName))
            {
                string fn = string.IsNullOrWhiteSpace(firstName) ? "" : firstName.Trim();
                string ln = string.IsNullOrWhiteSpace(lastName) ? "" : lastName.Trim();
                displayName = (fn + " " + ln).Trim();
            }
            else
            {
                displayName = displayName.Trim();
            }

            // 3) Garantir que contract nunca seja nulo
            if (contract == null)
                contract = new DriverContract();

            // 4) ✅ Normaliza teamId no contrato (PADRÃO ÚNICO)
            contract.teamId = NormId(contract.teamId);

            // 5) Sincronização do status do contrato
            if (string.IsNullOrWhiteSpace(contract.teamId))
            {
                contract.status = ContractStatus.FreeAgent;
            }
            else
            {
                contract.status = ContractStatus.Signed;
            }

            // 6) Sincroniza LEGACY status
            if (IsFreeAgent)
            {
                status = DriverStatus.FreeAgent;
            }
            else
            {
                status = GetLegacyContractedStatusFallback();
            }

            // 7) Sanidade: aposentado (mantém como você deixou)
            if (careerStatus == DriverCareerStatus.Retired)
            {
                // opcional: soltar contrato (se você quiser no futuro)
                // contract.teamId = "";
                // contract.status = ContractStatus.FreeAgent;
            }
        }

        private DriverStatus GetLegacyContractedStatusFallback()
        {
            if (System.Enum.IsDefined(typeof(DriverStatus), "Contracted"))
                return (DriverStatus)System.Enum.Parse(typeof(DriverStatus), "Contracted");

            if (System.Enum.IsDefined(typeof(DriverStatus), "Signed"))
                return (DriverStatus)System.Enum.Parse(typeof(DriverStatus), "Signed");

            return DriverStatus.FreeAgent;
        }

        public string GetFullName()
        {
            return string.IsNullOrWhiteSpace(displayName)
                ? $"{firstName} {lastName}".Trim()
                : displayName.Trim();
        }
    }
}

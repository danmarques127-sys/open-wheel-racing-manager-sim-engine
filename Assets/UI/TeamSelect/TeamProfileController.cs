using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TeamProfileController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Database")]
    [SerializeField] private TeamProfileData[] teams;

    [Header("Scenes")]
    [SerializeField] private string backSceneName = "TeamSelect";
    [SerializeField] private string nextSceneName = "CareerHub"; // você cria depois

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) { Debug.LogError("[TeamProfile] UIDocument não encontrado."); return; }

        var root = uiDocument.rootVisualElement;

        var themeBg = root.Q<VisualElement>("themeBg");
        var teamLogo = root.Q<Image>("teamLogo");
        var teamName = root.Q<Label>("teamName");
        var constructorsTitles = root.Q<Label>("constructorsTitles");
        var driversTitles = root.Q<Label>("driversTitles");
        var historyText = root.Q<Label>("historyText");
        var driversList = root.Q<VisualElement>("driversList");

        var btnBack = root.Q<Button>("btnBack");
        var btnConfirm = root.Q<Button>("btnConfirm");

        if (btnBack != null) btnBack.clicked += () => SceneManager.LoadScene(backSceneName);

        if (btnConfirm != null)
        {
            btnConfirm.clicked += () =>
            {
                SceneManager.LoadScene(nextSceneName);
            };
        }

        var selectedId = GameSelection.SelectedTeamId;

        if (string.IsNullOrWhiteSpace(selectedId))
        {
            Debug.LogError("[TeamProfile] SelectedTeamId vazio. Volte ao TeamSelect e escolha uma equipe.");
            return;
        }

        if (teams == null || teams.Length == 0)
        {
            Debug.LogError("[TeamProfile] Array 'teams' vazio. Arraste seus TeamProfileData no Inspector.");
            return;
        }

        var team = teams.FirstOrDefault(t => t != null && string.Equals(t.teamId, selectedId, StringComparison.OrdinalIgnoreCase));
        if (team == null)
        {
            Debug.LogError($"[TeamProfile] Não achei TeamProfileData com teamId='{selectedId}'.");
            return;
        }

        // ---- Conteúdo base ----
        if (teamName != null) teamName.text = team.teamName;

        if (teamLogo != null)
        {
            teamLogo.sprite = team.teamLogo;
            teamLogo.scaleMode = ScaleMode.ScaleToFit;
        }

        if (constructorsTitles != null) constructorsTitles.text = team.constructorsTitles.ToString();
        if (driversTitles != null) driversTitles.text = team.driversTitles.ToString();
        if (historyText != null) historyText.text = team.shortHistory;

        // ---- Tema (seguro, sem gradient no USS) ----
        if (themeBg != null)
        {
            // (se você quiser inverter as cores depois é só inverter aqui)
            Color themed = Color.Lerp(team.secondaryColor, team.primaryColor, Mathf.Clamp01(team.themeIntensity));
            themeBg.style.backgroundColor = new StyleColor(themed);
        }

        if (btnConfirm != null)
        {
            btnConfirm.style.backgroundColor = new StyleColor(new Color(team.primaryColor.r, team.primaryColor.g, team.primaryColor.b, 0.18f));
        }

        // ---- Drivers: nome humano + contrato (agora lê Contract End Year dentro de Contract) ----
        if (driversList != null)
        {
            driversList.Clear();

            IEnumerable enumerable = GetEnumerable(team);

            if (enumerable == null)
            {
                Debug.LogWarning("[TeamProfile] Não encontrei lista de drivers (currentDrivers / drivers / driverList / lineup) no TeamProfileData.");
                return;
            }

            foreach (var d in enumerable)
            {
                if (d == null) continue;

                // 1) tenta nome direto (campos comuns)
                string name = ReadString(d,
                    "fullName", "FullName",
                    "driverName", "DriverName",
                    "displayName", "DisplayName",
                    "name", "Name",
                    "shortName", "ShortName");

                // 2) tenta montar first + last
                if (string.IsNullOrWhiteSpace(name))
                {
                    string first = ReadString(d, "firstName", "FirstName", "givenName", "GivenName");
                    string last  = ReadString(d, "lastName", "LastName", "surname", "Surname");
                    string joined = (first + " " + last).Trim();
                    if (!string.IsNullOrWhiteSpace(joined))
                        name = joined;
                }

                // role pode ser enum, string, etc.
                string role = ReadString(d,
                    "role", "Role", "driverRole", "DriverRole", "seatRole", "SeatRole");

                // 3) contrato: primeiro tenta no nível 1 (se existir)
                int contractUntil = ReadInt(d,
                    "contractUntilYear", "ContractUntilYear",
                    "contractUntil", "ContractUntil",
                    "contractEndYear", "ContractEndYear",
                    "contractEndSeason", "ContractEndSeason",
                    "untilYear", "UntilYear",
                    "endYear", "EndYear",
                    "expiresYear", "ExpiresYear");

                // 4) SE AINDA NÃO ACHOU, tenta dentro do objeto Contract (igual seu print)
                if (contractUntil <= 0)
                {
                    object contractObj = ReadObject(d, "contract", "Contract");
                    if (contractObj != null)
                    {
                        contractUntil = ReadInt(contractObj,
                            "contractEndYear", "ContractEndYear",
                            "endYear", "EndYear",
                            "ContractEnd", "contractEnd",
                            "contractUntilYear", "ContractUntilYear",
                            "untilYear", "UntilYear",
                            "expiresYear", "ExpiresYear");
                    }
                }

                // 5) fallback do nome (NUNCA MAIS d.ToString() cru)
                if (string.IsNullOrWhiteSpace(name))
                {
                    string id = ReadString(d, "driverId", "DriverId", "id", "Id", "code", "Code", "slug", "Slug");
                    if (!string.IsNullOrWhiteSpace(id))
                        name = HumanizeId(id);
                    else
                        name = HumanizeId(((UnityEngine.Object)d).name);
                }

                string rightText;
                if (contractUntil > 0 && !string.IsNullOrWhiteSpace(role))
                    rightText = $"{role} • Contract until {contractUntil}";
                else if (contractUntil > 0)
                    rightText = $"Contract until {contractUntil}";
                else if (!string.IsNullOrWhiteSpace(role))
                    rightText = $"{role}";
                else
                    rightText = "Contract —";

                var row = new VisualElement();
                row.AddToClassList("driver-row");

                var left = new Label(name);
                left.AddToClassList("driver-name");

                var right = new Label(rightText);
                right.AddToClassList("driver-meta");

                row.Add(left);
                row.Add(right);

                driversList.Add(row);
            }
        }
    }

    // =============================
    // Helpers (reflection safe)
    // =============================

    private static IEnumerable GetEnumerable(TeamProfileData team)
    {
        if (team == null) return null;

        var candidates = new[]
        {
            "currentDrivers",
            "CurrentDrivers",
            "drivers",
            "Drivers",
            "driverList",
            "DriverList",
            "lineup",
            "Lineup"
        };

        var t = team.GetType();

        foreach (var name in candidates)
        {
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null)
            {
                var val = p.GetValue(team);
                if (val is IEnumerable e) return e;
            }

            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
            {
                var val = f.GetValue(team);
                if (val is IEnumerable e) return e;
            }
        }

        return null;
    }

    private static string ReadString(object obj, params string[] names)
    {
        foreach (var n in names)
        {
            if (TryGetMemberValue(obj, n, out var v))
            {
                if (v == null) continue;
                if (v is string s) return s;

                var type = v.GetType();
                if (type.IsEnum) return v.ToString();

                return Convert.ToString(v);
            }
        }
        return "";
    }

    private static int ReadInt(object obj, params string[] names)
    {
        foreach (var n in names)
        {
            if (TryGetMemberValue(obj, n, out var v))
            {
                if (v == null) continue;

                if (v is int i) return i;
                if (v is long l) return (int)l;
                if (v is float f) return Mathf.RoundToInt(f);
                if (v is double d) return (int)Math.Round(d);

                if (int.TryParse(Convert.ToString(v), out var parsed))
                    return parsed;
            }
        }
        return 0;
    }

    // ✅ NOVO: lê objeto aninhado (ex: DriverData.Contract)
    private static object ReadObject(object obj, params string[] names)
    {
        foreach (var n in names)
        {
            if (TryGetMemberValue(obj, n, out var v))
            {
                if (v != null) return v;
            }
        }
        return null;
    }

    private static bool TryGetMemberValue(object obj, string name, out object value)
    {
        value = null;
        if (obj == null || string.IsNullOrWhiteSpace(name)) return false;

        var t = obj.GetType();

        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null)
        {
            value = p.GetValue(obj);
            return true;
        }

        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null)
        {
            value = f.GetValue(obj);
            return true;
        }

        return false;
    }

    private static string HumanizeId(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Unknown Driver";

        string s = raw.Trim();

        // remove prefixos comuns
        s = s.Replace("Driver_", "", StringComparison.OrdinalIgnoreCase);
        s = s.Replace("driver_", "", StringComparison.OrdinalIgnoreCase);
        s = s.Replace("drv_", "", StringComparison.OrdinalIgnoreCase);

        // separadores
        s = s.Replace("_", " ").Replace("-", " ");

        while (s.Contains("  ")) s = s.Replace("  ", " ");

        // Title Case
        s = s.ToLowerInvariant();
        s = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s);

        return s;
    }
}

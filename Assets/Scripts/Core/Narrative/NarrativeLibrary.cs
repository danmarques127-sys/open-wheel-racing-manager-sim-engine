using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Core.Narrative
{
    [CreateAssetMenu(fileName = "NarrativeLibrary_", menuName = "F1 Manager/Narrative/Library", order = 60)]
    public class NarrativeLibrary : ScriptableObject
    {
        // ---------- PRE-RACE ----------
        [Header("Pre-race")]
        public List<string> introsNormal = new()
        {
            "O paddock chega em {track} com expectativas altas.",
            "A caravana desembarca em {track} e o clima é de confiança no grid."
        };

        public List<string> introsTense = new()
        {
            "A disputa esquenta antes de {track} — cada ponto pode valer ouro.",
            "O campeonato entra em fase crítica e {track} pode mudar tudo."
        };

        public List<string> sprintHooks = new()
        {
            "Com sprint no fim de semana, as equipes têm pouco espaço para errar.",
            "Formato sprint: risco maior, e quem vacilar paga caro."
        };

        public List<string> standardHooks = new()
        {
            "O formato tradicional dá margem para estratégia, mas exige ritmo constante.",
            "A etapa promete decisões no detalhe — pneus e consistência serão chave."
        };

        public List<string> preRaceCrisis = new()
        {
            "{winnerTeam} tenta afastar rumores de crise interna antes de {track}.",
            "A pressão aumenta nos bastidores, e {track} vira prova de fogo para alguns."
        };

        // ---------- POST-RACE HEADLINES ----------
        [Header("Post-race headlines (keyed)")]
        public List<HeadlineTemplate> postRaceHeadlines = new()
        {
            new HeadlineTemplate { key = "STANDARD", template = "{winner} vence em {track} e soma pontos cruciais." },
            new HeadlineTemplate { key = "STREAK", template = "{winner} mantém a sequência e amplia a vantagem após {track}." },
            new HeadlineTemplate { key = "CHAOS", template = "Caos em {track}: safety car e incidentes, mas {winner} sobrevive e vence." },
            new HeadlineTemplate { key = "RAIN", template = "Chuva em {track} embaralha tudo — {winner} brilha no molhado." },
            new HeadlineTemplate { key = "FROM_BEHIND", template = "{winner} vence após largar em P{grid}: recuperação impressionante em {track}." },
            new HeadlineTemplate { key = "UPSET", template = "Zebra em {track}: {winnerTeam} derruba favoritos e {winner} leva a vitória." }
        };

        [Serializable]
        public class HeadlineTemplate
        {
            public string key;
            [TextArea(1, 3)] public string template;
        }

        // ---------- POST-RACE LINES ----------
        [Header("Post-race recaps")]
        public List<string> postRaceRecaps = new()
        {
            "{winner} controlou o ritmo e garantiu a vitória em {track}.",
            "Estratégia e consistência definiram o domingo em {track}, com {winner} no topo."
        };

        public List<string> weatherLines = new()
        {
            "As condições instáveis em {track} puniram qualquer hesitação.",
            "No molhado, coragem e leitura de pista decidiram a etapa."
        };

        public List<string> safetyLines = new()
        {
            "Intervenções do safety car mudaram o jogo em {track}.",
            "O caos estratégico apareceu e a corrida virou um xadrez sob pressão."
        };

        public List<string> upsetLines = new()
        {
            "Os favoritos saem arranhados, e a próxima etapa já promete reação.",
            "O resultado surpreendente abre margem para novas narrativas no campeonato."
        };

        // ---------- MARKET ----------
        [Header("Market")]
        public List<string> marketGenericRumors = new()
        {
            "Rumores de mercado crescem no paddock — algumas equipes já planejam a próxima temporada.",
            "Nos bastidores, conversas discretas sugerem mudanças para a próxima temporada."
        };

        public List<string> marketTensionRumors = new()
        {
            "Tensão interna aumenta e pode acelerar decisões no mercado.",
            "Um clima pesado nos bastidores indica que mudanças podem acontecer antes do esperado."
        };

        // ---------- REGULATIONS ----------
        [Header("Regulations")]
        public List<string> regulationLines = new()
        {
            "A federação discute ajustes no regulamento para as próximas temporadas.",
            "Equipes monitoram possíveis mudanças técnicas que podem redefinir a hierarquia."
        };

        // ---------- GOSSIP ----------
        [Header("Gossip")]
        public List<string> gossipLight = new()
        {
            "O paddock comenta pequenas provocações — nada fora do normal.",
            "Clima leve fora da pista, mas todos sabem: a próxima corrida é outra história."
        };

        public List<string> gossipTension = new()
        {
            "Nos bastidores, um comentário virou faísca — e a tensão aumentou.",
            "Alguns olhares no paddock dizem mais que palavras: a pressão está no limite."
        };

        public bool TryGetHeadline(string key, out string template)
        {
            for (int i = 0; i < postRaceHeadlines.Count; i++)
            {
                if (string.Equals(postRaceHeadlines[i].key, key, StringComparison.OrdinalIgnoreCase))
                {
                    template = postRaceHeadlines[i].template;
                    return true;
                }
            }

            template = null;
            return false;
        }
    }
}

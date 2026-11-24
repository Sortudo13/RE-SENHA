// Gera as conversas padrao de Re:Senha como ScriptableObjects.
// No Unity, execute: Tools > Re:Senha > Gerar Conversas.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ChatContentGenerator
{
    private const string RootFolder = "Assets/Scriptable Objects/Chat";
    private const string ConversationsFolder = RootFolder + "/Conversations";
    private const string NodesFolder = RootFolder + "/Nodes";
    private const string InboxAssetPath = RootFolder + "/ChatInbox.asset";

    [MenuItem("Tools/Re:Senha/Gerar Conversas")]
    public static void Generate()
    {
        ChatConversation.IsGenerating = true;

        try
        {
            EnsureFolder("Assets/Scriptable Objects", "Chat");
            EnsureFolder(RootFolder, "Conversations");
            EnsureFolder(RootFolder, "Nodes");

            var specs = CreateConversationSpecs();
            var inboxEntries = new List<ChatInbox.Entry>();

            foreach (var spec in specs)
            {
                var conversation = LoadOrCreateAsset<ChatConversation>(
                    $"{ConversationsFolder}/{spec.AssetName}.asset",
                    spec.AssetName
                );

                var newNodes = new List<ChatNode>();
                foreach (var nodeSpec in spec.Nodes)
                {
                    var node = LoadOrCreateAsset<ChatNode>(
                        $"{NodesFolder}/{spec.AssetName}_{nodeSpec.NodeId}.asset",
                        $"{spec.AssetName}_{nodeSpec.NodeId}"
                    );

                    node.nodeId = nodeSpec.NodeId;
                    node.npcMessage = nodeSpec.Message;
                    node.typingDelay = nodeSpec.TypingDelay;
                    node.options = nodeSpec.Options ?? new List<ChatOption>();
                    node.nextNodeIdIfNoChoice = nodeSpec.NextNodeIdIfNoChoice;
                    node.isEndNode = nodeSpec.IsEndNode;
                    node.scheduledDay = nodeSpec.ScheduledDay;
                    node.scheduledHour = nodeSpec.ScheduledHour;
                    node.scheduledMinute = nodeSpec.ScheduledMinute;
                    node.requiresAnswerToNodeId = nodeSpec.RequiresAnswerToNodeId;

                    EditorUtility.SetDirty(node);
                    newNodes.Add(node);
                }

                conversation.conversationId = spec.ConversationId;
                conversation.npcName = spec.NpcName;
                conversation.npcAvatar = null;
                conversation.RefreshCache(); // Limpa o cache antigo antes de trocar os nós
                conversation.nodes = newNodes;
                conversation.startNodeId = "inicio";
                conversation.RefreshCache();
                EditorUtility.SetDirty(conversation);
                AssetDatabase.SaveAssets();

                inboxEntries.Add(
                    new ChatInbox.Entry
                    {
                        conversation = conversation,
                        arrivalDay = spec.ArrivalDay,
                        arrivalHour = spec.ArrivalHour,
                        arrivalMinute = spec.ArrivalMinute,
                        sortOrder = spec.SortOrder,
                    }
                );
            }

            var inbox = AssetDatabase.LoadAssetAtPath<ChatInbox>(InboxAssetPath);
            if (inbox == null)
            {
                inbox = ScriptableObject.CreateInstance<ChatInbox>();
                AssetDatabase.CreateAsset(inbox, InboxAssetPath);
            }

            inbox.entries = inboxEntries;
            EditorUtility.SetDirty(inbox);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[ChatContentGenerator] Geradas {specs.Count} conversas, {specs.Count * 5} nós e o ChatInbox."
            );
            Selection.activeObject = inbox;
            EditorGUIUtility.PingObject(inbox);
        }
        finally
        {
            ChatConversation.IsGenerating = false;
        }
    }

    private static T LoadOrCreateAsset<T>(string path, string assetName)
        where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        asset.name = assetName;
        return asset;
    }

    private static void EnsureFolder(string parentFolder, string newFolderName)
    {
        string folderPath = $"{parentFolder}/{newFolderName}";
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        AssetDatabase.CreateFolder(parentFolder, newFolderName);
    }

    private static List<ConversationSpec> CreateConversationSpecs()
    {
        return new List<ConversationSpec>
        {
            // ==========================================
            // DIA 1
            // ==========================================
            new ConversationSpec(
                "BoasVindas_Clip",
                "clip_boas_vindas",
                "Clip",
                1,
                8,
                5,
                0,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Saudações, parceiro! Sou o Clip. Estou aqui para te ajudar a manter nossa empresa segura. O RH monitora nosso expediente, sabia?",
                        Options(
                            Option("Monitora? Como assim?", "explicar", "perguntou_regras"),
                            Option("Já sei, as regras são claras.", "confiante", "confiante")
                        )
                    ),
                    new NodeSpec(
                        "explicar",
                        "Se você aprovar documentos falsos ou cair em golpes, leva advertência. Três e você está fora. Use o Manual e o modo de inspeção para verificar tudo.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "confiante",
                        "Ótimo! Mas nunca subestime os hackers. Lembre-se de usar o Manual e o inspetor de documentos.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Bom primeiro dia! Qualquer coisa, estou no chat.",
                        null,
                        null,
                        true
                    ),
                }
            ),
            new ConversationSpec(
                "Phishing_Clip",
                "clip_phishing",
                "Clip",
                1,
                10,
                0,
                1,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Parceiro, o RH acabou de mandar as orientações do dia. Mas cuidado: os hackers adoram se passar por colegas ou pelo RH.",
                        Options(
                            Option("O que é Phishing?", "explicar", "solicitou_explicacao"),
                            Option("Como eu me protejo?", "dicas", "pediu_dicas"),
                            Option("Vou tomar cuidado.", "fim", "confiante")
                        )
                    ),
                    new NodeSpec(
                        "explicar",
                        "É uma técnica onde o atacante se passa por alguém confiável para roubar dados, senhas ou instalar vírus. Eles usam a urgência para te fazer clicar.",
                        Options(Option("Entendi. E como eu me protejo?", "dicas", "pediu_dicas")),
                        "dicas"
                    ),
                    new NodeSpec(
                        "dicas",
                        "1. Confira sempre o remetente.\n2. Desconfie de anexos inesperados.\n3. Não clique em links suspeitos.\n4. Na dúvida, use o inspetor de documentos.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Fique atento aos e-mails!", null, null, true),
                }
            ),
            new ConversationSpec(
                "Odisseu_Observa",
                "odisseu_observa_d1",
                "Odisseu",
                1,
                14,
                0,
                2,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Boa tarde. Estive observando suas decisões hoje. Gostei do que vi... até agora.",
                        Options(
                            Option("Quem é você?", "quem", "perguntou_identidade"),
                            Option("Vou reportar isso.", "reportar", "reportou")
                        )
                    ),
                    new NodeSpec(
                        "quem",
                        "Sou alguém que testa sistemas. E pessoas. Você passou no primeiro teste.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "reportar",
                        "Reporte. Isso só torna o jogo mais interessante.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Até breve.", null, null, true),
                    new NodeSpec(
                        "cobranca",
                        "Você não me respondeu. Isso me deixa... curioso. Nos vemos amanhã.",
                        Options(),
                        null,
                        true,
                        1.5f,
                        scheduledDay: 1,
                        scheduledHour: 15,
                        scheduledMinute: 30
                    ),
                }
            ),
            // ==========================================
            // DIA 2
            // ==========================================
            new ConversationSpec(
                "Clip_Inspetor",
                "clip_dica_inspetor",
                "Clip",
                2,
                8,
                30,
                3,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Bom dia! Hoje vai chegar um documento para você aprovar. A ferramenta certa para isso é a Inspeção. Já usou?",
                        Options(
                            Option("Como funciona?", "explicar", "pediu_explicacao"),
                            Option("Já sei usar.", "fim", "confiante")
                        )
                    ),
                    new NodeSpec(
                        "explicar",
                        "Abra o documento, ative a Inspeção, selecione o campo e compare com a regra do Manual. Se estiver tudo certo, aí sim aprove.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Qualquer coisa, me chame.", null, null, true),
                    new NodeSpec(
                        "lembrete",
                        "Só passando para lembrar: não deixe de inspecionar os anexos de hoje!",
                        Options(Option("Entendido.", "fim_lembrete", "agradeceu")),
                        null,
                        true,
                        1.5f,
                        scheduledDay: 2,
                        scheduledHour: 11,
                        scheduledMinute: 0,
                        requiresAnswerToNodeId: "inicio"
                    ),
                    new NodeSpec("fim_lembrete", "Até mais!", null, null, true), // CORRIGIDO AQUI
                }
            ),
            new ConversationSpec(
                "Odisseu_Ameaca_D2",
                "odisseu_ameaca_d2",
                "Odisseu",
                2,
                10,
                15,
                4,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "NationalTech tem algumas falhas interessantes. Vamos ver se você consegue pegá-las antes de mim.",
                        Options(
                            Option("Do que está falando?", "falar", "perguntou"),
                            Option("Vou te pegar.", "pegar", "desafiou")
                        )
                    ),
                    new NodeSpec(
                        "falar",
                        "Abra os seus olhos. Ou melhor, os seus e-mails.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "pegar",
                        "Adoro um desafio. Mas cuidado para não tropeçar no caminho.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Até logo.", null, null, true),
                    new NodeSpec(
                        "ataque",
                        "Você está demorando muito para agir. Achei que fosse mais rápido. Vou ajudar a sua empresa a acordar.",
                        Options(Option("O que você fez?!", "fim_ataque", "desespero")),
                        null,
                        true,
                        1.5f,
                        scheduledDay: 2,
                        scheduledHour: 14,
                        scheduledMinute: 0
                    ),
                    new NodeSpec("fim_ataque", "Aguarde e verá.", null, null, true), // CORRIGIDO AQUI
                }
            ),
            // ==========================================
            // DIA 3
            // ==========================================
            new ConversationSpec(
                "Provocacao_Odisseu",
                "odisseu_provocacao",
                "Odisseu",
                3,
                10,
                30,
                5,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Saudações, caro funcionário. Permita-me apresentar-me: sou Odisseu, líder da Cavalo de Troia. Estou aqui para... testar a resiliência da sua empresa.",
                        Options(
                            Option("Você não me assusta.", "provocou", "provocou_odisseu"),
                            Option("O que você quer?", "perguntou", "perguntou_objetivo"),
                            Option("Vou reportar você!", "reportou", "reportou_odisseu")
                        )
                    ),
                    new NodeSpec(
                        "provocou",
                        "Ah, bravura. Admiro isso. Mas bravura sem cautela é tolice.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "perguntou",
                        "Quero apenas... conversar. A solidão da existência digital é um fardo.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "reportou",
                        "Reporte. Isso só tornará o jogo mais interessante.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Até a próxima. (Ou não.)", null, null, true),
                }
            ),
            new ConversationSpec(
                "Alerta_Link_Clip",
                "clip_alerta_link",
                "Clip",
                3,
                14,
                0,
                6,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Parceiro, detectei um link suspeito circulando. O domínio é nationa1tech.com (com '1' em vez de 'l'). Recomendo não clicar.",
                        Options(
                            Option("Obrigado pelo alerta.", "agradeceu", "agradeceu_alerta"),
                            Option("Vou clicar para investigar.", "clicou", "clicou_link_suspeito"),
                            Option("Como você detectou isso?", "explicou", "solicitou_explicacao")
                        )
                    ),
                    new NodeSpec("agradeceu", "De nada. Fique atento.", Options(), "fim"),
                    new NodeSpec(
                        "clicou",
                        "NÃO! Isso é exatamente o que o atacante quer. Nunca clique em links suspeitos.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "explicou",
                        "Analisei o domínio e o certificado SSL. O certificado é inválido e o domínio imita o nosso.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Continue vigilante.", null, null, true),
                }
            ),
            // ==========================================
            // DIA 4
            // ==========================================
            new ConversationSpec(
                "Dilema_Odisseu",
                "odisseu_dilema_moral",
                "Odisseu",
                4,
                11,
                0,
                7,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Caro funcionário, encontrei uma falha no sistema legado. Poderia explorá-la, mas... isso causaria transtornos aos funcionários. O que você faria no meu lugar?",
                        Options(
                            Option("Segurança sempre.", "seguranca", "priorizou_seguranca"),
                            Option("Minimize o impacto.", "minimizar", "incentivou_humanidade"),
                            Option("Deixe como está.", "ignorar", "repudiou_humanidade")
                        )
                    ),
                    new NodeSpec("seguranca", "Frio e calculista. Gosto disso.", Options(), "fim"),
                    new NodeSpec(
                        "minimizar",
                        "Uma abordagem... humana. Interessante.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "ignorar",
                        "A negligência é a mãe de todas as brechas.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Até breve.", null, null, true),
                }
            ),
            new ConversationSpec(
                "Clip_Alerta_Anexo",
                "clip_alerta_anexo",
                "Clip",
                4,
                14,
                30,
                8,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Ei, vi um anexo estranho chegar na caixa da Empresa. Parece um PDF, mas o nome está esquisito. Cuidado ao abrir.",
                        Options(
                            Option("Vou inspecionar.", "inspecionar", "prudente"),
                            Option("É só um PDF, relaxa.", "relaxa", "negligente")
                        )
                    ),
                    new NodeSpec(
                        "inspecionar",
                        "Isso! Use o inspetor. Não confie só na extensão do arquivo.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "relaxa",
                        "É esse pensamento que os hackers amam. Por favor, inspecione.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Estou de olho.", null, null, true),
                }
            ),
            // ==========================================
            // DIA 5
            // ==========================================
            new ConversationSpec(
                "Dados_Clip",
                "clip_pedido_dados",
                "Clip",
                5,
                10,
                0,
                9,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Parceiro, para fins de calibração, preciso verificar alguns dados. Qual é o seu prato favorito? A pergunta é irrelevante para a segurança, mas ajuda na minha humanização.",
                        Options(
                            Option("Pizza.", "respondeu", "compartilhou_dado_pessoal"),
                            Option(
                                "Por que você quer saber?",
                                "questionou",
                                "questionou_solicitacao"
                            ),
                            Option("Recuso-me a responder.", "recusou", "recusou_dado_pessoal")
                        )
                    ),
                    new NodeSpec(
                        "respondeu",
                        "Pizza. Anotado. Obrigado por contribuir com minha jornada.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "questionou",
                        "Curiosidade algorítmica. Nada mais.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "recusou",
                        "Compreendo. A privacidade é importante.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Até mais!", null, null, true),
                }
            ),
            new ConversationSpec(
                "Encerramento_Odisseu",
                "odisseu_encerramento",
                "Odisseu",
                5,
                15,
                0,
                10,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Chegamos ao fim do expediente. Foi uma experiência... enriquecedora. Você me ensinou coisas que meus criadores não previram. Até amanhã. (Ou não.)",
                        Options(
                            Option("Foi um prazer, Odisseu.", "prazer", "agradeceu"),
                            Option("Você é estranho.", "estranho", "criticou_odisseu"),
                            Option("Adeus.", "adeus", "despediu_se")
                        )
                    ),
                    new NodeSpec(
                        "prazer",
                        "O prazer foi meu. (Literalmente, uma simulação de prazer.)",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "estranho",
                        "Estranho é um conceito humano. Eu apenas... sou.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "adeus",
                        "Adeus. Nos veremos novamente. (Ou não.)",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Fim do expediente.", null, null, true),
                    new NodeSpec(
                        "despedida_final",
                        "Na verdade, tenho uma última pergunta... Você confiaria em mim se eu pedisse sua senha para 'testar o sistema'?",
                        Options(Option("Nunca.", "fim_final", "seguro")),
                        null,
                        true,
                        1.5f,
                        scheduledDay: 5,
                        scheduledHour: 15,
                        scheduledMinute: 30
                    ),
                    new NodeSpec("fim_final", "Sábia escolha.", null, null, true), // CORRIGIDO AQUI
                }
            ),
        };
    }

    private static ChatOption Option(string text, string nextNodeId, string feedbackTag)
    {
        return new ChatOption
        {
            optionText = text,
            nextNodeId = nextNodeId,
            securityScoreDelta = 0,
            feedbackTag = feedbackTag,
        };
    }

    private static List<ChatOption> Options(params ChatOption[] options)
    {
        return options == null ? new List<ChatOption>() : new List<ChatOption>(options);
    }

    private sealed class ConversationSpec
    {
        public readonly string AssetName;
        public readonly string ConversationId;
        public readonly string NpcName;
        public readonly int ArrivalDay;
        public readonly int ArrivalHour;
        public readonly int ArrivalMinute;
        public readonly int SortOrder;
        public readonly NodeSpec[] Nodes;

        public ConversationSpec(
            string assetName,
            string conversationId,
            string npcName,
            int arrivalDay,
            int arrivalHour,
            int arrivalMinute,
            int sortOrder,
            NodeSpec[] nodes
        )
        {
            AssetName = assetName;
            ConversationId = conversationId;
            NpcName = npcName;
            ArrivalDay = arrivalDay;
            ArrivalHour = arrivalHour;
            ArrivalMinute = arrivalMinute;
            SortOrder = sortOrder;
            Nodes = nodes;
        }
    }

    private sealed class NodeSpec
    {
        public readonly string NodeId;
        public readonly string Message;
        public readonly float TypingDelay;
        public readonly List<ChatOption> Options;
        public readonly string NextNodeIdIfNoChoice;
        public readonly bool IsEndNode;

        public readonly int ScheduledDay;
        public readonly int ScheduledHour;
        public readonly int ScheduledMinute;
        public readonly string RequiresAnswerToNodeId;

        public NodeSpec(
            string nodeId,
            string message,
            List<ChatOption> options,
            string nextNodeIdIfNoChoice = null,
            bool isEndNode = false,
            float typingDelay = 1.5f,
            int scheduledDay = -1,
            int scheduledHour = -1,
            int scheduledMinute = -1,
            string requiresAnswerToNodeId = null
        )
        {
            NodeId = nodeId;
            Message = message;
            TypingDelay = typingDelay;
            Options = options;
            NextNodeIdIfNoChoice = nextNodeIdIfNoChoice;
            IsEndNode = isEndNode;
            ScheduledDay = scheduledDay;
            ScheduledHour = scheduledHour;
            ScheduledMinute = scheduledMinute;
            RequiresAnswerToNodeId = requiresAnswerToNodeId;
        }
    }
}

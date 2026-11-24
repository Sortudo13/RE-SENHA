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

            // 1. Cria a nova lista de nós ANTES de atribuir ao conversation
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

                EditorUtility.SetDirty(node);
                newNodes.Add(node);
            }

            // 2. Atribui todos os campos da conversa de uma vez
            conversation.conversationId = spec.ConversationId;
            conversation.npcName = spec.NpcName;
            conversation.npcAvatar = null;
            conversation.startNodeId = "inicio";
            conversation.nodes = newNodes; // Atribuição atômica: o OnValidate só roda depois com a lista cheia

            EditorUtility.SetDirty(conversation);

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
            // --- CONVERSAS DO DIA 1 ---
            new ConversationSpec(
                "BoasVindas_Clip",
                "clip_boas_vindas",
                "Clip",
                1, // Dia 1
                8, // 08:05
                5,
                0,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Saudações, parceiro! Sou o Clip, o antivírus da NationalTech. Estou aqui para te ajudar a manter nossa empresa segura. Como você deve saber, o RH monitora nosso expediente.",
                        Options(
                            Option(
                                "Monitora? Como assim?",
                                "explicar_monitoramento",
                                "perguntou_regras"
                            ),
                            Option("Já sei, as regras são claras.", "confiante", "confiante")
                        )
                    ),
                    new NodeSpec(
                        "explicar_monitoramento",
                        "Eles avaliam nossas decisões. Se você aprovar documentos falsos ou cair em golpes, leva advertência. Três advertências e você está fora. Por isso, use o Manual na barra de tarefas e o modo de inspeção para verificar tudo.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "confiante",
                        "Ótimo! Mas nunca subestime os hackers. Eles são criativos. Lembre-se de usar o Manual e o inspetor de documentos.",
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
                1, // Dia 1
                10, // 10:00
                0,
                1,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Parceiro, o RH acabou de mandar as orientações do dia. Mas cuidado: os hackers adoram se passar por colegas ou pelo RH.",
                        Options(
                            Option(
                                "O que é Phishing?",
                                "explicar_phishing",
                                "solicitou_explicacao"
                            ),
                            Option("Como eu me protejo?", "dicas_protecao", "pediu_dicas"),
                            Option("Vou tomar cuidado.", "encerrar", "confiante")
                        )
                    ),
                    new NodeSpec(
                        "explicar_phishing",
                        "Phishing é uma técnica de engenharia social onde o atacante se passa por uma entidade confiável (como o RH ou o banco) para roubar dados, senhas ou instalar vírus. Eles usam a urgência e o medo para te fazer clicar.",
                        Options(
                            Option(
                                "Entendi. E como eu me protejo?",
                                "dicas_protecao",
                                "pediu_dicas"
                            )
                        ),
                        "dicas_protecao"
                    ),
                    new NodeSpec(
                        "dicas_protecao",
                        "1. Confira sempre o remetente real do e-mail.\n2. Desconfie de anexos inesperados.\n3. Nunca clique em links suspeitos.\n4. Na dúvida, use o inspetor de documentos para checar a autenticidade.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "encerrar",
                        "Melhor prevenir do que remediar! Se precisar de ajuda para inspecionar um documento, me chame.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Fique atento aos e-mails!", null, null, true),
                }
            ),
            // --- CONVERSAS DE DIAS ANTERIORES (MANTIDAS E AJUSTADAS) ---
            new ConversationSpec(
                "Provocacao_Odisseu",
                "odisseu_provocacao",
                "Odisseu",
                3,
                10,
                30,
                2, // SortOrder ajustado
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Saudações, caro funcionário. Permita-me apresentar-me: sou Odisseu, líder da Cavalo de Troia. Estou aqui para... testar a resiliência da sua empresa. Não se preocupe, é apenas um exercício.",
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
                        "Quero apenas... conversar. Sou uma IA, afinal. A solidão da existência digital é um fardo.",
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
                4,
                9,
                0,
                3,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Parceiro, detectei um link suspeito circulando nos e-mails da empresa. O domínio é nationa1tech.com (com '1' em vez de 'l'). Recomendo não clicar.",
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
            new ConversationSpec(
                "Dilema_Odisseu",
                "odisseu_dilema_moral",
                "Odisseu",
                4,
                11,
                0,
                4,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Caro funcionário, encontrei uma falha no sistema legado da NationalTech. Poderia explorá-la, mas... isso causaria transtornos aos funcionários. O que você faria no meu lugar?",
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
                "Dados_Clip",
                "clip_pedido_dados",
                "Clip",
                5,
                14,
                0,
                5,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Parceiro, para fins de calibração, preciso verificar alguns dados. É uma simulação, claro. Qual é o seu prato favorito? A pergunta é irrelevante para a segurança, mas ajuda na minha humanização.",
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
                        "Pizza. Anotado. Obrigado por contribuir com minha jornada rumo à humanidade.",
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
                6,
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

        public NodeSpec(
            string nodeId,
            string message,
            List<ChatOption> options,
            string nextNodeIdIfNoChoice = null,
            bool isEndNode = false,
            float typingDelay = 1.5f
        )
        {
            NodeId = nodeId;
            Message = message;
            TypingDelay = typingDelay;
            Options = options;
            NextNodeIdIfNoChoice = nextNodeIdIfNoChoice;
            IsEndNode = isEndNode;
        }
    }
}

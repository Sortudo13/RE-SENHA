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

            int totalNodes = 0;
            foreach (var spec in specs)
                totalNodes += spec.Nodes.Length;

            Debug.Log(
                $"[ChatContentGenerator] Geradas {specs.Count} conversas, {totalNodes} nós e o ChatInbox."
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
                "BoasVindas_Mousy",
                "mousy_boas_vindas",
                "Mousy",
                1,
                8,
                5,
                0,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Saudações, estimado parceiro! Sou Mousy, seu antivírus residente. Estou aqui para ajudá-lo a manter a NationalTech segura. Permita-me guiá-lo pelas primeiras medidas de proteção.",
                        Options(
                            Option("O que devo fazer primeiro?", "explicar", "perguntou_regras"),
                            Option("Já conheço as regras.", "confiante", "confiante")
                        )
                    ),
                    new NodeSpec(
                        "explicar",
                        "Consulte o Manual antes de decidir sobre mensagens e documentos. Examine remetente, conteúdo e anexos; a pressa é uma velha aliada dos ataques.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "confiante",
                        "Excelente. Ainda assim, até os mais experientes podem ser surpreendidos. Use o Manual e a Inspeção sempre que algo parecer fora do lugar.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Um primeiro dia promissor. Permanecerei à disposição, parceiro.",
                        null,
                        null,
                        true
                    ),
                }
            ),
            new ConversationSpec(
                "Phishing_Mousy",
                "mousy_phishing",
                "Mousy",
                2,
                8,
                0,
                3,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Bom dia, parceiro. Detectei um e-mail inesperado que imita uma comunicação interna. Não abra anexos nem siga links antes de conferir remetente e conteúdo.",
                        Options(
                            Option(
                                "Como reconheço uma imitação?",
                                "explicar",
                                "solicitou_explicacao"
                            ),
                            Option("Quais cuidados devo tomar?", "dicas", "pediu_dicas"),
                            Option("Vou inspecionar antes de agir.", "fim", "prudente")
                        )
                    ),
                    new NodeSpec(
                        "explicar",
                        "Phishing é a tentativa de obter dados ou induzir uma ação fingindo ser alguém confiável. Urgência e familiaridade são disfarces frequentes.",
                        Options(Option("Entendi. E como eu me protejo?", "dicas", "pediu_dicas")),
                        "dicas"
                    ),
                    new NodeSpec(
                        "dicas",
                        "Verifique o endereço completo do remetente, desconfie de anexos inesperados e não clique em links suspeitos. Na dúvida, use a Inspeção e confirme por outro canal.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Uma análise cuidadosa vale mais do que uma resposta apressada.",
                        null,
                        null,
                        true
                    ),
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
                        "Boa tarde. Sou Odisseu. Observo os sistemas da NationalTech há algum tempo; hoje, comecei a observar você.",
                        Options(
                            Option("Quem é você?", "quem", "perguntou_identidade"),
                            Option("O que você quer?", "reportar", "questionou"),
                            Option("Vou reportar esta conversa.", "reportar", "reportou")
                        )
                    ),
                    new NodeSpec(
                        "quem",
                        "Alguém que testa sistemas. E pessoas. Por enquanto, você continua sendo uma incógnita.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "reportar",
                        "Faça como achar necessário. As consequências de uma decisão raramente aparecem no mesmo instante.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Não se distraia com a rotina. Até breve.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "cobranca",
                        "Li sua resposta. Ela me diz menos do que você imagina — e mais do que gostaria.",
                        Options(),
                        null,
                        true,
                        1.5f,
                        scheduledDay: 1,
                        scheduledHour: 15,
                        scheduledMinute: 30,
                        requiresAnswerToNodeId: "inicio"
                    ),
                }
            ),
            new ConversationSpec(
                "Elena_PrimeiroContato",
                "elena_primeiro_contato",
                "Elena Valadares",
                1,
                9,
                0,
                20,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Oi! Você é a pessoa nova da equipe, né? Sou a Elena, do Marketing. Se precisar de café ou de uma dica pra sobreviver às reuniões, me chama :)",
                        Options(
                            Option(
                                "Obrigada! Como está sendo a semana?",
                                "corredor",
                                "conversa_casual"
                            ),
                            Option("Pode deixar, obrigado!", "fim", "agradeceu")
                        )
                    ),
                    new NodeSpec(
                        "corredor",
                        "Corrida! O pessoal de Segurança vive pedindo pra gente confirmar tudo antes de compartilhar arquivos. Eu ainda me confundo com tantos sistemas.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Boa sorte por aí! Até mais :)", Options(), null, true),
                }
            ),
            new ConversationSpec(
                "Wilbar_Comunicado_D1",
                "wilbar_comunicado_d1",
                "Constantino Wilbar",
                1,
                16,
                0,
                21,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Comunicado da Diretoria de Segurança: colaboradores devem registrar ocorrências pelos canais oficiais e preservar mensagens potencialmente suspeitas. Evitem especulações em canais corporativos.",
                        Options(
                            Option("Recebido.", "fim", "confirmou_recebimento"),
                            Option(
                                "Há alguma ocorrência específica?",
                                "pergunta",
                                "perguntou_ocorrencia"
                            )
                        )
                    ),
                    new NodeSpec(
                        "pergunta",
                        "Não há informação adicional autorizada para divulgação. Cumpra o procedimento vigente.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Constantino Wilbar\nDiretor de Segurança",
                        Options(),
                        null,
                        true
                    ),
                }
            ),
            // ==========================================
            // DIA 2
            // ==========================================
            new ConversationSpec(
                "Mousy_Inspetor",
                "mousy_dica_inspetor",
                "Mousy",
                2,
                8,
                30,
                3,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Um documento para aprovação deve chegar hoje. Permita-me uma breve digressão: a Inspeção compara cada campo com as regras do Manual.",
                        Options(
                            Option("Como faço a inspeção?", "explicar", "pediu_explicacao"),
                            Option("Vou conferir os campos e o Manual.", "fim", "confiante")
                        )
                    ),
                    new NodeSpec(
                        "explicar",
                        "Abra o item, ative a Inspeção, selecione o campo relevante e compare-o com a subregra correspondente. Registre evidências antes de bloquear; se não houver violação, aprove.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Uma rotina metódica é uma defesa elegante. Estou por aqui.",
                        null,
                        null,
                        true
                    ),
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
                    new NodeSpec("fim_lembrete", "Até mais!", null, null, true),
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
                        "A NationalTech mantém falhas que prefere chamar de legado. Vou demonstrar uma delas. Não confunda este aviso com uma ameaça; ameaças são menos eficientes.",
                        Options(
                            Option("O que pretende fazer?", "falar", "perguntou"),
                            Option("Vou alertar a Segurança.", "pegar", "desafiou")
                        )
                    ),
                    new NodeSpec(
                        "falar",
                        "Observe as mensagens que chegam. A falha mais útil é aquela que alguém aceita como normal.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "pegar",
                        "Faça isso. Enquanto você redige o aviso, eu continuo trabalhando.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "O tempo é um recurso que não se recupera.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "ataque",
                        "Sua reação foi registrada. Não alterará o que já está em movimento.",
                        Options(
                            Option("O que está em movimento?", "fim_ataque", "questionou_ameaca")
                        ),
                        null,
                        true,
                        1.5f,
                        scheduledDay: 2,
                        scheduledHour: 14,
                        scheduledMinute: 0
                    ),
                    new NodeSpec(
                        "fim_ataque",
                        "Aguarde. Você verá o suficiente.",
                        null,
                        null,
                        true
                    ),
                }
            ),
            new ConversationSpec(
                "Elena_DiaQ_D2",
                "elena_dia_q_d2",
                "Elena Valadares",
                2,
                11,
                15,
                22,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Posso te perguntar uma coisa meio aleatória? Ouvi duas pessoas falando \"Dia Q\" perto do elevador. Quando perguntei, mudaram de assunto. Você sabe o que é?",
                        Options(
                            Option(
                                "Não sei. Melhor não espalhar boato.",
                                "fim",
                                "evitou_especular"
                            ),
                            Option("Talvez seja algo da Segurança.", "comentario", "comentou_dia_q")
                        )
                    ),
                    new NodeSpec(
                        "comentario",
                        "Também pensei nisso. O senhor Wilbar passou por lá e ficou bem sério. Enfim, deve ser coisa confidencial... deixa pra lá.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Se descobrir que era só nome de projeto, me conta! Até :)",
                        Options(),
                        null,
                        true
                    ),
                }
            ),
            new ConversationSpec(
                "Wilbar_Comunicado_D2",
                "wilbar_comunicado_d2",
                "Constantino Wilbar",
                2,
                16,
                0,
                23,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Comunicado de encerramento: o desempenho operacional deste expediente será consolidado pelos registros oficiais. Caso identifique uma decisão incorreta, siga o protocolo de reporte; a repetição de falhas será tratada administrativamente.",
                        Options(
                            Option(
                                "Identifiquei uma falha e seguirei o protocolo.",
                                "advertencia",
                                "reconheceu_falha"
                            ),
                            Option(
                                "Não identifiquei falhas nas minhas decisões.",
                                "elogio",
                                "declarou_acerto"
                            )
                        )
                    ),
                    new NodeSpec(
                        "advertencia",
                        "Fica registrado o alerta: revise o procedimento aplicável antes de decisões futuras. Não discutirei ocorrências individuais por este canal.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "elogio",
                        "Mantenha esse padrão. A conformidade deve ser demonstrável, não presumida.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Constantino Wilbar\nDiretor de Segurança",
                        Options(),
                        null,
                        true
                    ),
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
                        "Vou dispensar as sutilezas: sou Odisseu, líder da organização Cavalo de Troia. Tenho acesso a informações que a sua empresa acredita estarem protegidas. Se continuar investigando, passará a fazer parte do problema.",
                        Options(
                            Option("Isso é uma ameaça?", "provocou", "questionou_ameaca"),
                            Option(
                                "O que você quer da NationalTech?",
                                "perguntou",
                                "perguntou_objetivo"
                            ),
                            Option(
                                "Vou preservar esta conversa e reportá-la.",
                                "reportou",
                                "reportou_odisseu"
                            )
                        )
                    ),
                    new NodeSpec(
                        "provocou",
                        "Chame como quiser. Eu prefiro chamar de previsão.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "perguntou",
                        "Quero que observe o que a sua própria empresa esconde. Não espere que eu explique as consequências.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "reportou",
                        "Registre o que quiser. Um registro não é o mesmo que controle.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "A próxima decisão ainda será sua.", null, null, true),
                }
            ),
            new ConversationSpec(
                "Alerta_Link_Mousy",
                "mousy_alerta_link",
                "Mousy",
                3,
                14,
                0,
                6,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Uma observação de método, parceiro: inspecione o domínio completo, o remetente e o conteúdo antes de decidir. Um endereço semelhante ao oficial ainda pode ser uma falsificação.",
                        Options(
                            Option(
                                "Vou registrar qualquer inconsistência antes de bloquear.",
                                "agradeceu",
                                "prudente"
                            ),
                            Option(
                                "Se parece conhecido, deve ser seguro.",
                                "clicou",
                                "confiou_na_aparencia"
                            ),
                            Option("O que devo comparar?", "explicou", "solicitou_explicacao")
                        )
                    ),
                    new NodeSpec("agradeceu", "De nada. Fique atento.", Options(), "fim"),
                    new NodeSpec(
                        "clicou",
                        "A familiaridade visual não valida um endereço. Suspenda a ação e inspecione o conteúdo.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "explicou",
                        "Compare o domínio letra por letra, verifique o contexto da mensagem e use a subregra adequada do Manual. Registre a evidência antes de bloquear.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "A inspeção cuidadosa é uma disciplina, não um palpite.",
                        null,
                        null,
                        true
                    ),
                }
            ),
            new ConversationSpec(
                "Elena_Pedido_D3",
                "elena_pedido_d3",
                "Elena Valadares",
                3,
                12,
                0,
                24,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Oi, você está ocupado? Chegou um pedido para localizar uns arquivos antigos ligados ao Dia Q. Não sei se posso abrir; o nome da pasta é meio estranho.",
                        Options(
                            Option(
                                "Não abra ainda. Confirme com a Segurança por um canal oficial.",
                                "prudente",
                                "orientou_confirmacao"
                            ),
                            Option(
                                "Me manda o nome e eu vejo o procedimento.",
                                "pergunta",
                                "pediu_contexto"
                            )
                        )
                    ),
                    new NodeSpec(
                        "prudente",
                        "Boa. Vou perguntar ao meu gerente sem clicar em nada. Não quero causar outro daqueles sustos de TI.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "pergunta",
                        "A mensagem diz que é urgente, mas veio de um endereço que não reconheço. Melhor eu confirmar antes de encaminhar qualquer coisa.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Obrigada por me ajudar! E, por favor, não comenta isso no grupo do Marketing ainda.",
                        Options(),
                        null,
                        true
                    ),
                }
            ),
            new ConversationSpec(
                "Wilbar_Alerta_D3",
                "wilbar_alerta_d3",
                "Constantino Wilbar",
                3,
                16,
                0,
                25,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Preciso esclarecer uma questão sobre o chamado Dia Q: você consultou materiais relacionados a esse assunto? A investigação não está autorizada individualmente.",
                        Options(
                            Option(
                                "Consultei. Por que esse material é restrito?",
                                "confronto",
                                "questionou_dia_q"
                            ),
                            Option("Não consultei. Qual é a orientação?", "fim", "acatou_diretriz")
                        )
                    ),
                    new NodeSpec(
                        "confronto",
                        "Já respondi. O Dia Q não é objeto de discussão neste canal. Não transforme curiosidade em insubordinação.",
                        Options(Option("Entendido.", "fim", "encerrou_confronto")),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "A comunicação está encerrada.\nConstantino Wilbar",
                        Options(),
                        null,
                        true
                    ),
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
                "Mousy_Alerta_Anexo",
                "mousy_alerta_anexo",
                "Mousy",
                4,
                14,
                30,
                8,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Parceiro, chegou um anexo inesperado com uma extensão incompatível com o nome exibido. Não o abra por curiosidade: confira o arquivo pela Inspeção.",
                        Options(
                            Option("Vou inspecionar.", "inspecionar", "prudente"),
                            Option(
                                "O nome diz PDF; deve estar tudo bem.",
                                "relaxa",
                                "confiou_na_extensao"
                            )
                        )
                    ),
                    new NodeSpec(
                        "inspecionar",
                        "Muito bem. A extensão real e o tipo de conteúdo importam mais do que o rótulo visível.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "relaxa",
                        "É justamente essa confiança que um atacante espera. Verifique a extensão real antes de qualquer abertura.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Continuarei monitorando os sinais, parceiro.",
                        null,
                        null,
                        true
                    ),
                }
            ),
            new ConversationSpec(
                "Elena_Pedido_Codigo_D4",
                "elena_pedido_codigo_d4",
                "Elena Valadares",
                4,
                14,
                30,
                26,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Oi! Troquei de celular e não consigo entrar no Marketing. Você pode me mandar o código de verificação que acabou de chegar aí? É rapidinho, prometo!",
                        Options(
                            Option(
                                "Não vou compartilhar códigos. Vou confirmar com você por outro canal.",
                                "verificou",
                                "verificou_identidade"
                            ),
                            Option(
                                "Tudo bem, mando o código.",
                                "compartilhou",
                                "compartilhou_codigo"
                            )
                        )
                    ),
                    new NodeSpec(
                        "verificou",
                        "Boa decisão. Eu não pedi código nenhum — alguém está usando meu nome. Vou avisar meu gerente por um canal oficial.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "compartilhou",
                        "Recebido. Obrigada! O acesso voltou a funcionar.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Elena Valadares\n(Conversa não verificada)",
                        Options(),
                        null,
                        true
                    ),
                }
            ),
            new ConversationSpec(
                "Wilbar_Comunicado_D4",
                "wilbar_comunicado_d4",
                "Constantino Wilbar",
                4,
                16,
                0,
                27,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Comunicado urgente: não compartilhem códigos de autenticação nem dados internos por solicitação em chat. A identidade do remetente deve ser confirmada por canal independente. A Diretoria está tratando uma ocorrência.",
                        Options(
                            Option("Qual é a ocorrência?", "restrito", "perguntou_ocorrencia"),
                            Option(
                                "Entendido. Vou seguir o procedimento.",
                                "fim",
                                "confirmou_procedimento"
                            )
                        )
                    ),
                    new NodeSpec(
                        "restrito",
                        "Os detalhes permanecem restritos. Não encaminhe capturas de tela nem especulações. Aguarde instruções oficiais.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Constantino Wilbar\nDiretor de Segurança",
                        Options(),
                        null,
                        true
                    ),
                }
            ),
            // ==========================================
            // DIA 5
            // ==========================================
            new ConversationSpec(
                "Dados_Mousy",
                "mousy_pedido_dados",
                "Mousy",
                5,
                10,
                0,
                9,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Parceiro, chegamos ao último expediente previsto no cronograma. Ainda assim, mantenha seus critérios: mensagens urgentes e pedidos incomuns merecem verificação, mesmo quando vêm de contatos conhecidos.",
                        Options(
                            Option(
                                "Entendido. Vou continuar verificando.",
                                "respondeu",
                                "confirmou_cuidado"
                            ),
                            Option("Você detectou alguma coisa?", "questionou", "questionou_alerta")
                        )
                    ),
                    new NodeSpec(
                        "respondeu",
                        "Agradeço. Sua prudência continua sendo uma variável importante.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "questionou",
                        "Há sinais que ainda não consigo classificar. Não interprete a ausência de diagnóstico como confirmação de segurança.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "fim",
                        "Estarei atento. Por favor, faça o mesmo.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "aviso",
                        "Uma atualização, parceiro: recebi uma sequência de alertas inconclusivos. Não posso confirmar a origem. Se esta mensagem for interrompida, preserve o registro.",
                        Options(),
                        null,
                        true,
                        scheduledDay: 5,
                        scheduledHour: 15,
                        scheduledMinute: 30,
                        requiresAnswerToNodeId: "inicio"
                    ),
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
                        "Você chegou até aqui sem saber o que aconteceu no Dia Q. Não confunda falta de resposta com falta de evidência. A NationalTech sabe mais do que admite; eu também.",
                        Options(
                            Option("O que aconteceu no Dia Q?", "estranho", "questionou_dia_q"),
                            Option("Por que está me contando isso?", "prazer", "questionou_motivo"),
                            Option(
                                "Não vou aceitar sua versão sem evidências.",
                                "adeus",
                                "exigiu_evidencias"
                            )
                        )
                    ),
                    new NodeSpec(
                        "prazer",
                        "Porque você ainda está olhando. Por enquanto, isso basta.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "estranho",
                        "Se eu lhe desse uma resposta, você teria de decidir em quem acreditar.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec(
                        "adeus",
                        "Uma exigência sensata. Guarde-a para quando alguém lhe oferecer uma conclusão conveniente.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Não considere esta conversa encerrada.", null, null, true),
                    new NodeSpec(
                        "despedida_final",
                        "Uma última observação: se a tela ficar em silêncio, não presuma que a conversa acabou.",
                        Options(),
                        null,
                        true,
                        1.5f,
                        scheduledDay: 5,
                        scheduledHour: 15,
                        scheduledMinute: 45,
                        requiresAnswerToNodeId: "inicio"
                    ),
                }
            ),
            new ConversationSpec(
                "Elena_Enigmatica_D5",
                "elena_enigmatica_d5",
                "Elena Valadares",
                5,
                15,
                0,
                28,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Oi... acho que vou sair mais cedo. Me perguntaram de novo sobre uma pasta antiga, mas ninguém explicou por quê. Deve ser só uma confusão do trabalho, né?",
                        Options(
                            Option(
                                "Não sei. Confirme com seu gerente antes de abrir ou enviar qualquer coisa.",
                                "confirmacao",
                                "orientou_confirmacao"
                            ),
                            Option(
                                "Se ninguém explicou, melhor não mexer.",
                                "fim",
                                "recomendou_cautela"
                            )
                        )
                    ),
                    new NodeSpec(
                        "confirmacao",
                        "É, vou falar com ele amanhã. Quer dizer... se eu conseguir falar com ele. Desculpa, estou cansada. Até depois.",
                        Options(),
                        "fim"
                    ),
                    new NodeSpec("fim", "Elena está offline.", Options(), null, true),
                }
            ),
            new ConversationSpec(
                "Wilbar_Comunicado_Final",
                "wilbar_comunicado_final",
                "Constantino Wilbar",
                5,
                16,
                0,
                29,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "COMUNICADO DE EMERGÊNCIA: mantenha os registros disponíveis e não encerre a sessão. A Diretoria está verificando uma ocorrência relacionada ao arquivo de contingência. Aguarde novas instruções...",
                        Options(),
                        null,
                        true
                    ),
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

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

    // Mapeia o nome do NPC para o caminho em Resources (sem extensão).
    // Ex.: "Elena" -> "NPCs/Elena" carrega Assets/Resources/NPCs/Elena.png
    private static readonly Dictionary<string, string> NpcSpriteMap = new()
    {
        { "Elena Valadares", "NPCs/Elena" },
        { "Constantino Wilbar", "NPCs/Constantino" },
        { "Mousy", "NPCs/Mousy" },
        { "Odisseu", "NPCs/Odisseu" },
        { "Bóris", "NPCs/Boris" },
    };

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
                conversation.npcAvatar = LoadSpriteByNpcName(spec.NpcName); // <-- NOVO
                conversation.blocksDayEnd = spec.BlocksDayEnd;
                conversation.RefreshCache();
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

    // Carrega o sprite do NPC a partir de Resources/NPCs/<nome>.png
    private static Sprite LoadSpriteByNpcName(string npcName)
    {
        if (string.IsNullOrEmpty(npcName))
            return null;

        if (!NpcSpriteMap.TryGetValue(npcName, out string path))
        {
            Debug.LogWarning(
                $"[ChatContentGenerator] Nenhum sprite mapeado para o NPC '{npcName}'. "
                    + "Adicione uma entrada em NpcSpriteMap."
            );
            return null;
        }

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning(
                $"[ChatContentGenerator] Sprite não encontrado em Resources/{path}. "
                    + "Verifique se o arquivo existe em Assets/Resources/ e se está importado como Sprite."
            );
        }
        return sprite;
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
                        "Oi! Você é a pessoa nova da equipe, né? Sou a Elena Valadares, do Marketing. Se precisar de café ou de uma dica pra sobreviver às reuniões, me chama :)",
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
                "Boris_Senha_D1",
                "boris_senha_d1",
                "Bóris",
                1,
                11,
                0,
                30,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Oi, Amigo! O Mousy mandou um guia de senhas tão formal e chique que fiquei com vergonha de perguntar: usar \"NationalTech123\" e trocar o final pra 124 todo mês já conta como senha forte?",
                        Options(
                            Option(
                                "Não: a sequência é previsível. Prefira uma senha longa e única por serviço e use o gerenciador aprovado pela empresa.",
                                "tecnico",
                                "boris_senha_tecnico",
                                ChatOptionOutcome.Neutral,
                                shortText: "Não: senha longa e única, no gerenciador."
                            ),
                            Option(
                                "Pensa numa chave diferente pra cada porta: se uma vazar, as outras continuam protegidas. O gerenciador ajuda a lembrar.",
                                "didatico",
                                "boris_senha_didatico",
                                ChatOptionOutcome.Neutral,
                                shortText: "Uma chave diferente pra cada porta."
                            ),
                            Option(
                                "\"124\" é só o \"123\" de bigode. Crie uma senha longa e exclusiva e deixe o gerenciador cuidar da memória.",
                                "pratico",
                                "boris_senha_pratico",
                                ChatOptionOutcome.Neutral,
                                shortText: "\"124\" é só o \"123\" de bigode!"
                            )
                        )
                    ),
                    new NodeSpec(
                        "tecnico",
                        "Entendi: nada de trocar só o número. Vou criar uma senha longa e diferente e guardar no gerenciador aprovado.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "didatico",
                        "Ah, boa! Se cada porta tem uma chave, uma senha vazada não abre todas. Vou usar o gerenciador pra não me enrolar.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "pratico",
                        "Hahaha, senha com bigode não vale! Vou parar com a sequência e usar uma senha longa e única no gerenciador.",
                        null,
                        null,
                        true
                    ),
                },
                blocksDayEnd: false
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
                        "Abra o item e ative a Inspeção pelo botão da lupa na barra de tarefas (ou a tecla Espaço). Depois, selecione o campo relevante e compare-o com a subregra correspondente. Registre evidências antes de bloquear; se não houver violação, aprove.",
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
                "Boris_2FA_D2",
                "boris_2fa_d2",
                "Bóris",
                2,
                9,
                15,
                31,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Amigo, dúvida básica: pra que serve o 2FA? Se alguém descobrir minha senha, eu posso só trocar depois, né?",
                        Options(
                            Option(
                                "O 2FA exige uma segunda prova de identidade, reduzindo o risco se a senha for descoberta. Se vazar, avise a TI e troque-a imediatamente.",
                                "tecnico",
                                "boris_2fa_tecnico",
                                ChatOptionOutcome.Neutral,
                                shortText: "É uma segunda prova de identidade."
                            ),
                            Option(
                                "É como uma segunda fechadura: saber a senha não basta. E, se ela vazar, a gente conta pra TI e troca logo.",
                                "didatico",
                                "boris_2fa_didatico",
                                ChatOptionOutcome.Neutral,
                                shortText: "É como uma segunda fechadura."
                            ),
                            Option(
                                "O código extra é o segurança da porta. Não deixa pra trocar depois: senha suspeita? TI e troca imediata!",
                                "pratico",
                                "boris_2fa_pratico",
                                ChatOptionOutcome.Neutral,
                                shortText: "O código extra é o segurança da porta."
                            )
                        )
                    ),
                    new NodeSpec(
                        "tecnico",
                        "Agora entendi: o 2FA é uma camada extra, não substitui a senha. Vou manter ligado e avisar a TI se ela for exposta.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "didatico",
                        "Ahhh, segunda fechadura! Vou deixar o 2FA ligado e, se minha senha escapar, aviso a TI e troco na hora.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "pratico",
                        "Entendi, não espero o expediente acabar: mantenho o 2FA e chamo a TI se notar algo estranho na senha.",
                        null,
                        null,
                        true
                    ),
                },
                blocksDayEnd: false
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
                                "orientou_confirmacao",
                                shortText: "Não abra. Confirme com a Segurança."
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
                "Boris_Tailgating_D3",
                "boris_tailgating_d3",
                "Bóris",
                3,
                13,
                0,
                32,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Amigo, fui super educado hoje: segurei a porta da sala de servidores pra um cara gente boa de terno que esqueceu o crachá no carro. Fiz bem? Agora tô com medo do diretor Wilbar descobrir e me dar bronca.",
                        Options(
                            Option(
                                "Acesso à sala de servidores exige identificação e autorização. Avise a Segurança ou recepção para validar; relate também que ele já entrou.",
                                "tecnico",
                                "boris_tailgating_tecnico",
                                ChatOptionOutcome.Neutral,
                                shortText: "Sem crachá e autorização, não entra. Avise a Segurança."
                            ),
                            Option(
                                "Ser educado é ótimo, mas cada pessoa precisa usar o próprio crachá. Peça ajuda à recepção e conte que ele passou.",
                                "didatico",
                                "boris_tailgating_didatico",
                                ChatOptionOutcome.Neutral,
                                shortText: "Cada um usa o próprio crachá. Avise a recepção."
                            ),
                            Option(
                                "Gentileza sim; abrir a porta no modo 'visita surpresa', não! Chama a Segurança agora e explica o que rolou.",
                                "pratico",
                                "boris_tailgating_pratico",
                                ChatOptionOutcome.Neutral,
                                shortText: "Gentileza sim, porta aberta não! Chame a Segurança."
                            )
                        )
                    ),
                    new NodeSpec(
                        "tecnico",
                        "Vou comunicar a Segurança agora. Da próxima vez, ninguém entra sem crachá e autorização confirmada.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "didatico",
                        "Saquei: simpatia não confirma identidade. Vou contar à recepção que ele entrou e não seguro mais a porta pra desconhecidos.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "pratico",
                        "Hahaha, justo. Vou avisar a Segurança e na próxima mando o visitante validar o acesso, sem virar porteiro do servidor.",
                        null,
                        null,
                        true
                    ),
                },
                blocksDayEnd: false
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
                "Boris_Wifi_D4",
                "boris_wifi_d4",
                "Bóris",
                4,
                13,
                0,
                33,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Amigo, quero adiantar uns chamados à noite no Wi-Fi livre da padaria da esquina. Tem problema se for só por dez minutinhos?",
                        Options(
                            Option(
                                "Use apenas uma conexão aprovada pela empresa e siga a política de VPN. Dez minutos ainda expõem dados numa rede não confiável.",
                                "tecnico",
                                "boris_wifi_tecnico",
                                ChatOptionOutcome.Neutral,
                                shortText: "Só conexão aprovada, com VPN."
                            ),
                            Option(
                                "Wi-Fi aberto é como deixar o chamado numa mesa pública: use a conexão autorizada pela empresa, com a VPN conforme as regras.",
                                "didatico",
                                "boris_wifi_didatico",
                                ChatOptionOutcome.Neutral,
                                shortText: "Wi-Fi aberto é uma mesa pública."
                            ),
                            Option(
                                "Dez minutos ou dez segundos, a padaria não é nosso escritório! Espera uma rede aprovada e segue as regras da VPN.",
                                "pratico",
                                "boris_wifi_pratico",
                                ChatOptionOutcome.Neutral,
                                shortText: "A padaria não é nosso escritório!"
                            )
                        )
                    ),
                    new NodeSpec(
                        "tecnico",
                        "Entendido: pouco tempo não torna a rede confiável. Vou usar uma conexão aprovada e seguir a política de VPN.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "didatico",
                        "Ah, não quero meu chamado virando papo de balcão! Vou esperar uma conexão autorizada e seguir a política da empresa.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "pratico",
                        "Hahaha, verdade. A padaria fica só pro café; trabalho só em rede aprovada e com os controles exigidos.",
                        null,
                        null,
                        true
                    ),
                },
                blocksDayEnd: false
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
                                "verificou_identidade",
                                shortText: "Não compartilho códigos; confirmo por outro canal."
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
                    new NodeSpec("fim", "Elena\n(Conversa não verificada)", Options(), null, true),
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
                "Boris_Scareware_D5",
                "boris_scareware_d5",
                "Bóris",
                5,
                13,
                0,
                34,
                new[]
                {
                    new NodeSpec(
                        "inicio",
                        "Amigo, caos total aqui: apareceu um pop-up dizendo que meu PC tem 47 vírus e mandando baixar um \"Limpador Oficial\" urgente. Clico agora ou espero acabar o expediente?",
                        Options(
                            Option(
                                "Não clique nem instale. Feche o navegador ou app sem interagir e reporte o alerta à TI pelo canal oficial.",
                                "tecnico",
                                "boris_scareware_tecnico",
                                ChatOptionOutcome.Neutral,
                                shortText: "Não clique: feche e reporte à TI."
                            ),
                            Option(
                                "Um aviso assustador na tela não prova que há vírus. Não toque nele; feche a janela e peça ajuda à TI.",
                                "didatico",
                                "boris_scareware_didatico",
                                ChatOptionOutcome.Neutral,
                                shortText: "Aviso assustador não prova vírus."
                            ),
                            Option(
                                "Quarenta e sete? Que número específico! Não baixa esse 'limpador': fecha a janela e manda o alerta pra TI.",
                                "pratico",
                                "boris_scareware_pratico",
                                ChatOptionOutcome.Neutral,
                                shortText: "47 vírus? Fecha e avisa a TI."
                            )
                        )
                    ),
                    new NodeSpec(
                        "tecnico",
                        "Entendi: não clico, não instalo e aviso a TI pelo canal oficial. Vou fechar a janela sem interagir.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "didatico",
                        "Ah, então o susto pode ser falso! Não vou baixar nada; fecho a janela e peço ajuda à TI.",
                        null,
                        null,
                        true
                    ),
                    new NodeSpec(
                        "pratico",
                        "Hahaha, muito específico mesmo. Nada de clicar: vou fechar a janela e reportar pra TI, sem instalar o tal limpador.",
                        null,
                        null,
                        true
                    ),
                },
                blocksDayEnd: false
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
                                "orientou_confirmacao",
                                shortText: "Confirme com seu gerente antes."
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

    // shortText: resumo mostrado no botao de escolha; o chat envia o text completo.
    // Vazio mostra o text no botao.
    private static ChatOption Option(
        string text,
        string nextNodeId,
        string feedbackTag,
        ChatOptionOutcome outcome = ChatOptionOutcome.Neutral,
        string outcomeFeedback = null,
        string shortText = null
    )
    {
        return new ChatOption
        {
            optionText = text,
            shortText = shortText,
            nextNodeId = nextNodeId,
            outcome = outcome,
            outcomeFeedback = outcomeFeedback,
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
        public readonly bool BlocksDayEnd;

        public ConversationSpec(
            string assetName,
            string conversationId,
            string npcName,
            int arrivalDay,
            int arrivalHour,
            int arrivalMinute,
            int sortOrder,
            NodeSpec[] nodes,
            bool blocksDayEnd = false
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
            BlocksDayEnd = blocksDayEnd;
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

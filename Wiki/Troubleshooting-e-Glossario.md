# Troubleshooting e glossário

## Problemas comuns

| Sintoma | Verificação / ação |
|---|---|
| Chat não abre no primeiro nó ou aponta `startNodeId` inexistente | No asset `ChatConversation`, confirme que `startNodeId` corresponde exatamente a um `nodeId`; valide também `nextNodeId` e `nextNodeIdIfNoChoice`. O `OnValidate` registra erros no Console. |
| Alterei a lista `nodes`, mas o lookup não acompanha | O asset mantém um lookup em cache. Reabra/valide o asset ou chame `ChatConversation.RefreshCache()` após substituir a lista em uma ferramenta de Editor. O gerador atual já chama esse método. |
| Badge de app não aparece | Confirme que emissor e `IconController` usam o mesmo nome de app, que o payload é `string`, que o ícone está ativo/visível naquele dia e que `notificationBadge` está atribuído. O `IconController` tem logs `BadgeDebug`. |
| Badge de e-mail não aparece | Confira a caixa/lista e `UnreadBadge`, além do badge do ícone. Abrir uma mensagem marca-a como lida; `ChatController` reconcilia conversas não lidas no `Start`. |
| Dica do Companion não aparece | Confirme `Resources/Companion/CompanionTips.json`, ID exato, linhas não vazias e que o ID ainda não está em `SaveData.seenTips`. Consulte os avisos `[Companion]` no Console. |
| Companion não anima ou não troca pose | Confira Animator Controller, bool `Visible`, estados tagueados `Shown`/`Hidden`, nomes de poses/estados e `poseSprites` no prefab. `Talking` é opcional. |
| Mudança de JSON não é refletida | `CompanionTips` e `ManualRules` cacheiam conteúdo em runtime. Inicie uma nova sessão de Play para limpar o cache; `Resources.Load` exige o caminho sem extensão. |
| Regra do Manual não carrega | Verifique JSON, `version: 1`, campos exatos, IDs únicos, `app`, `introductionDay`, `check.type` e parâmetros obrigatórios. Erros de chave podem ser reportados por `MissingMemberHandling.Error`. |
| Conversa chegou atrasada e não aparece | Chegadas de `ChatInboxState` não são retroativas. Confira dia, minuto, relógio e `ChatInbox.asset`; valide o teste a partir de um horário anterior à chegada. |
| E-mail acionado pelo chat não chega | O `triggerId` do e-mail deve coincidir com `emailTriggerId` do nó/opção. E-mails com trigger não são enviados pela rotina horária comum. |
| Wallpaper salvo não carrega | `MetaData.currentBackground` guarda o nome do sprite. Restaure o nome/asset em `Resources/Background` ou selecione outro wallpaper. |
| Um ticket não aparece no dia esperado | Confira se existe `Assets/Resources/Tickets/Dia_N.json`; no conteúdo atual só foi encontrado o arquivo do Dia 1. |

## Glossário

| Termo | Significado no jogo |
|---|---|
| NationalTech | Empresa fictícia onde o jogador trabalha como analista. |
| Mousy | Assistente/aliado que conversa com o jogador e oferece dicas de segurança. |
| Odisseu | Personagem presente nas conversas e no conflito narrativo. |
| Advertência / erro | Penalidade por uma decisão incorreta; o padrão de `MaxErrors` é três. |
| Inspetor / modo de inspeção | Modo para comparar um item com regras e selecionar campos/subregras relevantes. |
| Achado / evidência | Violação registrada pelo jogador e associada a um campo/regra do item. |
| Cavalo de Troia | Software malicioso disfarçado de um programa ou arquivo legítimo. |
| Caixa User / Company | Caixas distintas do app de e-mail; as mensagens Company exibem decisões de aprovar/bloquear. |
| `Resources` | Pasta Unity cujos assets podem ser carregados por caminho em runtime. |
| `ScriptableObject` | Asset Unity usado, neste projeto, para dados de conversa, nós e inbox de chat. |

Para detalhes de regras e conteúdo, veja [Sistema de Inspeção](./Sistema-de-Inspecao.md),
[Chat](./Sistema-de-Chat.md) e [E-mails e Tickets](./Emails-e-Tickets.md).

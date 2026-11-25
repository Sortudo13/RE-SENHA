# Ferramentas de Editor e autoria

## Gerar conversas

`Assets/Editor/ChatContentGenerator.cs` registra
**Tools > Re:Senha > Gerar Conversas**. O comando cria/atualiza:

- `Assets/Scriptable Objects/Chat/Conversations/*.asset`;
- `Assets/Scriptable Objects/Chat/Nodes/*.asset`;
- `Assets/Scriptable Objects/Chat/ChatInbox.asset`.

As specs são tipos privados definidos no próprio arquivo C#; não são importadas de
um CSV/JSON. O gerador atualiza valores das conversas e nós e define
`startNodeId = "inicio"` e `npcAvatar = null`. Alterações manuais nos campos que
ele atribui podem ser sobrescritas na próxima geração.

Para alterar o conjunto de conversas padrão, edite `CreateConversationSpecs()` e
use os construtores privados `ConversationSpec` e `NodeSpec`. Para ver um exemplo
mínimo com ramificação, consulte [Sistema de Chat](./Sistema-de-Chat.md).

## Validadores no Editor

| Validador | O que verifica |
|---|---|
| `ChatConversation.OnValidate` | IDs duplicados, `startNodeId`, opções e próximos nós que não existem; também alerta sobre opções avaliadas com `blocksDayEnd` desligado. |
| `ChatInbox.OnValidate` | `conversationId` repetido, dia fora de 1–5 e horário fora da faixa fixa 08:00–16:00. |

Corrija os avisos antes de testar a conversa no runtime. Os assets são referenciados
por GUIDs do Unity; não substitua `.meta` manualmente ao mover conteúdo.

A cena `Desktop` está configurada até 18:00; portanto, o limite de validação do
`ChatInbox` não acompanha o expediente dessa cena. Horários de chat após 16:00
estão **A confirmar**.

## Outros conteúdos

- Dicas: JSON em `Assets/Resources/Companion/CompanionTips.json`; veja
  [Companion e Tutorial](./Companion-e-Tutorial.md).
- E-mails e tickets: JSON por dia em `Assets/Resources/Emails/` e
  `Assets/Resources/Tickets/`; veja [E-mails e Tickets](./Emails-e-Tickets.md).
- Regras de inspeção: `Assets/Resources/Manual/ManualRules.json`; veja
  [Sistema de Inspeção](./Sistema-de-Inspecao.md).
- Wallpapers: sprites em `Assets/Resources/Background/`; veja
  [Metadados e Wallpaper](./Metadados-Wallpaper.md).

Não foi encontrado `WallpaperCatalogGenerator`; a lista de wallpapers é construída
em runtime por `BackgroundList`.

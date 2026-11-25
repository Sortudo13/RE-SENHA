# E-mails e Tickets

## Onde fica o conteúdo

- E-mails por dia: `Assets/Resources/Emails/Dia_1.json` até `Dia_5.json`.
- Tickets: `Assets/Resources/Tickets/Dia_1.json`. O carregador tenta abrir
  `Dia_N.json`; dias sem arquivo começam sem tickets.
- As regras que orientam decisões ficam em
  [`ManualRules.json`](../Assets/Resources/Manual/ManualRules.json).

`DataController.GetDailyDataJson(day)` carrega as listas de `Resources`; o
`SenderController` entrega e-mails conforme o expediente. E-mails da caixa Company
que não chegaram quando o expediente acaba são descartados pelo fluxo de envio.
E-mails User não contam como pendências para liberar o próximo dia.

## Exemplo mínimo de e-mail

```json
{
  "timeToReceive": 10,
  "mailBox": "Company",
  "sender": "suporte@empresa.com.br",
  "subject": "Confirmação de solicitação",
  "body": "Confirme a solicitação pelo canal interno.",
  "attachments": []
}
```

Campos úteis de `EmailData`:

| Campo | Descrição |
|---|---|
| `timeToReceive` | Atraso em minutos de jogo, convertido para tempo real pelo relógio. |
| `hourToReceive`, `minuteToReceive` | Horário de jogo opcional que serve de referência antes do atraso. |
| `mailBox` | `User` ou `Company`. Mensagens Company têm botões de decisão. |
| `triggerId` | Se preenchido, o envio aguarda um sinal `TriggerEmail` com o mesmo ID. |
| `companionTip` | ID de dica do Companion exibida ao abrir a mensagem. |
| `attachments` | Lista de `AttachmentData`: `documentType`, nome, extensão, `linkedName` e `data`. |

`documentType` segue a enumeração `Manual=0`, `Email=1`, `PDF=2`, `PNG=3`,
`RG=4`. `linkedName` pode usar o link TextMeshPro `OpenAttachment|N`; `N` é a
posição do anexo começando em 1. O tipo concreto de `data` é inferido pelas
propriedades do JSON, por exemplo `text` para PDF, `imagePath` para PNG ou
`employeeId` para RG.

### E-mail acionado pelo chat

Inclua `triggerId` no e-mail e use o mesmo texto em `emailTriggerId` de um nó ou
opção. Exemplo de fragmento do e-mail:

```json
{
  "timeToReceive": 0,
  "mailBox": "Company",
  "triggerId": "clip_followup_01",
  "sender": "clip@nationaltech.com",
  "subject": "Conforme conversamos",
  "body": "Aqui está a informação combinada.",
  "attachments": []
}
```

O chat envia o sinal depois do `emailDelay`. E-mails com `triggerId` não são
enviados pela rotina de horário normal.

## Exemplo mínimo de ticket

```json
{
  "id": "TK-2001",
  "requester": "Mariana Souza - Financeiro",
  "body": "Solicito acesso temporário a um relatório para concluir uma tarefa.",
  "reason": "A solicitação está alinhada à função e deve ser limitada ao tempo necessário.",
  "decision": 0
}
```

`decision` é `TicketDecision` serializado como número: `0` = `Approve`, `1` =
`Reject`. `TicketController` compara a escolha, apresenta `reason` como justificativa,
registra acerto/erro e remove o ticket da lista.

## Diferenças e limites importantes

- Decisões de e-mail Company passam pelo Inspector/Manual e exigem evidência para
  bloquear corretamente. Veja [Sistema de Inspeção](./Sistema-de-Inspecao.md).
- Ticket usa a decisão e a justificativa do próprio JSON. `TicketController` não
  publica uma superfície de inspeção.
- Ticket incorreto conta como erro, mas `DayProgressController` ainda não inclui
  tickets em sua checagem de itens pendentes no fim do dia.
- `ManualRules` reconhece `email`, `ticket` e `chat`, mas o Inspector está ligado
  ao e-mail. Tickets e conversas ainda não passam por essa avaliação.

Após editar conteúdo, abra o jogo na cena `Desktop` e teste horários, links de
anexos, decisão e feedback. Consulte [Save](./Sistema-de-Save.md) para saber quando
o conteúdo é copiado para o save do slot.

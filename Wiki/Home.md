# Re:Senha — Wiki

Re:Senha é um serious game de cibersegurança ambientado no desktop fictício da
NationalTech. Durante um expediente, o jogador recebe mensagens e solicitações,
consulta o Manual e decide como agir. O objetivo pedagógico é praticar a leitura
de contexto, a verificação de sinais de fraude e o registro de evidências antes
de tomar uma decisão.

## Comece por público

- **Desenvolvimento:** [Arquitetura](./Arquitetura.md), [Save](./Sistema-de-Save.md)
  e [Troubleshooting e glossário](./Troubleshooting-e-Glossario.md).
- **Design e autoria de conteúdo:** [Chat](./Sistema-de-Chat.md),
  [Companion e Tutorial](./Companion-e-Tutorial.md),
  [E-mails e Tickets](./Emails-e-Tickets.md),
  [Inspeção e regras](./Sistema-de-Inspecao.md) e
  [Ferramentas de Editor](./Editor-Tools.md).
- **Professores e avaliadores:** [Como jogar](#como-jogar), [E-mails e Tickets](./Emails-e-Tickets.md)
  e [Inspeção e regras](./Sistema-de-Inspecao.md).

## Índice técnico

| Página | O que responde |
|---|---|
| [Arquitetura](./Arquitetura.md) | Quais sistemas existem, como se comunicam e quais sinais são importantes? |
| [Sistema de Chat](./Sistema-de-Chat.md) | Como conversas, nós, escolhas e horários são definidos e salvos? |
| [Companion e Tutorial](./Companion-e-Tutorial.md) | Como criar dicas e ligar eventos de jogo ao tutorial? |
| [Sistema de Save](./Sistema-de-Save.md) | Onde o progresso é salvo e como resetar dia ou slot? |
| [Sistema de Inspeção](./Sistema-de-Inspecao.md) | Como regras e evidências são avaliadas? |
| [E-mails e Tickets](./Emails-e-Tickets.md) | Como autorar e-mails, anexos e chamados? |
| [Áudio](./Audio.md) | Como música, efeitos e volumes são controlados? |
| [Metadados e Wallpaper](./Metadados-Wallpaper.md) | Como selecionar e adicionar wallpapers? |
| [Ferramentas de Editor](./Editor-Tools.md) | O que o gerador de conversas altera? |
| [Troubleshooting e glossário](./Troubleshooting-e-Glossario.md) | Como resolver problemas comuns e entender os termos do jogo? |

## Como jogar

1. Inicie pela cena `MainMenu` e comece um jogo ou selecione um slot.
2. No desktop, abra o E-mail, o Chat e o Manual conforme as tarefas chegam.
3. Para e-mails da caixa Company, examine remetente, assunto, corpo e anexos.
   Consulte o Manual e use o modo de inspeção para registrar achados antes de
   bloquear uma mensagem suspeita.
4. Nos tickets, leia o pedido e a justificativa disponível antes de aprovar ou
   rejeitar. No Chat, acompanhe o contexto e escolha uma resposta segura.
5. Na cena Desktop atual, o expediente vai de 08:00 a 18:00 e dura cinco minutos
   reais. Decisões incorretas contam como erros; o limite padrão é três.

O comportamento de fechamento do dia não é idêntico em todos os apps: o código
verifica e-mails da caixa Company e conversas com `blocksDayEnd`; tickets ainda
não fazem parte dessa checagem. Para detalhes e limitações, veja
[E-mails e Tickets](./Emails-e-Tickets.md).

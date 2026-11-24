# Editor Tools

## Gerador de conversas

`Assets/Editor/ChatContentGenerator.cs` registra
**Tools > Re:Senha > Gerar Conversas**. A ferramenta cria ou atualiza:

- `Assets/Scriptable Objects/Chat/Conversations/*.asset`;
- `Assets/Scriptable Objects/Chat/Nodes/*.asset`;
- `Assets/Scriptable Objects/Chat/ChatInbox.asset`.

Ela usa especificações internas para construir conversas, nós, opções e horários,
salva os assets e atualiza o AssetDatabase.

## Validações

`ChatConversation.OnValidate` detecta IDs duplicados, início inexistente e opções
apontando para nós ausentes. `ChatInbox.OnValidate` detecta `conversationId`
duplicado e horários fora dos dias 1–5 ou do expediente.

Ao mudar o formato de conteúdo, atualize o gerador e as validações conjuntamente.


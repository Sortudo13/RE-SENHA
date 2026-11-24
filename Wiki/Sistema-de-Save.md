# Sistema de Save

`DataController` é a fachada estática de persistência:

- `save_1.json`: `SaveData`, progresso diário, e-mails, tickets e `chatStates`.
- `data.json`: `MetaData`, incluindo wallpaper, achievements, trinkets e estatísticas.
- `PlayerPrefs`: dia atual, erros, limites e volumes.

`SavingGame` serializa com Newtonsoft JSON. `LoadingGame` cria dados do dia quando o
arquivo não existe e publica `DayChanged`. `ChatTimeSync.EnsureState` reidrata flags,
nó atual e término das conversas.

Ao alterar um modelo serializável, preserve compatibilidade com saves antigos ou
adicione migração explícita; o conversor de `AttachmentContent` identifica subclasses
por suas propriedades JSON.


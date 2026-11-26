# Sistema de Áudio

`AudioManager` é um singleton `DontDestroyOnLoad` com duas fontes: uma para música
de fundo (BGM) e outra para efeitos (SFX). Ele reage às cenas e aos sinais de
abertura/fechamento de janela, clique e aprovação.

| Configuração | Uso |
|---|---|
| `mainMenuMusic` | Música em loop na cena `MainMenu`. |
| `desktopPlaylist` | Lista tocada sequencialmente na cena `Desktop`. |
| `shufflePlaylist` | Embaralha a playlist; quando ativa, evita repetir a faixa imediatamente anterior se houver mais de uma. |
| `silenceBetweenTracks` | Intervalo de silêncio entre faixas da playlist. |
| `fadeOutDuration` | Fade ao final de cada faixa da playlist. |
| Clips de UI/GameOver/GameWin | SFX definidos no Inspector do objeto do manager. |

Volumes de BGM e SFX ficam nas chaves `MusicVolume` e `SFXVolume` de `PlayerPrefs`,
limitados a 0–1. Os padrões do manager são 0,3 para música e 1,0 para efeitos.
`MusicSliderController` lê a preferência de música e chama
`AudioManager.SetMusicVolume`. O manager também oferece `SetSFXVolume`; o projeto
tem um slider para música, mas não um controlador equivalente para SFX.

## Adicionar ou depurar música

1. Atribua clips nos campos do `AudioManager` do prefab/objeto de cena que cria o
   singleton.
2. Verifique se há mais de uma instância do manager; instâncias posteriores são
   destruídas.
3. Teste as transições `MainMenu` → `Desktop` → cena final e pausa (`Time.timeScale`).
   O fade usa tempo não escalado, mas o intervalo entre faixas usa
   `WaitForSeconds`.

Registre as licenças e atribuições das faixas antes de distribuir o jogo.

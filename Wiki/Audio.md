# Áudio

`AudioManager` é singleton e usa `DontDestroyOnLoad`. A cena `MainMenu` usa
`mainMenuMusic`; `Desktop` usa `desktopPlaylist`; `GameOver` e `GameWin` tocam SFX
específicos. O manager também escuta sinais de abertura/fechamento de janela,
cliques e aprovação.

Os volumes ficam nas chaves `MusicVolume` e `SFXVolume` de `PlayerPrefs`, são
limitados ao intervalo 0–1 e podem ser alterados por `MusicSliderController`.

Para adicionar uma faixa, atribua o `AudioClip` no Inspector ao campo apropriado e
teste a transição de cena, o silêncio entre faixas e o fade.


# Metadados e Wallpaper

`BackgroundList` usa `Resources.LoadAll<Sprite>("Background")` para construir a
lista de escolha. `BackgroundButton` publica `SetWallpaper` com o sprite escolhido.
`WallpaperController` aplica o sprite à `Image`, grava seu nome em
`DataController.CurrentMetaData.currentBackground` e chama `SaveMetaData`.

Na inicialização, o controller tenta carregar o mesmo nome de
`Resources/Background/{nome}`. Portanto, renomear ou mover um wallpaper pode deixar
um metadado antigo sem correspondência; nesse caso o sprite configurado no objeto
permanece como fallback visual.


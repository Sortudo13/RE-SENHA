# Metadados e Wallpaper

`BackgroundList` carrega sprites por
`Resources.LoadAll<Sprite>("Background")` e cria uma opção para cada sprite.
`BackgroundButton` envia `SetWallpaper` com o sprite escolhido;
`WallpaperController` troca a imagem e grava `sprite.name` em
`MetaData.currentBackground`, salvando `data.json`.

## Adicionar um wallpaper

1. Adicione uma imagem em `Assets/Resources/Background/`.
2. Configure a importação como Sprite 2D (e ajuste recorte se houver múltiplos
   sprites na mesma imagem).
3. Entre no jogo e confirme que a opção aparece e que a escolha persiste ao
   reiniciar.

Na inicialização, o wallpaper salvo é resolvido por
`Resources.Load<Sprite>("Background/<nome>")`. Se o arquivo for renomeado/removido,
o nome persistido deixa de corresponder; mantenha o sprite original ou escolha
outro wallpaper no jogo.

## Outros metadados

`data.json` também contém `achievements`, `unlockedTrinkets` e `statistics`.
`StatisticsData` está vazio e esses campos ainda não fazem parte de um fluxo de
coleta ou desbloqueio do jogo.

Consulte [Sistema de Save](./Sistema-de-Save.md) para escopo e localização dos
arquivos.

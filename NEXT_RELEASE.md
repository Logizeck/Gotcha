# Prossima release — modifiche annotate (da NON implementare ancora)

In attesa del feedback sui livelli esistenti e delle nuove istruzioni.

## Livello 2 — Tilt Maze
- Pareti del labirinto meno "spigolose": angoli e terminali arrotondati invece delle barre smussate.
- Colori e resa da **tubi al neon**: tubo luminoso con nucleo chiaro e alone colorato diffuso,
  mantenendo la palette del logo (giallo, magenta, ciano).
- Dove intervenire: `Assets/Editor/Art/DoodleSprites.cs` (`WallPiece`) ed eventualmente le giunzioni
  agli angoli in `Assets/Editor/GotchaProjectBuilder.cs` (`Wall`, `Rect`).

# Gotcha! — Foundation v0.1

Scheletro del progetto (solo codice, nessuna grafica). Unity 2022.3 LTS, C#.

## Come usarlo
1. Unity Hub > New project > **2D (Built-in Render Pipeline)**, versione 2022.3 LTS.
2. Project Settings > Player > **Active Input Handling = Input Manager (Old)** (default del template).
3. Copia la cartella `Assets/Scripts` dentro il tuo `Assets/`.
4. Scena, prefab e UI arriveranno col livello pupazzo di neve (prossimo passo).

## Struttura
- `Core/GameFlow` regista: carica livelli, salva progressi, gestisce le pause pubblicitarie.
- `Core/PointerInput`, `Core/SaveData` utilità touch/mouse e salvataggio.
- `Sensors/SensorHub` unico accesso ai sensori (accelerometro, giroscopio, bussola, microfono, volume, batteria).
- `Ads/AdBreakManager` regola 5 livelli / 120 s di gioco attivo; `LogAdProvider` simula gli annunci in Editor.
- `Levels/LevelController` base di ogni livello; `LevelDefinition` e `LevelCatalog` dati dei livelli.
- `Components/` Draggable2D, DropSlot, PinchScalable, TiltGravity, GoalZone, IdleTimer, SensorCondition, EasterEggTrigger.

## Regola pubblicità
Un livello solo "superato" aumenta il contatore livelli. Il tempo conta solo mentre si gioca (non in pausa, menu o app in background).
La pubblicità parte solo in `GameFlow.Continue()`, cioè tra un livello e l'altro; dopo averla mostrata entrambi i contatori si azzerano.
Se l'annuncio non è pronto, si riprova al livello successivo.

## Non ancora fatto
AdMob reale + consenso UMP, UI, scena, livello pupazzo di neve, registrazione/condivisione video, sensori luce e prossimità.

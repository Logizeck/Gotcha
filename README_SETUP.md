# Gotcha! — v0.4 (stile doodle)

Progetto Unity 2022.3 LTS o Unity 6, C#, 2D (Built-in Render Pipeline).
Grafica in stile cartone "doodle" generata da codice (nessuna immagine esterna da importare).

## Come aprirlo
1. Clona o scarica questo repository, poi Unity Hub > **Add project from disk** > seleziona la cartella del
   repository (con Android Build Support, OpenJDK, Android SDK & NDK). Non creare un progetto nuovo.
2. Alla prima apertura `Assets/Editor/GotchaProjectBuilder.cs` genera da solo: sprite doodle
   (`Assets/Art/Generated`), materiali, prefab riutilizzabili, prefab dei livelli, LevelDefinition, LevelCatalog,
   scena `Assets/Scenes/Main.unity` e impostazioni Android (verticale, ARM64 + ARMv7, IL2CPP).
   Con Unity 6 imposta anche Active Input Handling = Both e chiede di riavviare.
   Si può rilanciare da **Gotcha > Build Scene and Levels** (ricrea scena, sprite e prefab generati).
3. Apri `Assets/Scenes/Main.unity` e premi Play. Nell'Editor il mouse sostituisce il dito
   (rotella = pizzico, frecce = inclinazione).
4. APK: File > Build Settings > Android > Switch Platform > Build (o Build And Run col telefono collegato).
   Package name provvisorio: `com.gotcha.game` (da cambiare prima della pubblicazione).

Dopo la prima generazione conviene fare commit anche dei file `.meta` e degli asset generati.

## Grafica
- `Editor/Art/DoodlePainter.cs` piccolo motore di disegno in C# puro: forme con contorno a inchiostro tremolante,
  ombre a mezzaluna, tratteggi. `Editor/Art/DoodleSprites.cs` contiene tutti i disegni (chiave, porta, pupazzo,
  carota, pareti al neon, sfondi...). Per cambiare un disegno si modifica lì e si rilancia il builder.
- `DoodleWobble` fa "vibrare" i disegni come nei cartoni animati fatti a mano.
- Pareti del labirinto ispirate al logo: barre smussate giallo/magenta con neon ciano (colori in `DoodleSprites`).
- Font: Luckiest Guy (titoli, Apache 2.0) e Patrick Hand (testi, SIL OFL), in `Assets/Resources/Fonts` con licenze.

## Struttura
- `Core/GameFlow` regista: carica livelli, salva progressi, pausa, pause pubblicitarie, schermo sempre acceso
  nei livelli che non usano il touch (`LevelDefinition.keepScreenAwake`).
- `Core/PointerInput`, `Core/SaveData` utilità touch/mouse e salvataggio.
- `Sensors/SensorHub` unico accesso ai sensori.
- `Ads/AdBreakManager` regola 5 livelli / 120 s di gioco attivo; `LogAdProvider` simula gli annunci in Editor.
- `Levels/` LevelController, LevelDefinition, LevelCatalog e i livelli KeyDoor, Maze, Snowman.
- `Components/` Draggable2D (con limite allo schermo), DropSlot, PinchScalable, TiltGravity, GoalZone, IdleTimer,
  SensorCondition, EasterEggTrigger, LockedDoor, CameraFit, DoodleWobble, Pulse, StrokeDrawer (disegno col dito),
  SmokeEmitter/SmokePuff (fumo), SpeechBubble (fumetto), ChromeBall (sfera cromata che riflette l'ambiente).
- `Shaders/ChromeBall.shader` riflesso sferico + luce speculare.
- Prefab riutilizzabili in `Assets/Prefabs`: Key, Door, Carrot, Coal.
- `UI/GameUI` interfaccia costruita da codice in stile doodle.

## Livello 1 — Pinch & Open
- La chiave è enorme: va rimpicciolita col pizzico e trascinata con la punta nella serratura.
- Se la lasci altrove resta dove l'hai lasciata. Se è ancora troppo grande la porta trema.
- Con la chiave nella serratura la chiave gira, la porta si apre e il livello è superato.

## Livello 2 — Tilt Maze
- La sfera cromata parte fuori dal labirinto e va portata alla stella al centro inclinando il telefono.
- Risposta all'inclinazione più reattiva (TiltGravity: `strength`, `response`). Lo schermo non va in stand-by.
- La sfera riflette le pareti che ha intorno (camera di riflesso + shader).

## Livello 3 — Snowman
- Occhi (carboni) e naso (carota) vanno trascinati dove vuoi sulla faccia; la bocca si disegna col dito sulla faccia.
  Nessuna posizione è suggerita. Gli oggetti lasciati fuori dalla faccia cadono nella neve.
- Easter egg `snowman_butt`: avvicina la carota al sedere del pupazzo. Arrossisce, fuma, dice "Oh noooo!" e si
  scioglie lentamente in una pozzanghera.

## Regola pubblicità
Un livello solo "superato" aumenta il contatore livelli. Il tempo conta solo mentre si gioca (non in pausa, menu o app in background).
La pubblicità parte solo in `GameFlow.Continue()`, cioè tra un livello e l'altro; dopo averla mostrata entrambi i contatori si azzerano.
Se l'annuncio non è pronto, si riprova al livello successivo.

## Non ancora fatto
Suoni, altri livelli, AdMob reale + consenso UMP, registrazione/condivisione video, icona dell'app.

# Gotcha! — v0.3 (tre livelli giocabili)

Progetto Unity 2022.3 LTS, C#, 2D (Built-in Render Pipeline). Grafica ancora segnaposto, generata da codice.

## Come aprirlo
1. Unity Hub > **Add project from disk** > seleziona questa cartella `Gotcha/` (Unity 2022.3 LTS o Unity 6,
   con Android Build Support). Non creare un progetto nuovo: va aperta proprio questa cartella.
2. Alla prima apertura lo script `Assets/Editor/GotchaProjectBuilder.cs` genera da solo sprite segnaposto,
   prefab (Chiave, Porta, livelli), LevelDefinition, LevelCatalog, scena `Assets/Scenes/Main.unity` e impostazioni
   Android (verticale, 64 bit ARM64 + ARMv7, IL2CPP). Con Unity 6 imposta anche Active Input Handling = Both
   e chiede di riavviare. Si può rilanciare da **Gotcha > Build Scene and Levels**.
   Attenzione: ricrea scena e prefab generati (le modifiche fatte a mano a quei file si perdono).
3. Apri `Assets/Scenes/Main.unity` e premi Play. Nell'Editor il mouse sostituisce il dito
   (rotella = pizzico, frecce = inclinazione).
4. APK: File > Build Settings > Android > Switch Platform > Build.
   Package name provvisorio: `com.gotcha.game` (da cambiare prima della pubblicazione).

Dopo la prima generazione conviene fare commit anche dei file `.meta` e degli asset generati.

## Struttura
- `Core/GameFlow` regista: carica livelli, salva progressi, pausa (ferma `Time.timeScale`), pause pubblicitarie.
- `Core/PointerInput`, `Core/SaveData` utilità touch/mouse e salvataggio.
- `Sensors/SensorHub` unico accesso ai sensori (accelerometro, giroscopio, bussola, microfono, volume, batteria).
- `Ads/AdBreakManager` regola 5 livelli / 120 s di gioco attivo; `LogAdProvider` simula gli annunci in Editor.
- `Levels/LevelController` base di ogni livello; `LevelDefinition` e `LevelCatalog` dati dei livelli.
- `Levels/KeyDoor/KeyDoorLevel`, `Levels/Maze/MazeLevel`, `Levels/Snowman/SnowmanLevel` livelli 1, 2, 3.
- `Components/` Draggable2D, DropSlot (con `maxItemScale`: rifiuta oggetti troppo grandi), PinchScalable, TiltGravity,
  GoalZone, IdleTimer, SensorCondition, EasterEggTrigger, LockedDoor (porta con serratura), CameraFit (adatta la
  camera a qualsiasi proporzione dello schermo).
- Prefab riutilizzabili: `Assets/Prefabs/Key.prefab` (trascinabile + pizzico, pivot sulla punta) e
  `Assets/Prefabs/Door.prefab` (anta che si apre, serratura che accetta solo chiavi piccole).
- `UI/GameUI` interfaccia costruita da codice: barra (MENU, titolo, HINT), pannello Hint, Menu (livelli sbloccati,
  conteggio Easter egg), esito livello (SOLVED! / GOTCHA!), fine gioco. Il tasto Indietro di Android apre/chiude il menu.
- `Editor/GotchaProjectBuilder` genera scena, prefab e asset.

## Livello 1 — Pinch & Open
- La chiave è enorme: va rimpicciolita col pizzico a due dita (rotella nell'Editor) e poi trascinata con la punta
  nella serratura. Se è ancora troppo grande la porta trema e la chiave torna sul pavimento.
- Con la chiave nella serratura la chiave gira, la porta si apre e il livello è superato.

## Livello 2 — Tilt Maze
- La sfera parte fuori dal labirinto (in basso a sinistra) e va portata al centro luminoso inclinando il telefono
  (accelerometro; frecce nell'Editor). Tre anelli con varchi alternati e due sbarramenti che obbligano al giro lungo.

## Livello 3 — Snowman
- Obiettivo: trascinare sulla testa del pupazzo i due occhi di carbone, la carota (naso) e la bocca. Con tutti
  gli slot pieni il pupazzo salta e il livello è superato.
- Easter egg `snowman_bunny`: se porti la carota al coniglio, se la mangia. È un fallimento divertente:
  l'Easter egg viene registrato e il livello si riprova.

## Regola pubblicità
Un livello solo "superato" aumenta il contatore livelli. Il tempo conta solo mentre si gioca (non in pausa, menu o app in background).
La pubblicità parte solo in `GameFlow.Continue()`, cioè tra un livello e l'altro; dopo averla mostrata entrambi i contatori si azzerano.
Se l'annuncio non è pronto, si riprova al livello successivo.

## Non ancora fatto
Grafica definitiva, suoni, altri livelli, AdMob reale + consenso UMP, registrazione/condivisione video,
sensori luce e prossimità.

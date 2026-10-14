# Gotcha! — v0.7 (cinque livelli, musica, pubblicità simulata)

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
   Quando un aggiornamento cambia grafica o livelli (`BuilderVersion` nel builder), all'apertura del progetto
   la rigenerazione parte da sola.
3. Apri `Assets/Scenes/Main.unity` e premi Play. Nell'Editor il mouse sostituisce il dito
   (rotella = pizzico, frecce = inclinazione).
4. APK: menu **Gotcha > Build Android APK** (oppure **Gotcha > Build And Run on Phone** col telefono collegato).
   Meglio di File > Build Settings > Build: da quella finestra Unity a volte scrive in Console
   "GUI Error: Invalid GUILayout state in BuildPlayerWindow" e "ArgumentException: Getting control 1's position...",
   un bug noto e innocuo dell'Editor che dal menu Gotcha non si presenta.
   Package name provvisorio: `com.gotcha.game` (da cambiare prima della pubblicazione).

Dopo la prima generazione conviene fare commit anche dei file `.meta` e degli asset generati.

## Grafica
- `Editor/Art/DoodlePainter.cs` piccolo motore di disegno in C# puro: forme con contorno a inchiostro tremolante,
  ombre a mezzaluna, tratteggi. `Editor/Art/DoodleSprites.cs` contiene tutti i disegni (chiave, porta, pupazzo,
  carota, pareti al neon, sfondi...). Per cambiare un disegno si modifica lì e si rilancia il builder.
- `DoodleWobble` fa "vibrare" i disegni come nei cartoni animati fatti a mano.
- Pareti del labirinto: tubi al neon con estremità arrotondate (ciano, magenta, giallo come il logo), `DoodleSprites.NeonTube`.
- Font: Luckiest Guy (titoli, Apache 2.0) e Patrick Hand (testi, SIL OFL), in `Assets/Resources/Fonts` con licenze.

## Struttura
- `Core/GameFlow` regista: carica livelli, salva progressi, pausa, pause pubblicitarie. Mentre il gioco è aperto
  lo schermo non si spegne e non abbassa la luminosità.
- `Core/PointerInput`, `Core/SaveData` utilità touch/mouse e salvataggio.
- `Sensors/SensorHub` unico accesso ai sensori.
- `Ads/AdBreakManager` regola 5 livelli / 120 s di gioco attivo; `LogAdProvider` simula gli annunci in Editor.
- `Levels/` LevelController, LevelDefinition, LevelCatalog e i livelli KeyDoor, Maze, Snowman, Idle, Apple.
- `Components/` Draggable2D (con limite allo schermo), DropSlot, PinchScalable, TiltGravity, GoalZone, IdleTimer,
  SensorCondition, EasterEggTrigger, LockedDoor, CameraFit, DoodleWobble, Pulse, StrokeDrawer (disegno col dito),
  SmokeEmitter/SmokePuff (fumo, Z, cuori, succo), SwipeCutter (taglio col dito), SpeechBubble (fumetto), ChromeBall (sfera cromata che riflette l'ambiente).
- `Shaders/ChromeBall.shader` riflesso sferico + luce speculare.
- Prefab riutilizzabili in `Assets/Prefabs`: Key, Door, Carrot, Coal.
- `UI/GameUI` interfaccia costruita da codice in stile doodle.

## Livello 1 — Pinch & Open
- La chiave è enorme: va rimpicciolita col pizzico e trascinata con la punta nella serratura.
- Se la lasci altrove resta dove l'hai lasciata. Se è ancora troppo grande la porta trema.
- Con la chiave nella serratura la chiave gira, la porta si apre e il livello è superato.

## Livello 2 — Tilt Maze
- Una biglia d'acciaio parte fuori dal labirinto e va portata alla stella al centro inclinando il telefono.
- Pareti a tubi al neon arrotondati (anche i collider sono a capsula, così la biglia scorre morbida sugli spigoli):
  recinto e anello interno ciano, primo anello magenta, secondo anello giallo.
- Sensibilità: TiltGravity `strength` 41.4 (-10% rispetto alla v0.5), `response` 0.5; poco attrito, scia luminosa.
- Aspetto realistico calcolato dallo shader `Gotcha/ChromeBall`: al centro riflette il "soffitto" (luce di studio
  con softbox), ai bordi le pareti al neon che ha intorno (camera di riflesso), fresnel, due luci speculari,
  bordo di contatto scuro. Sotto c'è un'ombra morbida.

## Livello 3 — Snowman
- Pupazzo in posa di 3/4 (guarda a sinistra): il sedere sporge sul retro, a destra in basso.
- Occhi (carboni) e naso (carota) restano attaccati dove li lasci sulla testa; altrove cadono nella neve.
  Nessuna posizione è indicata. Per vincere ogni occhio deve stare nella sua zona (ampia e invisibile)
  e il naso al centro; la bocca si disegna col dito e resta come un solco nella neve: deve essere un sorriso.
- Easter egg `snowman_butt`: lascia la carota sul sedere. La carota resta piantata lì; il pupazzo arrossisce,
  fuma, dice "Oh noooo!" e si scioglie lentamente dalla testa in giù; la base resta a terra in una pozzanghera.
  Passarci sopra trascinando o farla cadere non fa scattare niente: conta solo dove la lasci.
- Zone e misure sono regolabili dall'Inspector del prefab `Level_Snowman`.

## Livello 4 — Big Red Button
- Il gatto è sveglio e miagola: ha fame. Bisogna prendere il pesce dal quadro e darglielo.
- Dopo aver mangiato si addormenta: a quel punto vince chi aspetta 5 secondi senza toccare lo schermo e senza
  muovere il telefono. Ogni tocco o scossone lo sveglia e fa ripartire l'attesa.
  Il pulsantone rosso "PRESS ME" è solo un'esca.

## Livello 5 — Slice It!
- Taglia la mela col dito: il gesto deve entrare nella mela e uscire dall'altra parte (un taglio deciso).
  La mela si divide in due metà orientate come il taglio, che cadono sul tagliere con qualche goccia di succo.

## Musica
- Colonna sonora `Assets/Audio/Soundtrack.wav` in loop (importata compressa Vorbis). Si spegne/accende dal MENU
  (la scelta resta salvata). Durante la pubblicità la musica è in pausa.

## Regola pubblicità
- Timer invisibile: ogni 3 minuti di gioco attivo (non in pausa, menu o app in background) la pubblicità parte
  alla fine del livello in cui il timer è scaduto.
- Altrimenti parte dopo 5 livelli superati dall'ultima pubblicità mostrata.
- Ogni pubblicità azzera sia il timer sia il conteggio dei livelli.
- Per ora è simulata (`Ads/SimulatedAdProvider`): schermata intera, il tasto di chiusura si attiva dopo 3 secondi.
  Per passare ad AdMob basterà un nuovo `IAdProvider`.

## Non ancora fatto
Effetti sonori, altri livelli, AdMob reale + consenso UMP, registrazione/condivisione video, icona dell'app.

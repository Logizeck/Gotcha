# Gotcha! — v0.13 (trentuno livelli)

La versione installata si legge nel MENU del gioco (accanto al conteggio degli Easter egg).

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
- `Levels/` LevelController, LevelDefinition, LevelCatalog e i livelli KeyDoor, Maze, Snowman, Idle, Apple, Morse, Bomb, Volume, Hold, Wasp.
- `Components/` Draggable2D (con limite allo schermo), DropSlot, PinchScalable, TiltGravity, GoalZone, IdleTimer,
  SensorCondition, EasterEggTrigger, LockedDoor, CameraFit, DoodleWobble, Pulse, StrokeDrawer (disegno col dito),
  SmokeEmitter/SmokePuff (fumo, Z, cuori, succo), SwipeCutter (taglio col dito), TapButton (pulsanti nel mondo di gioco), Tremble (tremolio), WingFlap (battito d'ali), SpeechBubble (fumetto), ChromeBall (sfera cromata che riflette l'ambiente).
- `Shaders/ChromeBall.shader` riflesso sferico + luce speculare.
- Prefab riutilizzabili in `Assets/Prefabs`: Key, Door, Carrot, Coal.
- `UI/GameUI` interfaccia costruita da codice in stile doodle.

## Livello 1 — Pinch & Open
- La chiave è enorme: va rimpicciolita col pizzico e trascinata con la punta nella serratura.
- Se la lasci altrove resta dove l'hai lasciata. Se è ancora troppo grande la porta trema.
- Con la chiave nella serratura la chiave gira, la porta si apre e il livello è superato.

## Livello 2 — Tilt Maze
- Una biglia d'acciaio parte fuori dal labirinto e va portata alla stella al centro inclinando il telefono.
- Pareti al neon disegnate come un'unica immagine (`DoodleSprites.MazeNeon`): tratti dritti, angoli arrotondati e
  varchi hanno esattamente lo stesso tubo, senza giunture. La disposizione (`MazeRings`, `MazeBlocks`) è condivisa
  con il builder, che crea pareti fisiche invisibili identiche (capsule e archi con bordo arrotondato).
  Recinto e anello interno ciano, primo anello magenta, secondo anello giallo.
- Sensibilità: TiltGravity `strength` 37.3 (ridotta due volte del 10%), `response` 0.5; poco attrito, scia luminosa.
- Aspetto realistico calcolato dallo shader `Gotcha/ChromeBall`: al centro riflette il "soffitto" (luce di studio
  con softbox), ai bordi le pareti al neon che ha intorno (camera di riflesso), fresnel, due luci speculari,
  bordo di contatto scuro. Sotto c'è un'ombra morbida.

## Livello 3 — Snowman
- Pupazzo dalle sembianze umane (gambe tozze, fianchi, busto) con una testa enorme e buffa, in posa di 3/4
  (guarda a sinistra). Il sedere è solo accennato dalla curva dei fianchi sul retro.
- Occhi (carboni) e naso (carota), più grandi del normale, restano attaccati dove li lasci sulla testa;
  altrove cadono nella neve. Nessuna posizione è indicata: ogni occhio deve stare nella sua zona (ampia e
  invisibile) e il naso al centro; la bocca si disegna col dito e resta come un solco: deve essere un sorriso.
- Easter egg `snowman_butt`: lascia la carota sul sedere. Resta piantata lì; il pupazzo arrossisce, trema,
  grida "Oh noooo!": gli occhi (se li avevi messi) schizzano via in direzioni opposte e la bocca disegnata diventa
  una "O" di stupore. Poi si scioglie DEL TUTTO dall'alto in basso (`Components/SnowmanMelter`): una maschera fa
  sparire la neve, sul bordo scorre una fascia di neve bagnata con le colature larga quanto il pupazzo, gocciola,
  fuma, e alla fine resta solo una pozzanghera con cappello, braccia e carota che galleggiano.
- La scena viene registrata e a fine livello compare **SHARE VIDEO**: crea un MP4 (salvato in Movies/Gotcha) e apre
  il menu di condivisione di Android. Solo su Android; nell'Editor i fotogrammi finiscono in `persistentDataPath/clip_frames`.
- Zone, misure e punti d'appoggio sono regolabili dall'Inspector del prefab `Level_Snowman`.

## Livello 4 — Big Red Button
- Il gatto è sveglio e miagola: ha fame. Bisogna prendere il pesce dal quadro e darglielo.
- Dopo aver mangiato si addormenta: a quel punto vince chi aspetta 5 secondi senza toccare lo schermo e senza
  muovere il telefono. Ogni tocco o scossone lo sveglia e fa ripartire l'attesa.
  Il pulsantone rosso "PRESS ME" è solo un'esca.

## Livello 5 — Slice It!
- Taglia la mela col dito: il gesto deve entrare nella mela e uscire dall'altra parte (un taglio deciso).
  La mela si divide in due metà orientate come il taglio, che cadono sul tagliere con qualche goccia di succo.

## Livello 6 — Beep Beep
- La radio suona a caso uno di cinque segnali: `...` (tre brevi), `---` (tre lunghi), `.--`, `-.-`, `.-.`.
  Bisogna premere il tasto col disegno corrispondente. Toccando la radio il segnale si riascolta.
- Tasto sbagliato: ronzio, tutto trema e GOTCHA! (si riprova con un nuovo segnale a caso).
- La spia della radio si accende insieme ai toni (si gioca anche col telefono silenzioso); la musica si abbassa
  mentre suonano i segnali. Toni generati da codice (sinusoide 760 Hz).

## Livello 7 — Tick Tock
- Pulsante blu e pulsante rosso, due cavi gialli intrecciati a treccia: si scambiano di lato più volte con incroci
  netti (quasi perpendicolari) ma non corrono mai uno sopra l'altro per più di ~4 mm (ogni disposizione casuale
  viene controllata e scartata se i cavi si sovrappongono troppo). Uno finisce libero (fili di rame sfilacciati,
  all'altezza del timer), l'altro entra nel timer della dinamite. Il timer parte da 10 secondi.
- Accanto al timer un tizio in camicia guarda il giocatore in preda al panico: trema e suda sempre di più man mano
  che il tempo scende; se la bomba viene disinnescata tira un sospiro di sollievo.
- Il pulsante collegato al timer lo ferma ("Phew!"). Quello sbagliato o il tempo scaduto: BOOM! e GOTCHA!
  Ogni tentativo ridisegna i cavi e sceglie a caso quale pulsante è quello giusto.

## Livello 8 — Shhh!
- Lo stereo suona a tutto volume e il gatto (lo stesso del livello 4) è furioso: soffia, fuma dalle orecchie.
- Si risolve abbassando al minimo il volume del telefono: man mano che scende il gatto passa da furioso ad
  arrabbiato, seccato, assonnato; a volume zero si addormenta. Nell'Editor il volume si simula coi tasti - e +.
- Se il volume è già a zero all'inizio bisogna prima alzarlo (la musica deve essere "sentita" almeno una volta).

## Livello 9 — Hold On
- Due pulsanti collegati a un timer da 5 secondi. Il conto alla rovescia parte solo se entrambi sono premuti
  insieme (due dita; i cavi si illuminano). Se uno viene rilasciato prima della fine il timer torna a 5.
  Nell'Editor il secondo dito si simula tenendo premuta la barra spaziatrice.

## Livello 10 — Buzz Off
- Una vespa furiosa svolazza a zig-zag e ogni pochi secondi si lancia contro lo schermo per pungere chi guarda:
  diventa enorme, bagliore rosso ai bordi, "OUCH!" e schermo che trema.
- Nell'erba c'è una paletta acchiappamosche: va raccolta con un tocco. A mani nude la vespa se la ride ("Ha-ha!").
- Con la paletta ogni tocco la fa calare sul punto toccato (colpo con "splat"); l'area colpibile è più ampia
  della vespa. Servono 10 colpi: a ogni colpo sbanda, lampeggia e accelera; al decimo cade a zampe all'aria.

## Livello 11 — Art Theft
- Porta chiusa, ma la chiave è appesa dentro il quadro: si stacca dal quadro, si rimpicciolisce/allarga col
  pizzico (come nel livello 1) e si infila nella serratura.

## Livello 12 — Bodyguard
- Il gatto dorme e una vespa inferocita vuole pungerlo. Col dito si disegnano linee (massimo 3, la più vecchia
  sparisce) su cui la vespa rimbalza. Resisti 10 secondi e la vespa se ne va.

## Livello 13 — Lights On
- Sul muro "YOU" in rosso e "WON" in blu, invisibili al buio. Il pulsante rosso accende la lampada rossa (si legge
  solo WON), il blu quella blu (si legge solo YOU). Tenendoli premuti insieme (due dita) si legge YOU WON.
  Nell'Editor: tasti R e B.

## Livello 14 — Cookie Jar
- Il topolino vuole il biscotto chiuso in un barattolo aperto solo in alto. La gravità segue il telefono:
  bisogna capovolgerlo per far uscire il biscotto, poi farlo cadere dal topolino. Nell'Editor: frecce.

## Livello 15 — Make a Wish
- Torta con 5 candeline: si spengono soffiando davvero nel microfono (al primo avvio Android chiede il permesso).
  Ogni soffio deciso ne spegne una; poi coriandoli. Nell'Editor: tieni premuto B.

## Livello 16 — Trap Maze
- Labirinto a inclinazione a serpentina con buche (ci cadi dentro: si riprova) e due laser intermittenti
  (prima di accendersi tremolano; se ti prendono accesi: ZAP!).

## Livello 17 — In the Dark
- Labirinto al buio: si vede solo intorno alla sfera. L'uscita è chiusa da un cancello rosso: prima bisogna
  trovare la chiave in un vicolo cieco, poi tornare all'uscita.

## Livello 18 — Hidden Safe (azioni in sequenza)
- Scuoti il telefono finché il quadro cade (Editor: tasto S) → dietro c'è una cassaforte e sul retro del quadro
  un post-it col codice (casuale) → tocca la cassaforte e digita il codice → prendi la chiave → apri la porta.

## Livello 19 — Bedtime (azioni in sequenza)
- Spegni l'abat-jour, abbassa a zero il volume del telefono, poi non toccare niente per 5 secondi.

## Livello 20 — Lift Off (azioni in sequenza)
- Trascina la tanica sul razzo, alza il coperchio di sicurezza, tieni premuto il pulsante per tutto il
  conto alla rovescia (se lo lasci: ABORT!). Editor: barra spaziatrice per tenere premuto.

## Livelli col volume (8 e 19)
- Serve SEMPRE almeno una pressione del tasto "volume -" durante il livello: un telefono già a volume zero non
  risolve il livello da solo. Il tasto si intercetta con `Plugins/Android/GotchaKeys.java` (anche quando il volume è
  già al minimo e il valore non cambia); come riserva vale anche un calo del volume osservato. Editor: tasto "-".

## Livelli 21-31
- **21 Low Battery**: collega davvero il caricatore (se era già collegato: staccalo e riattaccalo). Editor: tasto C.
- **22 Hot Soup**: soffia nel microfono per raffreddare la zuppa, POI trascinala al topolino. Editor: B.
- **23 Frozen Key**: strofina il ghiaccio col dito, rimpicciolisci la chiave col pizzico, apri la porta.
- **24 Campfire**: strofina il bastoncino avanti e indietro per la scintilla, poi soffia PIANO (forte la spegne).
  Editor: B soffio leggero, N soffio forte.
- **25 Chicken Button**: premi 5 volte un pulsante con le gambe che scappa; contro i muri è più lento.
- **26 Thirsty Plant**: inclina il telefono per versare 18 gocce nel vaso.
- **27 Shell Game**: segui la scatola col topolino.
- **28 Sky Wire** (taglio): "taglia il filo del colore del cielo"... fuori è notte: il filo giusto è il nero.
- **29 Sunny Day**: il sole scioglie il pupazzo (stesso scioglimento dell'Easter egg); trascina la nuvola davanti al
  sole (il vento la sposta) fino al tramonto. Se si scioglie: Easter egg `sun_melt` con video da condividere.
- **30 Cut the Rope** (taglio + tempismo): taglia una corda, aspetta l'oscillazione, taglia l'altra.
- **31 Pizza Party** (taglio + trascinamento): due tagli dal centro, poi una fetta a ogni topolino.

## Icona dell'app
- L'icona "G!" al neon è ridisegnata dal codice (`DoodleSprites.AppIcon`). Per usare il logo originale basta
  copiare il PNG (quadrato, 1024×1024) in `Assets/Art/Icon/AppIcon.png` e rilanciare il builder: ha la precedenza.

## Musica
- Colonna sonora `Assets/Audio/Soundtrack.wav` ("Dolce Pizzicato") in loop. I livelli possono abbassarla solo
  finché esistono (`MusicPlayer.Duck/Unduck`): non può più restare muta per errore. Se la musica è accesa ma il
  telefono è a volume zero, il menu lo scrive ("MUSIC: ON (PHONE VOLUME 0)").
- Il permesso del microfono viene chiesto all'avvio del gioco. (importata compressa Vorbis). Si spegne/accende dal MENU
  (la scelta resta salvata). Durante la pubblicità la musica è in pausa.

## Regola pubblicità
- Timer invisibile: ogni 3 minuti di gioco attivo (non in pausa, menu o app in background) la pubblicità parte
  alla fine del livello in cui il timer è scaduto.
- Altrimenti parte dopo 5 livelli superati dall'ultima pubblicità mostrata.
- Ogni pubblicità azzera sia il timer sia il conteggio dei livelli.
- Per ora è simulata (`Ads/SimulatedAdProvider`): schermata intera, il tasto di chiusura si attiva dopo 3 secondi.
  Per passare ad AdMob basterà un nuovo `IAdProvider`.

## Non ancora fatto
Effetti sonori, AdMob reale + consenso UMP, condivisione video su iOS.

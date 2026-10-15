# Gotcha! — v0.19 (35 livelli)

La versione installata si legge nel MENU del gioco (accanto al conteggio degli Easter egg).

Progetto Unity 2022.3 LTS o Unity 6, C#, 2D (Built-in Render Pipeline).
Grafica in stile cartone "doodle" generata da codice (nessuna immagine esterna da importare).

## Come aprirlo
1. Clona o scarica questo repository, poi Unity Hub > **Add project from disk** > seleziona la cartella del
   repository (con Android Build Support, OpenJDK, Android SDK & NDK). Non creare un progetto nuovo.
2. Alla prima apertura `Assets/Editor/GotchaProjectBuilder.cs` genera da solo: sprite doodle
   (`Assets/Art/Generated`), materiali, prefab riutilizzabili, prefab dei livelli, LevelDefinition, LevelCatalog,
   scena `Assets/Scenes/Main.unity` e impostazioni Android (verticale, ARM64 + ARMv7, IL2CPP).
   Con Unity 6 imposta anche Active Input Handling = Input Manager (Old) ("Both" fa fallire la build Android) e chiede di riavviare.
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

## Livello 3 — Big Red Button
- Il gatto è sveglio e miagola: ha fame. Bisogna prendere il pesce dal quadro e darglielo.
- Dopo aver mangiato si addormenta: a quel punto vince chi aspetta 5 secondi senza toccare lo schermo e senza
  muovere il telefono. Ogni tocco o scossone lo sveglia e fa ripartire l'attesa.
  Il pulsantone rosso "PRESS ME" è solo un'esca.

## Livello 4 — Slice It!
- Taglia la mela col dito: il gesto deve entrare nella mela e uscire dall'altra parte (un taglio deciso).
  La mela si divide in due metà orientate come il taglio, che cadono sul tagliere con qualche goccia di succo.

## Livello 5 — Beep Beep
- La radio suona a caso uno di cinque segnali: `...` (tre brevi), `---` (tre lunghi), `.--`, `-.-`, `.-.`.
  Bisogna premere il tasto col disegno corrispondente. Toccando la radio il segnale si riascolta.
- Tasto sbagliato: ronzio, tutto trema e GOTCHA! (si riprova con un nuovo segnale a caso).
- La spia della radio si accende insieme ai toni (si gioca anche col telefono silenzioso); la musica si abbassa
  mentre suonano i segnali. Toni generati da codice (sinusoide 760 Hz).

## Livello 6 — Tick Tock
- Pulsante blu e pulsante rosso, due cavi gialli intrecciati a treccia: si scambiano di lato più volte con incroci
  netti (quasi perpendicolari) ma non corrono mai uno sopra l'altro per più di ~4 mm (ogni disposizione casuale
  viene controllata e scartata se i cavi si sovrappongono troppo). Uno finisce libero (fili di rame sfilacciati,
  all'altezza del timer), l'altro entra nel timer della dinamite. Il timer parte da 10 secondi.
- Accanto al timer un tizio in camicia guarda il giocatore in preda al panico: trema e suda sempre di più man mano
  che il tempo scende; se la bomba viene disinnescata tira un sospiro di sollievo.
- Il pulsante collegato al timer lo ferma ("Phew!"). Quello sbagliato o il tempo scaduto: BOOM! e GOTCHA!
  Ogni tentativo ridisegna i cavi e sceglie a caso quale pulsante è quello giusto.

## Livello 7 — Shhh!
- Lo stereo suona a tutto volume e il gatto (lo stesso del livello 4) è furioso: soffia, fuma dalle orecchie.
- Si risolve abbassando al minimo il volume del telefono: man mano che scende il gatto passa da furioso ad
  arrabbiato, seccato, assonnato; a volume zero si addormenta. Nell'Editor il volume si simula coi tasti - e +.
- Se il volume è già a zero all'inizio bisogna prima alzarlo (la musica deve essere "sentita" almeno una volta).

## Livello 8 — Hold On
- Due pulsanti collegati a un timer da 5 secondi. Il conto alla rovescia parte solo se entrambi sono premuti
  insieme (due dita; i cavi si illuminano). Se uno viene rilasciato prima della fine il timer torna a 5.
  Nell'Editor il secondo dito si simula tenendo premuta la barra spaziatrice.

## Livello 9 — Buzz Off
- Una vespa furiosa svolazza a zig-zag e ogni pochi secondi si lancia contro lo schermo per pungere chi guarda:
  diventa enorme, bagliore rosso ai bordi, "OUCH!" e schermo che trema.
- Nell'erba c'è una paletta acchiappamosche: va raccolta con un tocco. A mani nude la vespa se la ride ("Ha-ha!").
- Con la paletta ogni tocco la fa calare sul punto toccato (colpo con "splat"); l'area colpibile è più ampia
  della vespa. Servono 10 colpi: a ogni colpo sbanda, lampeggia e accelera; al decimo cade a zampe all'aria.

## Livello 10 — Art Theft
- La chiave non è appesa: è dipinta nella natura morta sul tavolo, tra frutta, brocca e candela. Toccandola si
  "stacca" dalla tela e diventa vera (nel quadro resta la sua impronta chiara); poi va nella serratura.

## Livello 11 — Snowman
- Pupazzo dalle sembianze umane (gambe tozze, fianchi, busto) con una testa enorme e buffa, in posa di 3/4
  (guarda a sinistra). Il sedere è solo accennato dalla curva dei fianchi sul retro.
- Occhi (carboni) e naso (carota), più grandi del normale, restano attaccati dove li lasci sulla testa;
  altrove cadono nella neve. Nessuna posizione è indicata: ogni occhio deve stare nella sua zona (ampia e
  invisibile) e il naso al centro; la bocca si disegna col dito e resta come un solco: deve essere un sorriso.
- Easter egg `snowman_butt`: lascia la carota sul sedere. Resta piantata lì; il pupazzo arrossisce, trema e
  URLA (voce femminile: `Assets/Audio/Scream.wav` se presente, altrimenti sintetizzata da `ScreamSynth`): gli occhi
  schizzano via, la bocca disegnata diventa una "O", il cappello salta, le braccia si agitano nel panico.
  Poi si scioglie davvero (`Components/SnowmanMelter` + shader `Shaders/SnowMelt.shader`): la neve si affloscia verso
  terra a colonne irregolari, cola, diventa bagnata e lucida e si allarga in una pozzanghera. La sciarpa NON è neve
  (disegno a parte): scende col collo e galleggia, come braccia, cappello e carote; alla fine gli occhi ricadono
  dal cielo nella pozzanghera ("plop!").
- La scena viene registrata e a fine livello compare **SHARE VIDEO**: crea un MP4 (salvato in Movies/Gotcha) e apre
  il menu di condivisione di Android. Solo su Android; nell'Editor i fotogrammi finiscono in `persistentDataPath/clip_frames`.
- Zone, misure e punti d'appoggio sono regolabili dall'Inspector del prefab `Level_Snowman`.

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

## Fumetti
- Il testo va a capo da solo sulle parole; se ancora non entra si rimpicciolisce un po' (mai sotto il 75%) e poi è
  il fumetto ad allargarsi: il testo resta sempre dentro e leggibile (`SpeechBubble.maxTextWidth/maxTextHeight`).

## Gatti che miagolano
- Quando un gatto "parla" (livelli 3, 7, 12, 19) si sente il verso, sintetizzato da `Components/CatVoice`: miagolio,
  soffio ("HISSS!"), fusa ("Purrr..."), trillo ("Mrr?"), sbadiglio ("Yaaawn..."). Si attiva con `catVoice` sul fumetto.

## Vite
- 5 cuori pulsanti sotto la barra in alto (toccandoli si apre il pannello delle vite). Ogni fallimento costa una
  vita (gli Easter egg no: sono scoperte; si cambia con `easterEggCostsLife` su GameFlow).
- Se ne ricarica una ogni 15 minuti; +1 guardando una pubblicità (simulata, 5 secondi); +1 per ogni amico che
  accetta l'invito. A zero vite compare "OUT OF LIVES!" con attesa, pubblicità e invito.
- Cambiare l'ora del telefono non serve: la ricarica usa il tempo dall'accensione (SystemClock.elapsedRealtime,
  non modificabile) e il contatore dei riavvii; dopo un riavvio si accredita solo il tempo dall'accensione. I dati
  sono firmati (HMAC): se vengono modificati a mano le vite ripartono da zero. Limite noto: disinstallare e
  reinstallare (o cancellare i dati dell'app) riparte con le vite piene; per evitarlo serve un server.
- Inviti: il pulsante condivide un link al Play Store con un codice personale. Il premio quando l'amico ACCETTA
  richiede un server (es. Firebase + Play Install Referrer) che chiami `InviteService.OnFriendAccepted()`.

## Menu
- Livelli a pagine da 10 (due colonne), si scorrono con lo swipe o con le frecce; pallini della pagina.
  Livelli bloccati in grigio, livello attuale in rosso, livelli con Easter egg in viola ("SECRET EGG" /
  "EGG FOUND!"). L'Easter egg di un livello si indica con `easterEggId` nel LevelDefinition.

## Livelli col volume (7 e 19)
- Serve SEMPRE almeno una pressione del tasto "volume -" durante il livello: un telefono già a volume zero non
  risolve il livello da solo. Il tasto si intercetta con `Plugins/Android/GotchaKeys.java` (anche quando il volume è
  già al minimo e il valore non cambia); come riserva vale anche un calo del volume osservato. Editor: tasto "-".

## Livelli 21-31
- **21 Low Battery**: collega davvero il caricatore (se era già collegato: staccalo e riattaccalo). Editor: tasto C.
  Senza caricatore: "NO CHARGER?" fa comparire una dinamo a manovella da girare col dito; il livello si supera
  ma il robot si accende triste.
  Easter egg `mecha_robot`: se la carica arriva dal caricatore VERO, il robot va in sovraccarico, lampo bianco, e si
  evolve in un robot da battaglia giapponese (V dorata, spada laser) che ringrazia "ARIGATO, MASTER!". Il livello è
  superato E l'Easter egg viene contato; la scena è registrata e si può condividere (SHARE VIDEO).
- **22 Hot Soup**: soffia nel microfono per raffreddare la zuppa, POI trascinala al topolino. Editor: B.
  Senza microfono si può anche fare vento col dito, sventolando sul vapore sopra la scodella.
  Easter egg `hot_soup`: se gliela dai bollente, il topo diventa rosso, fuma, prende fuoco e resta lo scheletrino
  con le X al posto degli occhi e la linguaccia (video da condividere).
- **23 Frozen Key**: strofina il ghiaccio col dito, rimpicciolisci la chiave col pizzico, apri la porta.
- **24 Campfire**: strofina il bastoncino avanti e indietro per la scintilla, poi soffia PIANO (forte la spegne).
  Editor: B soffio leggero, N soffio forte.
- **25 Wake-up Call**: la sveglia suona (bip sintetizzati) e il suo pulsante OFF è scappato con le gambette.
  Disegna col dito fino a 4 staccionate per chiuderlo in un angolo (ci sbatte contro), poi premilo: torna sulla
  sveglia e finalmente silenzio.
- **26 Thirsty Plant**: fionda alla Angry Birds: tira indietro la goccia (lontano dal vaso), mira con la traiettoria
  a puntini e rilascia. 10 gocce, ne bastano 5 nel vaso.
- **27 Shell Game**: segui la scatola col topolino. Il topolino è nascosto davvero (si muove con la sua scatola);
  ogni 3-7 secondi (a caso) la codina (sottile e nera) sbuca da sotto il bordo della scatola e penzola giù dal tavolo.
- **28 Sky Wire** (taglio): fili rosso, verde e nero. "Taglia il filo del colore del cielo"... fuori è notte:
  il filo giusto è il nero.
- **29 Sunny Day**: il sole scalda il pupazzo (suda, arrossisce, si affloscia appena); trascina la nuvola davanti al
  sole (il vento la sposta) fino al tramonto: il sole scende DIETRO le colline e sorge la luna. Se il caldo arriva al
  limite parte la stessa tragedia dell'Easter egg del livello 11 (`Components/SnowmanMeltdown`): Easter egg `sun_melt`.
- **30 Cut the Rope** (taglio + tempismo): taglia una corda, aspetta l'oscillazione, taglia l'altra.
- **31 Pizza Party** (taglio + trascinamento): due tagli dal centro, poi una fetta a ogni topolino.

## Livelli 32-35
- **32 Pizza Chef**: trascina gli ingredienti dalle ciotole sulla pasta. La lavagna sul muro ha i "disegnini della
  nonna" (pomodoro, mozzarella, fungo, oliva): sono la ricetta, ma non sembra. Ogni ciotola ha il nome sotto. Poi
  trascina la pizza sul forno (o tocca il forno, che si illumina quando la pizza è pronta), tagliala
  in quattro (due tagli dal centro) e dai una fetta a ogni topolino. Ricetta giusta: festa; manca qualcosa o c'è
  l'ananas: si riprova. Easter egg `spicy_pizza`: col peperoncino i topi diventano paonazzi, furiosi, con gli occhi in
  fiamme e la lingua di fuori (video da condividere).
- **33 Blow It Up**: gonfia il palloncino fino alla sagoma tratteggiata col fiato (microfono) o con la pompa (10 colpi).
  Arrivati alla sagoma bisogna aspettare 2 secondi: un colpo in più o troppo fiato e PLOP! Editor: B per soffiare.
- **34 Cat Eyes**: le pupille del gatto seguono l'inclinazione del telefono (rispetto a come lo tieni all'inizio); un
  topolino sbuca da una tana a caso: fissalo per 3 secondi di fila. Editor: frecce.
- **35 Pancake Flip**: un colpo secco col telefono lancia la frittella, che deve fare mezzo giro e ricadere nella
  padella (si sposta inclinando il telefono). Tre giri riusciti. Troppo piano non si gira, troppo forte resta
  appiccicata al soffitto, fuori dalla padella cade per terra. Editor: tasti 1/2/3 (piano/giusto/forte) e frecce.
  La forza del colpo si regola con `speedPerShake` e `idealSpeed` nel prefab.

## Livelli 36-40
- **36 Sting Run** (labirinto + inseguimento): si inclina come al livello 2. La vespa parte dall'angolo alle spalle
  della sfera dopo 1.5 s e vola dritta verso di lei; i muri la fermano (ci scivola lungo), quindi conviene metterle
  un muro in mezzo. Ogni 5 s si ferma un attimo a riprendere fiato. Se ti punge si riprova. Regolabili nel prefab:
  `speed`, `startDelay`, `restEvery`, `restFor`, `stingDistance`.
- **37 Secret Code** (luci rosse e blu): 4 cifre casuali, ognuna rossa o blu a caso (almeno una per colore). Con la
  luce rossa si leggono solo le cifre blu, con la blu solo le rosse, al buio nessuna. I due interruttori si escludono
  a vicenda: bisogna ricordare metà codice. Tocca la cassaforte per la tastiera; codice sbagliato = ERR e si ridigita.
- **38 Apple Catch**: scuoti il telefono (Editor: S) e cadono 1-2 mele; il cesto segue l'inclinazione (Editor:
  frecce). Ne servono 5 su 10: se ne cadono per terra troppe per arrivarci, si riprova.
- **39 Marble Run** (disegno + fisica): fino a 3 rampe (la quarta cancella la più vecchia), poi GO: più a lungo
  lo tieni premuto, più forte è la spinta (la barra arancione si riempie in 1.2 s; `minPush`, `maxPush`,
  `chargeSeconds` nel prefab). Se la biglia si ferma fuori dalla tazza torna al via dopo 1.2 s. La biglia parte
  dalla mensola, deve superare il muretto e restare mezzo secondo nella tazza. Se manca torna al via con le rampe
  intatte: si corregge e si riprova senza perdere vite.
- **40 Copycat** (memoria): il gatto suona una sequenza sui 4 tasti colorati (do, mi, sol, si); ripetila. Tre turni
  da 3, 4 e 5 note; un tasto sbagliato e il gatto ride. Lunghezze in `roundLengths`.

## Schermata del titolo (0.22)
- All'avvio non parte più subito un livello: c'è il titolo con **NEW GAME** (riparte dal livello 1, i livelli
  sbloccati restano), **LEVEL SELECTION** (le pagine dei livelli; si gioca solo quelli sbloccati) e **SETTINGS**
  (musica, livelli/Easter egg trovati, versione, RESET PROGRESS con doppio tocco di conferma). Se c'è già un
  progresso compare anche **CONTINUE (LEVEL N)**.
- Nel MENU di gioco c'è **HOME** per tornare al titolo. Il logo è `Assets/Art/Icon/AppIcon.png` se importato come
  Sprite, altrimenti l'icona disegnata.

## Livelli 61-100 e Pancake più facile (0.23)
- 61-100: altri 13 labirinti a griglia (`DoodleSprites7.cs`, percorso verificato) e 27 varianti con parametri più
  severi (`VariantLevels2`). Gli Easter egg delle varianti (zuppa, sole, robot) sono gli stessi dell'originale.
- Pancake Flip più facile: tolleranza del mezzo giro da 50° a 68° (`flipTolerance`), padella più larga
  (`catchRadius` 0.95), lancio meno storto (`drift` 0.6). Pancake Tower: 4 giri invece di 5.

## Livelli 41-60 (varianti)
Costruiti dal builder copiando il prefab del livello originale e cambiandone i parametri (`Variant(...)` in
`GotchaProjectBuilder.VariantLevels`), oppure con labirinti nuovi (`DoodleSprites6.cs`: griglia 5x7 generata una
volta, con percorso verificato). Le regole sono quelle dei livelli originali, cambiano i numeri: vedi l'elenco nel
README. Per ritoccare una variante basta cambiare il valore in `VariantLevels` (es. `Tune(Level(go), "swaps", 14)`).

## Correzioni 0.20
- **Bodyguard / Wake-up Call**: le barriere disegnate ora fermano sempre la vespa e il pulsante: il contatto è
  calcolato geometricamente sui tratti (`Components/StrokeBarrier`), non più col cast fisico che a volte li
  attraversava.
- **Thirsty Plant**: il fiore non si allunga più; a livello completato fa due saltelli di felicità, stesse dimensioni.

## Icona dell'app
- L'icona "G!" al neon è ridisegnata dal codice (`DoodleSprites.AppIcon`). Per usare il logo originale basta
  copiare il PNG (quadrato, 1024×1024) in `Assets/Art/Icon/AppIcon.png` e rilanciare il builder: ha la precedenza.

## Musica
- Colonna sonora `Assets/Audio/Soundtrack.wav` ("Dolce Pizzicato") in loop. I livelli possono abbassarla solo
  finché esistono (`MusicPlayer.Duck/Unduck`): non può più restare muta per errore. Se la musica è accesa ma il
  telefono è a volume zero, il menu lo scrive ("MUSIC: ON (PHONE VOLUME 0)").
- Il permesso del microfono viene chiesto all'avvio del gioco. Il microfono è condiviso (`SensorHub.UseMic/ReleaseMic`):
  resta acceso finché un livello lo usa, parte appena arriva il permesso e riparte se Android lo ferma. (importata compressa Vorbis). Si spegne/accende dal MENU
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

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
  altrove cadono nella neve. Nessuna posizione è richiesta: due occhi e il naso in un punto qualsiasi della testa
  e una bocca qualsiasi disegnata col dito (resta come un solco). Il pupazzo commenta la faccia: più è storta,
  più è buffo (occhi che ballano, saltelli sbilenchi).
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

## Versione 0.46
- **Livelli 223-230**: altri 8 livelli della specifica "105-200" (blocco 5):
- **223 The Angry Keyboard** — Premere i tasti fa perdere lettere alla tastiera (5 pressioni = esplode): trascina le lettere O, P, E, N dai tasti nella casella della risposta, in ordine.
- **224 The Cowardly Dragon** — Il drago ha paura del fuoco: trascina il sole dietro di lui, basso e vicino; l'ombra mostruosa sul muro lo spaventa e starnutisce una fiammata che brucia la barricata.
- **225 Hidden Frequency** — Gira piano la manopola senza numeri guardando la vibrazione della serratura: fermati sulle tre frequenze giuste (casuali) finché le tre luci diventano verdi.
- **226 The Crooked Bridge** — Con un dito il ponte bascula: con due dita porta entrambe le estremità all'altezza delle piattaforme, poi tocca il topo che attraversa.
- **227 The Final Wrong Answer** — Tre porte (3, 4, FISH) sono tutte sbagliate: trascina giù il cartello della domanda, che rivela un buco nel muro dove salta il topo.
- **228 The Impatient Clock** — Le lancette tornano indietro: tieni la lancetta dei minuti sul 12 con un dito, porta quella delle ore sul 12 con l'altro e stacca le dita insieme.
- **229 The Frozen Waterfall** — Trascina la lente nella luce del sole e orientala finché il raggio scioglie la cascata ghiacciata (4 s): il topo prende la chiave.
- **230 The Fake Checkpoint** — Il checkpoint rispedisce l'eroe all'inizio: trascina la bandierina sopra la porta finale, poi premi GO.
- Musica: Hidden Frequency e The Frozen Waterfall nel dungeon, The Cowardly Dragon tra gli animali.
- Versione del costruttore 49.

## Versione 0.45
- **Livello 25 (Toni STUCK!) ora si supera**: il burro veniva rimesso sul tavolo prima di controllare dove era
  stato lasciato, quindi non ungeva mai Toni. Ora vale se lasciato su Toni, vicino al sedere o alla gattaiola;
  poi un tocco o una spinta verso destra lo fa uscire.
- **Livello 11 (pupazzo di neve)**: occhi e naso in qualsiasi punto della testa, qualsiasi bocca disegnata.
  Alla vittoria il pupazzo commenta la faccia ("Who nose where my nose goes?!", "Cross-eyed and PROUD!", ...),
  gli occhi di carbone ballano e saltella tutto storto.
- Versione del costruttore 48.

## Versione 0.44
- **Livelli 215-222**: altri 8 livelli della specifica "105-200" (blocco 4):
- **215 The Stupid Genius** — Ignora la macchina gigante dello scienziato e tocca direttamente il pulsantino rosso.
- **216 The Escaping Cursor** — Il cursore scappa: trascina il bersaglio sotto la sua punta.
- **217 Upside-Down Code** — Il codice è stampato capovolto: capovolgi il telefono, leggilo e digitalo.
- **218 The Human Charger** — Trascina il fulmine della scritta LOW POWER sul robot quattro volte prima che si spenga.
- **219 Adpocalypse** — La X della pubblicità finta scappa: trascina tutto il banner fuori dallo schermo. Easter egg *Adpocalypse*: Tocca la X finta esattamente 7 volte in 15 secondi prima di spostare il banner: arrivano pubblicità assurde e le X scioperano.
- **220 The Glass Cage** — Ruota la campana di vetro con due dita finché l'apertura guarda il topo.
- **221 The Invisible Ink** — Muovi il phon sul foglio senza fermarti per rivelare la mappa a inchiostro simpatico.
- **222 The Last Drop** — Inclina il telefono per portare l'ultima goccia dal tappo al foro, poi tocca per farla cadere sulla pianta.
- Versione del costruttore 47.

## Versione 0.43
- **Livelli 183-214**: altri 32 livelli della specifica "105-200" (blocchi 2 e 3), in coda:
- **183 Pixel Rebellion** — Zooma col pizzico sul puntino luminoso finché diventa una creaturina di pixel e toccala. Easter egg *Pixel Rebellion*: Con lo zoom oltre il 600% tocca 5 volte in fretta l'occhio sinistro della creatura: i pixel scioperano.
- **184 Cold Coffee** — Metti la tazza sotto la lampada e tieni premuto HEAT, rilascia perché il termometro si fermi nel verde.
- **185 The Missing Second** — L'orologio è fermo alle 11:59:59: scorri in su la cifra dei secondi per arrivare a mezzogiorno.
- **186 Mirror Twins** — Fai scorrere lo specchio finché la chiave riflessa combacia con la serratura.
- **187 The Hungry Printer** — La stampante vuole carta: infila il foglio INSTRUCTIONS nella fessura.
- **188 Finger Maze** — Tieni un dito sull'impronta per renderla trasparente e col secondo dito guida il topolino.
- **189 False Finish** — Non premere NEXT: aspetta il topolino nascosto che passa il traguardo e toccalo.
- **190 Heartbeat** — Senti la vibrazione corto-corto-lungo e ripetila toccando il cuore.
- **191 The Wrong Shadow** — Sposta la lampada davanti al gatto: l'ombra da mostro finisce dietro di lui e il topo passa.
- **192 The Impossible Scale** — Ingrandisci la piuma con due dita finché bilancia l'elefante.
- **193 Rewind Reality** — Riavvolgi il filmato prima della caduta, metti in pausa e sposta il vaso al sicuro.
- **194 The Sticky Finger** — La moneta resta attaccata al dito: tienila e con un altro dito porta la cassaforte sotto.
- **195 The Guilty Button** — Tieni premuti i pulsanti per farli sudare: quello con la goccia metallica nasconde la chiave.
- **196 The Invisible Stairs** — Tocca lo spazio vuoto per trovare i 4 gradini invisibili, poi porta l'omino alla porta.
- **197 Lie Detector** — Non rispondere al robot: trascina la macchina della verità su di lui.
- **198 Too Bright!** — Chiudi le tende sulle finestre, poi porta il vampiro alla bara.
- **199 Unfair Race** — Lumaca contro lepre: prima del via sposta il traguardo davanti alla lumaca.
- **200 The Bottomless Cup** — Tieni un dito sul buco del bicchiere e con l'altro apri il rubinetto fino alla tacca.
- **201 The Dirty Secret** — Strofina lo schermo sporco: oltre il 95% compare il pulsante EXIT. Easter egg *The Dirty Secret*: Strofina per 8 s la macchia tonda in basso a sinistra: si apre una botola con l'omino che sporca il vetro.
- **202 The Fake Virus** — Trova il virus originale (starnutisce ogni 5 s) e trascinalo nel cestino.
- **203 Slow Motion** — Tieni due dita ferme per rallentare il tempo e con un terzo sposta la paratia sulla palla.
- **204 The Overworked Robot** — Raggruppa i 4 interruttori a coppie sotto le due mani del robot, poi PRESS.
- **205 The Missing Letter** — OPEN THE G_TE: trascina la A nascosta nella nuvola nel buco del cartello.
- **206 Anti-Gravity Taxi** — Ruota la freccia della gravità verso l'alto e il taxi sale alla piattaforma.
- **207 The Silent Disco** — Solo il ballerino blu tiene il ritmo: tocca il pavimento tre volte con la sua cadenza.
- **208 The Door That Lies** — Scambia le targhe delle porte: la vera uscita si spaventa quando riceve DANGER.
- **209 Invisible Button** — Striscia il dito al buio: la vibrazione cresce vicino al pulsante invisibile, poi tocca.
- **210 The Second Key** — Metti la chiave davanti allo specchio e tocca il riflesso: diventa la seconda chiave.
- **211 Shadow Thief** — Il ladro è invisibile ma ha l'ombra: accendi il lampione verso cui va.
- **212 The Broken Ruler** — Ricomponi il righello rotto e porta il ponte esattamente a 7 unità.
- **213 The Backward Song** — L'uccellino canta al contrario: suona le 4 note in ordine inverso.
- **214 Don'T Blink!** — Tieni premuto l'occhio WATCH e con un altro dito porta la chiave alla porta.
- Versione del costruttore 46.

## Versione 0.42
- **Pubblicità**: una ogni 5 livelli superati OPPURE ogni 140 secondi di gioco (prima erano 180), quella che
  arriva prima; parte sempre alla fine del livello in corso, sia dopo una vittoria sia dopo una sconfitta (chi resta
  bloccato più di 140 s su un livello la vede al termine del tentativo).
- **Promemoria "I'm missing you! <3"**: notifica locale 24 ore dopo l'ultimo accesso, solo tra le 08:00 e le 22:00
  (se cade di notte arriva alle 08:00). Il permesso si chiede dopo il primo livello superato; si spegne da
  Impostazioni → REMINDERS. Pacchetto Unity Mobile Notifications (vedi PLAY_STORE.md per l'icona).
- **Obiettivo giornaliero**: supera 5 livelli nello stesso giorno per un premio; i giorni consecutivi formano una
  serie e il premio cresce (1 hint, 5 min di vite infinite, 2 hint, 10 min, 3 hint, 15 min, poi 5 hint + 30 min al
  7° giorno, e il ciclo riparte). Saltare un giorno azzera la serie. Il progresso è nella schermata del titolo, nel
  pannello del risultato e in un pannello "DAILY GOAL!" quando si vince il premio.
- Versione del costruttore 45.

## Versione 0.41
- **Livelli 151-182**: i primi 32 livelli della specifica "105-200" (blocchi 1 e 2), messi in coda a quelli esistenti
  (i livelli di Toni della specifica arriveranno con i pacchetti successivi):
- **151 Don'T Touch Me!** — Il pulsante arrabbiato: tienilo premuto 5 secondi finché si arrende; tre tocchi rapidi lo fanno infuriare.
- **152 Balloon Surgery** — Sgonfia piano il palloncino con due dita finché la chiave si posa al sicuro (non farlo scoppiare).
- **153 The Lazy Elevator** — Il pulsante 5 è rotto: trascina il 4 sotto l'1, 4+1=5 e l'ascensore sale.
- **154 Phone Sandwich** — Senza gravità: inclina il telefono per impilare gli ingredienti sul pane, poi capovolgilo per chiudere il panino.
- **155 Left Behind** — Il cartello dice GO RIGHT: trascina via RIGHT, sotto c'è LEFT, esci a sinistra.
- **156 Sleeping Guard** — Trascina la chiave solo mentre la guardia russa forte.
- **157 The Broken Compass** — Capovolgi il telefono per aggiustare l'ago, poi ruota la ghiera finché la N coincide.
- **158 Fake Update** — Finto aggiornamento al 99%: trascina via il 99, metti l'1 nascosto davanti ai due 9 e diventa 100.
- **159 Glass Floor** — Tocca le lastre di vetro per ascoltarle: nota grave = robusta; porta il topolino solo su quelle.
- **160 Tiny Thief** — Ingrandisci col pizzico per trovare il ladro minuscolo e mettigli il cesto sul percorso vicino all'uscita.
- **161 The Silent Alarm** — Abbassa a zero il volume del telefono, poi premi i pulsanti nell'ordine mostrato.
- **162 Black Hole** — Allarga il buco nero col pizzico finché supera lo schermo: YOU ESCAPED.
- **163 Gravity Resigned** — Capovolgi il telefono e tienilo 2 s per far cadere l'omino. Easter egg *Gravity Resigned*: Ruota il telefono 0°→180°→0°→180°→0° entro 12 s con una pausa a ogni posizione: la Gravità si dimette.
- **164 The Cheap Magician** — Tira fuori il coniglio dal cilindro e coprilo col cilindro: TA-DAA!
- **165 Don'T Wake The Bomb** — Sfila la miccia lentissimamente senza svegliare la bomba.
- **166 The Upside-Down King** — Inclina il telefono per portare la scala sotto la corona, poi trascina il re sulla scala.
- **167 One Too Many** — Sei galline per cinque nidi: la sesta è un riflesso, trascina via la pozzanghera.
- **168 Reverse Parking** — Parcheggio con due pollici: uno sul volante e uno sul pedale, breve retromarcia nel posto giallo.
- **169 The Stubborn Door** — Una chiave per tre lucchetti: inseriscila e girala, poi porta gli altri due lucchetti sulla chiave.
- **170 Zero Gravity Dinner** — Raccogli 5 polpette fluttuanti con la ciotola e tienila ferma 3 secondi.
- **171 The Forgotten Password** — Non digitare: trascina il post-it 'I FORGOT IT' nel campo password.
- **172 The Shrinking Room** — Le pareti si stringono: rimpicciolisci tutta la stanza col pizzico e appare l'uscita.
- **173 Paperwork Nightmare** — Butta tutti i fogli nel cestino: il documento è stampato sulla scrivania, toccalo.
- **174 Battery Vampire** — Scambia il + e il − della batteria: il vampiro prende la scossa e scappa.
- **175 The Fourth Wall** — L'attore è chiuso in un quadro: trascina la CORNICE finché un lato esce dallo schermo, lui esce dalla parete mancante.
- **176 Hot Potato** — Patata bollente tra 4 amici: tocca chi la riceve quando la lucina diventa verde, 5 prese; 2 errori = purè.
- **177 Shake To Undo** — Il pittore ha disegnato mutande sul capolavoro: scuoti il telefono 3 volte e fermati (la 4ª scossa forte cancella tutto).
- **178 The Lonely Cloud** — Allarga la nuvola con due dita finché la pioggia arriva sul fiore, entro 25 s.
- **179 Ghost Selfie** — Tieni premuto il pulsante della foto: il fantasma entra nell'inquadratura durante il 3-2-1, rilascia alla fine.
- **180 Magnet Mayhem** — Guida la chiave nel labirinto spostando 3 calamite (rosse attirano, blu respingono) senza toccare le barre elettriche.
- **181 The Fake Crash** — Finta finestra 'GOTCHA HAS STOPPED WORKING': trascina via la parola WORKING.
- **182 Rainbow Thief** — Ricrea giallo, ciano e magenta mettendo a coppie le luci primarie nelle tre casse.
- Versione del costruttore 44.

## Versione 0.40
- **Livelli 129-150: rompicapi compositi**, ognuno a più fasi con interazioni diverse (difficoltà crescente):
- **129 Morning Routine** — La mattina del gatto in 4 fasi: scuoti il telefono per svegliarlo, apri le tende, inclina per versare i cereali fino alla linea, poi il latte fino alla fascia verde.
- **130 Bank Heist** — Colpo in banca: torcia UV sul quadro per il codice invisibile, tastierino, poi il filo giusto dell'allarme secondo l'indovinello.
- **131 Submarine** — Sottomarino: inclina per guidarlo nella grotta e soffia nel microfono per l'ossigeno; PING del sonar per il masso giusto; rotelle coi simboli della bandiera.
- **132 Haunted House** — Casa stregata: dito-torcia per trovare 3 candele, strofina il fiammifero e accendile, poi VOLUME SU per sentire il sussurro e scegliere la porta.
- **133 Kitchen Escape** — Fuga dalla cucina con inventario: raccogli, unisci spago e calamita, pesca la chiave dallo scarico, apri il pensile e pizzica il blocco per bambini.
- **134 Space Station** — Stazione spaziale: ruota i cavi per portare energia alla parabola, inclina per puntarla, decodifica il codice luminoso e lancia il razzo.
- **135 Treasure Map** — Mappa del tesoro: ricomponi i 4 pezzi, segui le istruzioni con gli swipe (la rosa dei venti è ruotata), scava strofinando, apri il baule coi simboli degli angoli.
- **136 Detective** — Giallo: ascolta gli alibi, ingrandisci la scena per trovare 3 indizi, ammanetta il bugiardo.
- **137 Bomb Squad** — Bomba a moduli con manuale da sfogliare: fili, pulsante e simboli in 90 secondi.
- **138 Potion Lab** — Pozione: ingredienti nell'ordine dell'indovinello, mescola con 5 cerchi orari, scalda soffiando fino al verde.
- **139 Robot Factory** — Fabbrica di robot: monta il pezzo mancante sui robot del nastro, poi programma il percorso con 6 carte comando.
- **140 Train Switches** — Scambi ferroviari in tempo reale: manda 8 treni alle stazioni del loro colore, max 2 errori.
- **141 Garden Seasons** — Un anno nell'orto in 4 fasi: semina e annaffia inclinando, scaccia le nuvole e fai ombra, difendi le zucche dalla talpa, scuoti per togliere la neve dalla serra.
- **142 Museum Night** — Furto al museo: muovi il gatto ladro nell'ombra, scambia il quadro col falso mentre il guardiano sbadiglia, esci tra i laser a ritmo.
- **143 Ice Cream Shop** — Gelateria: componi i coni degli scontrini prima che finisca la pazienza; il gatto vuole lo stesso del cliente prima.
- **144 Pirate Cannon** — Cannone pirata: angolo, potenza e vento per colpire 3 bersagli, poi aggancia il forziere col rampino.
- **145 Elevator Logic** — Logica dell'ascensore: porta 4 animali ai loro piani seguendo gli indizi (soluzione unica).
- **146 Circuit Board** — Circuito: collega pila, 2 lampadine e LED (con la resistenza!) e accendi l'interruttore.
- **147 Snow Globe** — Palla di neve: scuoti per vedere il codice nei fiocchi, imposta gli anelli inclinando, ingrandisci la chiavetta e apri il carillon.
- **148 Knight'S Quest** — Scacchi: col cavaliere mangia 4 topi in 6 mosse, poi scacco matto in 2 con torre e cavaliere.
- **149 The Gauntlet** — Remix dei trucchi classici con 3 cuori: chiave col pizzico, biglia, candeline, volume giù, non toccare per 5 secondi.
- **150 Fetch!** (Toni) — Toni al parco in 4 tappe: scuoti l'albero, soffia la palla attraverso lo stagno, allontana il gatto col gomitolo, lancia la palla a Toni.
- **Sblocco**: i livelli si sbloccano solo superandoli (anche nelle build di prova).
- **Modalità beta tester**: Impostazioni → BETA TESTING → password. Dà vite e hint infiniti e toglie la
  pubblicità (la password è salvata solo come hash; si può disattivare dallo stesso pannello).
- **Condivisione dei video**: proposta solo la prima volta che si trova un Easter egg, e mai più dopo averlo
  condiviso. Premio: 3 minuti di vite infinite oppure 1 hint gratis.
- **Pubblicità**: mai tra un tentativo e l'altro (solo dopo un livello superato).
- **Easter egg tolti** dai livelli 25, 30, 50, 102, 103, 115; **nuovo egg** al 55: *Shattered Screen* (rampa che spara
  la biglia dritta in alto a piena potenza: lo schermo va in frantumi).
- **Livelli rifatti** su segnalazione: 16 (trappole sul percorso), 17 (+3 buche e un laser), 18 (soffiare via la
  polvere dal retro del quadro), 20 (taniche H e O: 2 H + 1 O), 21 (trasformazione del robot più epica), 25 (Toni
  imburrato si vede), 40 e 55 (un solo tiro), 46 (nuovo: FOLD IT, mappa da piegare), 62 (interruttore mai sotto i
  pipistrelli o sui mobili), 92 (puzzle con Toni che lecca lo schermo), 94 (legenda delle direzioni invertite), 99
  (impronta di cane al posto del V), 106 (scritta "FILL THE JUG HERE"), 110 (più facile), 113 (niente icona di
  rotazione), 117 (gatto più lento), 122 (ogni hint dà la parola giusta), 125 (solo il sogno, niente fumetti).
- **Fumetti**: si allargano finché il testo ci sta e rientrano nello schermo se escono dai bordi.
- **Coda di Toni** arricciata a spirale.
- Versione del costruttore 43.

## Versione 0.39
- **25 livelli nuovi (104-128)**, ognuno con una meccanica mai usata prima nel gioco:
- **104 Bulb Grid** — Soffitta buia: tocca le lampadine (ognuna accende/spegne anche le vicine) finché sono tutte e 9 accese.
- **105 Sort It Out** — Ordina le palline (un colore per provetta); la pallina grigia è un criceto arrotolato che, sollevato, scappa liberando spazio.
- **106 Four Liters** — Il gatto vuole ESATTAMENTE 4 litri: caraffe da 5 L e 3 L, rubinetto, lavandino e ciotola.
- **107 Yarn Knot** — Il gatto ha ingarbugliato la lana: trascina i 7 bottoni finché nessun filo si incrocia (il bottone con 4 fili al centro).
- **108 Connect The Dots** — Unisci i puntini 1→12→1: diventa una chiave vera da portare alla serratura; una mosca sul 9 va scacciata con un tocco.
- **109 Sky Crane** — Tocca per sganciare la cassa che oscilla dalla gru; la parte che sporge viene tagliata. Impila 6 casse per far scendere il gatto dal tetto. 3 vite. Easter egg *Master Builder*: Tutte e 6 le casse con lancio PERFECT!
- **110 Gone Fishing** — Tira solo quando il galleggiante va tutto sott'acqua con lo spruzzo; rana, foglia e papera sono finte. 3 pesci.
- **111 Count The Sheep** — Conta le pecore che finiscono oltre lo steccato: quella che torna indietro e il lupo travestito non contano, l'agnellino che passa sotto sì.
- **112 Lock Screen** — Sblocca il telefono ripassando le ditate unte sul vetro, partendo dall'impronta grande. 3 tentativi.
- **113 Tall Guest** — La giraffa non entra nello schermo: gira il telefono in orizzontale e il mondo ruota.
- **114 Box Pusher** — Sokoban: spingi le 3 casse di formaggio sui segni rossi; attento alla zampa del gatto che ogni tanto colpisce una casella.
- **115 Hungry Snake** — Snake: guida il serpente con gli swipe e mangia 8 mele. 3 vite. Easter egg *Ouroboros*: Fai mordere alla testa esattamente la punta della coda.
- **116 Pinball** — Flipper: accendi i 3 respingenti, il gatto apre la bocca: mandaci dentro la pallina. 3 palline.
- **117 Air Hockey** — Air hockey contro il gatto, primo a 3 gol; sul 2-0 il gatto bara con lo zampone: toccalo per farlo ritirare.
- **118 Slam Dunk** — Tiro a canestro con uno swipe: 3 canestri con 6 palloni, il ferro si sposta.
- **119 Knock Knock** — Toccare lo schermo non vale: bussa con le nocche sul RETRO del telefono col ritmo giusto (un colpo, pausa, due colpi).
- **120 Screen Time** — Il bambino fissa lo schermo e non dorme: metti il telefono a faccia in giù per 3 secondi, poi giralo.
- **121 Broken Vase** — Ricomponi i 5 cocci del vaso (toccali per ruotarli); uno è sotto il gatto: toccalo per farlo spostare.
- **122 Word Salad** — Tre indovinelli con le tessere delle lettere; l'ultimo: la parola sempre scritta sbagliata è WRONG.
- **123 Why The Chicken** — Frogger: porta il pulcino oltre 4 corsie di traffico fino alla mamma (tocca = salto avanti). 3 vite.
- **124 Paint Mixer** — Mescola le gocce di tempera per ottenere il colore del campione; l'ultimo è solo una parola: SALMONE.
- **125 Sweet Dreams** (Toni) — Toni dorme e sogna un osso: tra tanti oggetti, la soluzione è trascinare l'osso FUORI dal sogno davanti al suo naso.
- **126 Bubble Wrap** — Scoppia tutte le bolle del pluriball; l'ultima è sul retro: gira il foglio con uno swipe.
- **127 Out Of Tune** — Accorda le 3 corde della chitarra del gatto: pizzica, ascolta il diapason e gira la chiavetta.
- **128 Claw Machine** — Gru del luna park con 5 gettoni: togli il pesce di peluche sopra l'orsetto con la chiave, poi prendi l'orsetto.
- **Musica all'avvio**: se l'uscita audio si perde (es. dopo la richiesta del permesso microfono al primo avvio), al
  ritorno il brano viene riavviato dal punto in cui era; in più un controllo riavvia il brano se risulta "in play" ma
  fermo da più di un secondo.
- **Vibrazioni** in circa 40 livelli: esplosioni (forte), punture, morsi, cadute e rotture (media), atterraggi,
  colpi e scatti (leggera).
- Easter egg nuovi: *Ouroboros* (115) e *Master Builder* (109).

## Versione 0.38
- **100 TONI'S BIRTHDAY**: il finale con la torta sullo schermo è più chiaro. La torta si spiaccica sul vetro in tanti
  schizzi di panna (`cream_blob`), poi il **muso di Toni in primo piano** (`toni_face_big`) sale da sotto, *dietro* la
  panna, e si schiaccia contro il vetro; la **linguona** (`toni_tongue_big`) esce dalla bocca, punta lo schizzo più
  lontano e spazza di lato: ogni schizzo toccato si schiaccia e sparisce (whoosh + vibrazione leggera a ogni leccata),
  finché il vetro è pulito. Poi "*burp*", il muso scende e parte la festa.
- **68 MAGIC RUNE**: nuovo riconoscitore "a nuvola di punti" (`PointCloudRecognizer`, algoritmo $P) che guarda la
  forma e non l'ordine del tratto: la **stella** ora è accettata sia a pentagramma sia col contorno, da qualsiasi punta
  e in qualsiasi verso. Le rune passano da 5 a **13** (cerchio, triangolo, quadrato, zigzag, stella, spunta, spirale,
  cuore, infinito, onda, rombo, luna, montagne): a ogni partita ne escono 3 a caso. Il vecchio `ShapeRecognizer` è
  stato rimosso.
- **103 WHACK-A-MOLE**: tutte le buche delle talpe sono sul prato (prima la fila alta finiva nel cielo).
- **Niente titoli di coda**: il livello ROLLING CREDITS è stato tolto (e con lui l'Easter egg Credits Skipper), perché
  il gioco riceverà nuovi livelli a ogni aggiornamento. Ora i livelli sono 103.
- Versione del costruttore 41.

## Versione 0.37
- **21 LOW BATTERY (Easter egg Mecha Mode)**: sotto "GATTAI!!" compare il sottotitolo giapponese **合体！**
  (disegnato a pennellate, `gattai_jp`: il font del gioco non ha i caratteri giapponesi), con un piccolo rimbalzo.
  Nuove luci nella trasformazione: anelli di energia che pulsano durante il sovraccarico, colonna di luce dal cielo
  (`light_pillar`), raggiera che cambia colore come un arcobaleno, piccolo lampo e stelline a ogni pezzo d'armatura,
  lampo orizzontale da obiettivo (`light_streak`) al momento del lampo bianco, pioggia di stelline sulla scritta e
  scintilla "ting!" sulla visiera del robot da battaglia dopo l'atterraggio (con vibrazione forte al lampo).

## Versione 0.36
- **Cuori** ridisegnati: due lobi tondi e lati dritti tangenti fino alla punta (prima sembravano un cono gelato con
  due palline); sprite ad alta risoluzione, è anche il cuore delle vite.
- **64 BRIDGE BUILDER**: il topolino supera i piccoli dislivelli del ponte con un saltello (fino a 0,28 unità); si
  arrende solo se non avanza per 2 secondi.
- **83 WHERE'S MOUSE?**: i cappellini (rosso e blu) stanno sulla testa dei topolini, tra le orecchie.
- **100 TONI'S BIRTHDAY**: il cappellino di Toni appoggia in cima alla testa (perno sulla base del cappello).

## Versione 0.35
- **Toni è un carlino NERO**: pelo antracite, orecchie e muso scuri, pieghe, bocca, unghie e occhi chiusi disegnati
  in grigio chiaro per farli vedere; resta il collare rosso.
- **25 STUCK!** più chiaro e tollerante: all'inizio Toni dice "I'm STUCK! Push me!"; basta un **tocco** su Toni (o
  trascinarlo verso il muro) per spingerlo; burro, osso e piuma funzionano lasciati ovunque sopra di lui (prima
  bisognava centrare il sedere e trascinare verso destra, e sembrava che non succedesse niente).

## Versione 0.34
- **Hint a consumo**: ogni livello ha **3 suggerimenti** (`LevelDefinition.hints`, scritti in `Editor/LevelHints.cs`):
  una spinta, uno più preciso, la soluzione. Ognuno costa **1 hint** (`HintWallet`, saldo firmato come le vite);
  si parte con 5 e **non si ricaricano**: +1 guardando una pubblicità, pacchetti nel negozio (**3 hint 0,39 €**,
  **15 hint 1,49 €**: prodotti consumabili `gotcha.hints_3` e `gotcha.hints_15`, da creare anche su Google Play),
  oppure come premio per la condivisione. Un suggerimento sbloccato resta visibile per quel livello.
- **Easter egg con nome** (`Core/EasterEggs.cs`): il nome compare quando lo trovi e nel messaggio di condivisione.
  Ogni livello è registrato (video con audio), quindi dopo ogni segreto si può condividere il video; il testo
  contiene il **link personale** per scaricare il gioco (`InviteService.ShareLink`: codice unico del giocatore +
  segreto). La prima condivisione di ogni segreto fa scegliere il premio: **15 minuti di vite infinite** o **5 hint**.
- **Sveglia**: il vicino si sveglia dopo **8 secondi** in WAKE-UP CALL e **5 secondi** in SNOOZE ALARM.
- **Elenco livelli in Excel**: `python Tools/levels_xlsx.py` crea `Gotcha_livelli.xlsx` (numero, immagine, nome,
  soluzione, 3 hint, Easter egg con nome e come sbloccarlo; dati in `Tools/levels.json`). Per i veri screenshot:
  in Unity menu **Gotcha > Screenshot di tutti i livelli** (salva `Screenshots/level_###.png`), poi rilanciare lo script.

## Versione 0.33
- **Blind Stopwatch**: tolleranza portata a 0,25 s (almeno 2 decimi, più il ritardo del tocco).
- **Nuovi livelli di Toni** (25, 50, 75; il 100 dello starnuto resta):
  - **25 STUCK!**: Toni è incastrato nella gattaiola, testa in giardino e sedere in casa (zampe che pedalano). Spingerlo
    (trascina il sedere verso il muro) non basta: osso (si agita), piuma (ride) o **burro** (spalmato, una spinta e
    SCHLUPP!). Easter egg `toni_toot`: 8 spinte senza burro e parte a razzo.
  - **50 TUG OF WAR**: le chiavi sono sparite; dal muso di Toni pende un calzino. Tirandolo PIANO escono uno dopo
    l'altro pesce, pallina, moneta, campanello, salsicce, formaggio... e la chiave. Uno strattone e Toni tira indietro.
    Easter egg `toni_tug`: 5 strattoni e si rimangia tutto (*BURP*).
  - **75 WHERE'S TONI?**: nascondino tra divano, scatolone, cesto e tenda. Toni russa: muovendo il dito il russare è
    più forte e fitto vicino a lui (e il telefono vibra), e il nascondiglio giusto "respira". Ogni posto sbagliato ha
    una sorpresa; dopo un errore Toni può cambiare posto; 4 errori e vince lui. Nuovo suono `Sfx` "snore".
  - Tolti i livelli del pulsante, della foto e del frigo (e i loro disegni).
- **100**: la piuma ora è una **piuma di pavone** (occhio colorato = la punta che fa il solletico) infilata nel vaso.

## Versione 0.32
- **Blocco al livello 25 risolto**: quando Toni raggiungeva la pallina ancora in movimento, l'inseguimento girava
  all'infinito senza mai cedere il fotogramma (il gioco si bloccava). Ora Toni avanza un fotogramma alla volta
  (`ToniPug.StepToward`).
- **Toni è tutto beige**: orecchie e muso beige più scuro; neri restano solo naso e occhi.
- **Nessun livello finale fisso**: i titoli di coda ("ROLLING CREDITS") sono un livello normale e i livelli nuovi di
  ogni aggiornamento si aggiungono dopo. Toni va sempre nelle posizioni multiple di 25 (`PlaceToniLevels`): per il
  125 basta aggiungere un livello in `ToniLevels()` (se manca, il builder lo segnala). Finiti tutti i livelli: "You
  solved every level... for now! New levels arrive with every update."
- **Testi sempre al loro posto**: ogni etichetta ha `TextMeshFit` (non esce mai dallo schermo) e quelle dentro un
  oggetto hanno il loro riquadro (`Fit(label, larghezza, altezza)`): biglietto, display di timer/bomba/cassaforte/
  tastierino/cronometro, retro del quadro, pulsante OFF, lavagna, polaroid, barattolo.
- **Scritte col dito**: `FingerText` disegna lettere e cifre a tratti tondi e storti (Catmull-Rom, spessore che
  varia) e le fa stare sempre nel riquadro. Usata sullo specchio appannato (codice ed Easter egg "BRUSH YOUR TEETH!").

## Versione 0.31
- **Toni il carlino**: ogni 25 livelli c'è un livello di Toni (25, 50, 75, 100). I livelli che erano in quelle posizioni
  e tutti i successivi scalano avanti: ora i livelli sono **104** (vecchi 25-48 → 26-49, 49-72 → 51-74,
  73-96 → 76-99, 97-100 → 101-104; "THE END?" resta l'ultimo). Il carlino compare solo nei livelli di Toni.
  - **25 MOVE, TONI!**: Toni è sdraiato sul pulsante e non si sposta (spingerlo non serve). Bisogna lanciare la
    sua pallina lontano: lui la rincorre, e mentre è via il pulsante è libero.
  - **50 SAY CHEESE!**: foto a Toni che guarda nell'obiettivo. Lui guarda solo i biscottini: va appoggiato sopra la
    macchina fotografica. 4 pose; vicino alla bocca se lo mangia; sbatte le palpebre. Easter egg `toni_unicorn`:
    biscotto in equilibrio sulla testa.
  - **75 WAKE UP, TONI!**: sveglia, tocchi e scossoni non lo svegliano; aprire il frigo sì (trascina la porta).
    Easter egg `toni_toot`: suonare la sveglia 5 volte.
  - **100 TONI'S BIRTHDAY**: un carlino non sa soffiare: solletico al naso con la piuma finché starnutisce, la
    candela si spegne e la torta finisce spiaccicata sullo schermo (Toni la lecca via).
  - Codice: `Levels/Toni/ToniPug.cs` (sguardo, coda, pose sdraiato/in piedi, corsa, morso) e un controller per
    livello; costruzione in `Editor/GotchaLevelsToni.cs`; disegni in `DoodleSprites15.cs` e `DoodleSprites16.cs`.
- **3** e **19**: torna il gatto (col pesce nel livello 3), come prima della 0.30.
- **Video degli Easter egg con audio**: ogni livello viene registrato (buffer circolare degli ultimi 14 secondi,
  avviato da `GameFlow`), quindi la condivisione è offerta per **ogni** Easter egg. Il suono del gioco (musica ed
  effetti) viene preso dall'`AudioListener` (`ClipAudioTap`) e messo nel MP4 come traccia AAC da `GotchaVideo.java`.
  Condividere il video regala **una vita** (una volta per video). Nell'Editor la clip viene salvata come immagini +
  `audio.wav` in `persistentDataPath/clip_frames`.

## Versione 0.30
- **3**: il gatto diventa un **carlino** e il pesce un **osso**. Le pupille (a parte, `pug_pupil`) seguono sempre l'osso:
  nel quadro, mentre lo trascini, per terra; senza osso seguono il dito. Morso animato: accucciata, bocca spalancata,
  scatto in avanti "GNAM!", tre morsi scuotendo la testa con le briciole (l'osso si accorcia), deglutizione, leccata
  di baffi, coda che scodinzola, cuoricini, poi crolla addormentato. Abbaia (`Sfx` "woof").
- **19**: carlino al posto del gatto (occhi aperti/assonnati con le pupille, coda).
- **9**: scarafaggio più vivo: dondola e si schiaccia a ogni passo, si ferma ad annusare, trema e tossisce nella
  nuvola, la bomboletta vibra; muore barcollando, si ribalta, scalcia e ne esce l'animella.
- **16**: labirinto con trappole **circolare**: tre cerchi concentrici con i varchi alternati e due muri radiali, buche
  appoggiate alle pareti e laser di traverso nei corridoi; stella al centro (percorso verificato).
- **26**: l'uccellino vola più basso e più piano, sbatte le ali salendo, plana, si inclina e a metà schermo si ferma a
  mezz'aria per un attimo; fionda più potente e bersaglio più grande.
- **31**: ogni topolino mangia la sua fetta a morsi (briciole, guance piene) finché la fetta sparisce.

## Pulizia e spazio (0.29)
- Tolti dal catalogo dei disegni ~45 sprite non più usati (labirinti e oggetti dei livelli sostituiti) e i loro dati;
  tolti gli script mai usati (`EasterEggTrigger`, `SensorCondition`).
- Il builder ora **cancella da solo** le cartelle di `Assets/Levels` dei livelli non più nel catalogo e i PNG di
  `Assets/Art/Generated` che non sono più disegnati (`RemoveStaleAssets`).
- Texture su Android compresse **ASTC 4x4** (8 bit/pixel invece di 32): APK e memoria delle texture ~4 volte più piccoli.
- Musica in **OGG** invece di WAV nella repository (15,6 MB → 1,8 MB; nel gioco viene comunque ricompressa in Vorbis).
- Build Android: stripping del codice "Medium" + `stripEngineCode`; `Assets/link.xml` protegge gli assembly caricati
  per nome (store IAP) e il codice del gioco.
- Le cartelle generate (`Assets/Art/Generated`, `Levels`, `Prefabs`, `Materials`, `Scenes`) sono in `.gitignore`.
- Consiglio per la pubblicazione: caricare su Play un **App Bundle (.aab)**: ogni telefono scarica solo il codice della
  sua architettura (ARM64 o ARMv7).

## Versione 0.29
- **Titolo**: sfondo nero e logo "G!" al neon arcobaleno fatto come un labirinto (`DoodleSprites12.cs`,
  `UI/TitleLogo.cs`): due palline cromate ci rotolano dentro inclinando il telefono (frecce nell'Editor).
- **Musica al primo avvio**: i brani sono precaricati e ripartono quando l'app torna in primo piano (dopo la richiesta
  del microfono); il titolo richiede sempre il suo brano.
- **Vibrazione** (`Components/Haptics.cs`, Vibrator di Android): esplosioni (bomba, razzo, rune, fili), oggetti che si
  rompono (biscotto, palloncino), scosse dei laser, schianti, punture. `Haptics.Enabled` per spegnerla.
- **Vespe**: ovunque la prima puntura fa solo male (la vespa torna al suo posto), alla seconda si perde
  (52/83 con la paletta, labirinti 36/61/69/73, Bodyguard 12/46/72).
- **Labirinti senza vespe con pareti curve o inclinate** (`DoodleSprites13.cs`): 41 zig-zag, 16 ondulato, 48 a vortice,
  65 a bolle, 17 inclinato, 57 a vortice inverso. È una deformazione continua del labirinto originale: il percorso
  resta quello verificato; collider, buche, laser, cancelli e chiavi seguono la deformazione.
- **Easter egg**: 15 `candle_storm` (soglia più bassa, il soffio a scatti non azzera), 29 `sun_melt` (si scioglie più in
  fretta e l'egg viene assegnato anche se l'animazione si inceppa), 67 `bad_breath` (5 s, tollerante alle pause).
- **74**: la ragazza ridisegnata come una diva in abito da sera rosso.
- **Menu livelli**: i puntini delle pagine non si spostano più.
- Livelli nuovi (`Assets/Scripts/Levels/V29`, `GotchaLevels29.cs`, `DoodleSprites14.cs`): 80 Where's Mouse? (zoom con due
  dita) · 85 House of Cards (mano ferma, aspetta che la tenda si calmi) · 88 Merry-Go-Round (giroscopio: gira su te
  stesso; senza giroscopio si fa girare il dito; egg `dizzy_cat`) · 98 Blind Stopwatch (conta a mente; egg `speedrun`).

## Versione 0.28
- **Labirinti arcobaleno**: tutte le pareti al neon hanno una tinta che scorre in diagonale (`DoodleSprites.Rainbow`).
- **41 Zig Zag**: tre cancelli rosa che si aprono e si chiudono da soli (`Components/SlidingGate`).
- **52 Wasp Returns**: due punture = livello perso (si perde una vita e si ricomincia); colpire la vespa mentre si
  lancia blocca la puntura; la velocità cresce di +0.1 a colpo invece di +0.3 (`maxStings`, `speedPerHit`).
- **81 Tightrope** più equilibrato (meno instabile, raffiche più deboli, si cade a 60°). **86**: gli anelli attraversati
  diventano verdi. **91** sentiero più largo e comandi più lenti. **94** lucciole di tre colori (il barattolo ne vuole
  uno alla volta, colore sbagliato = ne scappa una), lampeggi brevi, occhi del lupo che fanno l'occhiolino.
- Livelli nuovi (script in `Assets/Scripts/Levels/V28`, scene in `Assets/Editor/GotchaLevels28.cs`, disegni in
  `DoodleSprites11.cs`): 54 Laser Pointer (specchi) · 71 DJ Mouse (ritmo a 128 bpm) · 77 Plumber (tubi) ·
  78 Fake Coin (bilancia) · 79 Spot It (differenze casuali) · 80 Counting Sheep (egg `fell_asleep`) ·
  82 Shadow Play (due dita: ruota e scala; Editor: Q/E, Z/X) · 89 Slide Puzzle · 95 Cheating Cat (memory) ·
  98 Traffic Jam (puzzle verificato, 22 mosse) · 100 The End? (titoli di coda, egg `credits_skip`).
- Tolti: Deep Freeze, Total Silence, Spiral, Heavy Safe, Scalding Soup, Giant Balloon, Quick Wire, Coffee Time,
  Cake Tower, Bowling, The Trophy (e i loro Easter egg `coffee_cat`, `polish_maniac`).

## Versione 0.27 (musica per tipo di livello)
- Brani in `Assets/Audio/Music` (loop a 128 bpm): **Title** nel menu all'apertura (e quando si torna al titolo),
  **Dungeon** per labirinti e livelli con chiavi, **Animals** per animali e natura, **Kitchen** per i livelli col cibo,
  **Pressure** per tutti gli altri. La scelta è in `GotchaProjectBuilder.MusicFor` (per nome della cartella) e finisce
  nel campo `music` di ogni `LevelDefinition` (si può cambiare a mano dall'Inspector).
- Cambio di livello: se il nuovo livello usa un brano diverso, quello in corso sfuma (0,7 s) e il nuovo entra in
  dissolvenza (0,9 s); se è lo stesso brano continua in loop senza interruzioni (`MusicPlayer.Play`).
- `Soundtrack.wav` resta solo per gli stereo dei livelli 7 e 19.

## Versione 0.26 (free to play, livelli nuovi)
### Acquisti in-app
- Due prodotti: `gotcha.infinite_lives_1h` (consumabile, **0,99 €**: 1 ora di vite infinite) e `gotcha.remove_ads`
  (non consumabile, **4,99 €**: niente pause pubblicitarie per sempre; la pubblicità con premio resta, è volontaria).
- Pacchetto Unity IAP `com.unity.purchasing` 4.12.2 (già in `Packages/manifest.json`). Il codice che lo usa
  (`Assets/Scripts/Store/Iap/UnityIapStore.cs`, assembly `Gotcha.Iap`) si compila solo se il pacchetto c'è
  (define `GOTCHA_IAP`): senza il pacchetto il gioco compila lo stesso e usa il negozio simulato.
- Nell'Editor il negozio è **simulato** (Unity Services non vengono avviati): ogni acquisto riesce subito (per provare vite infinite e niente pubblicità). Per la build vera collega il progetto in Project Settings → Services (Project ID).
  Su telefono senza IAP configurato gli acquisti falliscono con un messaggio.
- Per pubblicare: in Google Play Console → Monetizza → Prodotti in-app crea i due ID qui sopra con gli stessi prezzi;
  carica una build firmata su un canale di test; aggiungi i tester. In Unity: Services → collega il progetto
  (Unity Gaming Services) se richiesto da IAP 4.x. Il negozio si apre dal titolo (SHOP) e dal pannello delle vite.
- "RESTORE PURCHASES" ripristina "niente pubblicità" (su Android avviene anche in automatico all'avvio).
- Lo stato è salvato in PlayerPrefs (`gotcha.noads`) e nei dati delle vite (secondi di vite infinite rimasti).

### Livelli
- **66 Magic Rune**: la stella si accetta più facilmente (soglia 0.66, confronto anche col contorno della stella).
- **69 Pitch Black**: labirinto nuovo con anelli: la vespa si può aggirare.
- Ogni meccanica compare al massimo 3 volte; gli Easter egg non si ripetono (tolti dalle varianti).
- Livelli nuovi (script in `Assets/Scripts/Levels/V26`, scene in `Assets/Editor/GotchaLevels26.cs`, disegni in
  `DoodleSprites9.cs` e `DoodleSprites10.cs`, suoni sintetizzati in `Components/Sfx.cs`):
  60 Flashlight · 67 Foggy Mirror (egg `bad_breath`) · 81 Tightrope · 85 Snowman on Ice (egg `growth_spurt`) ·
  86 Paper Plane · 87 Safe Cracker · 88 Sing, Birdie (egg `space_bird`) · 89 Coffee Time (egg `coffee_cat`) · 90 Domino ·
  91 Mirror Path · 92 Magnet Fishing · 93 Cuckoo Clock (egg `lunch_time`) · 94 Fireflies · 95 Cake Tower ·
  96 Bubble Count · 97 Sneaky Mouse · 98 Bowling · 99 Whack-a-Mole (egg `cat_bonk`) · 100 The Trophy (egg `polish_maniac`).
- Tasti nell'Editor: B = soffio/voce, N = urlo/soffio fortissimo, S = scossone, frecce = inclinazione.

## Versione 0.24 (correzioni dopo il primo giro di prove)
- **Tutti i livelli sono sbloccati** nella selezione (`GameFlow.UnlockAllLevels`, da rimettere a `false` per il rilascio).
- **9 Bug Spray** (nuovo): tieni il dito sullo schermo, lo spray esce verso l'alto; 1.3 s nella nuvola uccidono lo
  scarafaggio, la bomboletta dura 7 s (`poisonNeeded`, `sprayCapacity`). La vespa con la paletta resta nei livelli 52 e 83.
- **10**: la chiave è appesa nel fondo scuro del quadro, negli stessi toni; forchetta e cucchiaio sul tavolo fanno da esca.
- **11**: alla fine gli occhi rientrano dal lato da cui sono usciti, con un arco, e cadono nella pozzanghera.
- **12/46/72**: barriere controllate anche mentre si disegnano; la vespa punge solo se tra lei e il gatto non c'è una
  linea. 72: due vespe, e le linee che non toccano il gatto vengono spinte via a ogni urto (`pushUnanchored`).
- **14/45/67**: gravità più "vera" (28), meno attrito dell'aria; il biscotto si spezza se sbatte oltre `breakImpact`.
- **15/49/93**: Easter egg `candle_storm` soffiando fortissimo (Editor: N; B = soffio normale).
- **21/91**: trasformazione in Mecha con fulmini, raggiera, pezzi d'armatura in volo, lampo, GATTAI!!, atterraggio.
- **22/79, 32**: urletto del topolino (`ScreamSynth.Mouse`). **32**: ricetta casuale (3-4 ingredienti) disegnata col gesso
  con i nomi; esattamente quelli, niente di più. Le fette spariscono quando i topi le mangiano. Fiamme e lingua animate.
- **25/56/87**: inchiostro limitato, staccionate più corte di 1.4 scavalcate, 25 s prima che si svegli il vicino.
- **26/50/76**: fionda più centrale (si può tirare troppo lungo), vento (manica a vento e strisce), uccellino di
  passaggio: Easter egg `angry_bird`. 76: raffiche di 1-4 s con pause di 2-4 s.
- **27**: la codina sbuca solo dopo il mescolamento. **28/82**: filo e indovinello casuali tra 6 colori. **29**: naso giusto.
- **35/42 e 38/58/92**: padella e cesto filtrati (niente tremolio; lo scossone non li sposta).
- **Labirinti**: colori che sfumano alle giunzioni, sfera più controllabile (gravità più dolce + attrito), buche più piccole.
- **Vespe nei labirinti** (36, 60, 61, 73, 94, 100 e 69 al buio): seguono la strada più breve lungo i corridoi
  (`MazeChaser`); labirinti aperti con anelli e qualche vicolo cieco; 61, 73 e 100 con due vespe. 69: la vespa dorme
  finché la luce non la tocca, e il ronzio cresce avvicinandosi.
- **39/53/95**: la biglia si ferma nella tazza. **55/84**: il topolino cambia tana ogni 3 s (2.5 nell'84).
- **59/96**: pioggia, ombrello da trascinare (troppo vicino brucia: Easter egg `burnt_umbrella`), il topolino congela.
- **62 Bridge Builder**, **66 Magic Rune** (riconoscimento del disegno, `ShapeRecognizer`), **74 Two Doors**: nuovi.
- **68 Five Beeps**: segnali da cinque suoni. **71**: a volume zero il microfono ascolta: un rumore sveglia il gatto (Editor: M).

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

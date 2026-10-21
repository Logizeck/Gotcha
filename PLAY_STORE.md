# Gotcha! su Google Play — checklist per la pubblicazione

Obiettivo: Gotcha! pubblico su Google Play entro il **25 ottobre 2026**.
Questa guida va seguita dall'alto in basso. Le parti che il codice fa già da solo sono segnate con ✅.

---

## ▶ Beta interna (versione 0.60) — passo passo

Obiettivo: caricare l'AAB nel canale **Test interno** di Play Console (fino a 100 tester, niente revisione lunga,
niente vincolo dei 12 tester/14 giorni, che vale solo per passare alla produzione).
Scelte fatte per questa beta: pacchetto **`com.gotcha.zeck`**, **niente acquisti in-app** (il pacchetto Unity IAP
è stato tolto: il negozio risponde "Store not available"), pubblicità reali assenti (nessun AdMob).

0. **Unity 6.3 LTS** (dalla versione 0.60 il progetto è per Unity 6.3): Unity Hub > Installs > Install Editor >
   **Unity 6.3 (LTS)** con **Android Build Support** + **OpenJDK** + **Android SDK & NDK Tools**. Poi in
   *Edit > Preferences > External Tools* lascia spuntate le quattro caselle "installed with Unity (recommended)".
   (Le versioni di Unity senza la correzione CVE-2025-59489 vengono rifiutate da Google Play: il menu della build
   AAB si rifiuta di partire con quelle.)
1. **Aggiorna il progetto**: `git pull`, poi in Unity Hub *Add > Add project from disk* (se serve) e apri il
   progetto con **Unity 6.3**. La prima apertura chiede di aggiornare il progetto alla nuova versione: **Continue**
   (ci vuole qualche minuto: Unity ricostruisce la cartella Library). Se compare il dialogo sull'Input ("Active
   Input Handling"), scegli di riavviare. All'apertura il builder
   rigenera da solo scena e livelli (versione del costruttore 52). Se non parte: menu **Gotcha > Build Scene and
   Levels**. In *Window > Package Manager* "In App Purchasing" non deve più esserci (lo toglie `Packages/manifest.json`).
2. **Android SDK Platform 36** (target API 36 obbligatorio): alla prima build Unity può proporre
   *"Update Android SDK"* → accetta. Se non lo propone e la build si lamenta di "android-36", da terminale:
   `"<SDK>/cmdline-tools/latest/bin/sdkmanager" "platforms;android-36" "build-tools;36.0.0"`
   (`<SDK>` è il percorso in *Edit > Preferences > External Tools > Android SDK*).
3. **Chiave di caricamento** (una volta sola, poi **conservala con backup**: senza non puoi aggiornare l'app;
   con Play App Signing si può solo chiedere a Google di reimpostarla). Da terminale, nella cartella del progetto,
   usando il keytool del JDK di Unity
   (`<Unity>/Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK/bin/keytool`):
   `keytool -genkeypair -v -keystore gotcha-upload.keystore -alias gotcha -keyalg RSA -keysize 2048 -validity 10000`
   Poi crea `ReleaseSigning.json` nella cartella del progetto (è già escluso da git):
   `{"keystorePath":"gotcha-upload.keystore","keystorePass":"...","keyAlias":"gotcha","keyAliasPass":"..."}`
4. *(una volta sola, Unity 6)* **File > Build Profiles > Android**: *Debug Symbols* = **Symbol Table** (così Play
   Console non segnala i simboli mancanti).
5. **Crea l'AAB**: menu **Gotcha > Build Google Play AAB** → salva `Builds/Gotcha-0.60.aab`
   (versionCode **6000**, pacchetto `com.gotcha.zeck`, firmato, define `GOTCHA_RELEASE`).
   Errori della finestra "Build Settings" (GUILayout) non c'entrano: usa sempre questo menu.
6. **Play Console**: *Crea app* → nome "Gotcha!", lingua predefinita Inglese (US), **Gioco**, **Senza costi**,
   accetta le dichiarazioni. Poi *Testa e rilascia > Test > Test interno*:
   - scheda **Tester**: crea un elenco email con i tuoi tester (anche solo il tuo Gmail) e salva;
   - **Crea nuova release** → accetta **Play App Signing** (consigliato) → carica `Gotcha-0.60.aab` →
     nome release "0.60 beta", note di rilascio brevi → *Avanti* → *Salva e pubblica*.
   - Torna su *Tester* e copia il **link di partecipazione**: i tester lo aprono col proprio account, accettano e
     installano dal Play Store (la prima volta può volerci qualche minuto).
7. **Sul telefono**: Impostazioni del gioco → **BETA** → password `Zeckrulez` → tutti i livelli aperti, vite e
   suggerimenti infiniti, niente pubblicità. Così, se un livello si blocca, si passa agli altri.
8. **Per ricaricare** una correzione: aumenta `GameUI.Version` (es. "0.60.1" → versionCode 6001) e ripeti il punto 5.

Se la build fallisce, copia il primo errore rosso della Console (spesso Gradle o SDK) e mandamelo.
Avvisi che Play Console può mostrare e che **non bloccano** il test interno: "nessun file di deoffuscamento"
(R8 è spento di proposito, vedi sezione 6: non c'è nulla da caricare), simboli di debug mancanti (dalla 0.54 il
menu li mette dentro l'AAB da solo),
dichiarazioni dei contenuti ancora da compilare (servono per la produzione, sezioni 7-10).

---

## 0. Da sapere subito (possono bloccare la data)

1. **Account sviluppatore personale e test chiuso obbligatorio.** Gli account Play Console *personali* creati
   dopo il 13 novembre 2023 non possono pubblicare in produzione finché non hanno fatto un **test chiuso con
   almeno 12 tester iscritti per 14 giorni consecutivi**, e poi chiesto l'accesso alla produzione (Google
   risponde di solito entro ~7 giorni). Con un account così l'uscita pubblica il 25 ottobre **non è possibile**:
   - se l'account è più vecchio (prima del 13/11/2023) o è un account **Organizzazione** (serve un D-U-N-S,
     richiede giorni/settimane), il vincolo non c'è;
   - altrimenti: **avvia il test chiuso OGGI** (vedi §8) con 12+ persone vere che restino iscritte e aprano il
     gioco; 14 giorni da oggi (19/10) portano al 2/11, più la revisione per la produzione → uscita realistica a
     metà novembre. Il 25 ottobre può essere la data del test chiuso "pubblicizzato" tra amici.
   - In alternativa a "production" si può valutare **Accesso anticipato (Early access)**, che però per gli account
     personali nuovi richiede comunque l'accesso alla produzione: non aggira il vincolo.
2. **Unity IAP 4.12.2 usa Google Play Billing Library 6** — Google dal 31/8/2026 accetta app nuove e
   aggiornamenti **solo con Billing Library 8 o successiva** (proroga richiedibile fino al 1/11/2026 dalla Play
   Console). Con il pacchetto attuale il caricamento dell'AAB verrà rifiutato (o segnalato) per gli acquisti in-app.
   Soluzioni, in ordine di preferenza:
   - **A.** Window > Package Manager > In App Purchasing → aggiorna all'ultima versione 5.x (usa Billing 8).
     La 5.x mantiene come "obsolete" le API usate in `Assets/Scripts/Store/Iap/UnityIapStore.cs`
     (`UnityPurchasing.Initialize`, `IDetailedStoreListener`): ricompila, correggi eventuali errori (anche i nomi
     degli assembly in `Gotcha.Iap.asmdef`), poi prova un acquisto vero col test interno.
   - **B.** Se A non funziona entro 2 giorni: rimuovi il pacchetto In App Purchasing (il gioco passa da solo al
     negozio "non disponibile": i pulsanti del negozio rispondono "Store not available") ed esci senza acquisti;
     li aggiungi con un aggiornamento.
3. **Target API 36** (Android 16) obbligatorio per le app nuove dal 31/8/2026. ✅ Impostato dal codice. Serve la
   piattaforma Android 36 nell'SDK: con Unity 6 aggiornato c'è già; con Unity 2022.3 apri Android SDK Manager
   (o lascia che Unity proponga "Update Android SDK") e installa *Android SDK Platform 36*. Se la build fallisce con
   errori di SDK, usa Unity 6 (consigliato).
4. **Pagine di memoria da 16 KB**: richieste per le app con target Android 15+. Unity 6 e Unity 2022.3.56+ le
   supportano; il plugin AdMob va preso aggiornato. Play Console mostra un avviso se qualcosa non va.

---

## 1. Account Google Play Console

- [ ] Crea (o usa) l'account su <https://play.google.com/console> — 25 $ una tantum, verifica identità.
- [ ] Se puoi, account **Organizzazione** (vedi §0.1). Per un'attività in Italia serve D-U-N-S (gratuito, giorni).
- [ ] Profilo pagamenti (per vendere IAP): Play Console > Impostazioni > Profilo pagamenti, con IBAN e dati fiscali.
- [ ] Indirizzo email di contatto pubblico per la scheda dello store (diventa pubblico).

## 2. Nome del pacchetto (decidere PRIMA del primo caricamento)

Il nome del pacchetto (application ID) è l'identità dell'app su Play: **deve essere unico e non si può più
cambiare dopo il primo caricamento**. ✅ Scelto: **`com.gotcha.zeck`** (in `InviteService.PackageName`).

- [ ] Scegli qualcosa di tuo, ad es. `com.<tuonome>.gotcha` (solo minuscole, cifre, punti e `_`).
- [ ] Cambialo in **un solo punto**: `Assets/Scripts/Core/InviteService.cs` → `PackageName`. Da lì lo prendono il
      link allo store degli inviti e le impostazioni Android (`GotchaBuildMenu.ApplyStoreSettings`).
- Nota: le classi Java `com.gotcha.game.GotchaKeys` e `com.gotcha.video.GotchaVideo` hanno un *package Java*
  indipendente dal nome dell'app: **non vanno rinominate**.

## 3. Chiave di caricamento (upload key) e Play App Signing

Google firma l'app per gli utenti (Play App Signing, obbligatorio per gli AAB); tu firmi i caricamenti con una tua
**chiave di caricamento**. Se la perdi, Google la può reimpostare, ma ci vogliono giorni: tienine copie.

- [ ] Crea il keystore (una volta sola), **fuori** dalla cartella del progetto:
  ```
  keytool -genkeypair -v -keystore gotcha-upload.keystore -alias gotcha -keyalg RSA -keysize 2048 -validity 10000
  ```
  (`keytool` è nel JDK; quello di Unity è in `<Unity>/Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK/bin`.)
  In alternativa: Player Settings > Publishing Settings > Keystore Manager > Create new.
- [ ] Crea `ReleaseSigning.json` nella cartella del progetto (accanto ad `Assets/`):
  ```json
  {"keystorePath":"C:/Chiavi/gotcha-upload.keystore","keystorePass":"...","keyAlias":"gotcha","keyAliasPass":"..."}
  ```
  Il file è in `.gitignore` (anche `*.keystore`/`*.jks`): **mai** in git, mai condiviso.
- [ ] Salva keystore + password in un posto sicuro (password manager + copia offline).
- [ ] Al primo caricamento accetta **Play App Signing** (chiave generata da Google).

## 4. AdMob (pubblicità vere) — facoltativo ma previsto

Senza AdMob la build per lo store **non mostra pubblicità** (`NoAdsProvider`: niente spot finti, i pulsanti
"WATCH AD" spariscono, restano negozio e inviti). Per attivarla:

- [ ] Crea l'account su <https://admob.google.com>, aggiungi l'app Android (anche prima che sia sullo store) e
      crea due unità pubblicitarie: **Interstitial** e **Rewarded** (premio: 1 "life").
- [ ] Scarica e importa il plugin **Google Mobile Ads for Unity** (ultima versione, ≥ v9) da
      <https://github.com/googleads/googleads-mobile-unity/releases> (Assets > Import Package > Custom Package).
      Importa anche l'External Dependency Manager se richiesto, poi
      Assets > External Dependency Manager > Android Resolver > **Force Resolve**.
      Se richiesto, in Player Settings > Publishing Settings attiva *Custom Main Gradle Template* e
      *Custom Gradle Properties Template*.
- [ ] Assets > Google Mobile Ads > Settings: incolla l'**App ID** Android (`ca-app-pub-XXXX~YYYY`).
- [ ] Player Settings > Android > Other Settings > Scripting Define Symbols: aggiungi `GOTCHA_ADMOB`.
- [ ] In `Assets/Scripts/Ads/AdMobProvider.cs` sostituisci `InterstitialId` e `RewardedId` (ora sono gli ID di
      PROVA di Google) con i tuoi. Il menu di build ti avvisa se trova ancora gli ID di prova.
      Durante i test sul tuo telefono usa gli ID di prova (cliccare annunci veri sui propri telefoni può far
      sospendere l'account AdMob).
- [ ] AdMob > Privacy e messaggi > **GDPR**: crea e pubblica il messaggio di consenso (europeo), collega l'app e
      il link alla privacy policy. Senza questo messaggio il modulo UMP non compare e in UE non arrivano annunci.
- [ ] Crea `app-ads.txt` sul dominio del sito sviluppatore (consigliato, non obbligatorio): AdMob ti dà la riga.

Cosa fa il codice (✅): chiede il consenso UMP (`ConsentInformation.Update` → `LoadAndShowConsentFormIfRequired`)
**prima** di avviare AdMob; carica sempre un interstitial e un rewarded e ricarica dopo ogni visione (con
tentativi a intervalli crescenti se la rete manca); il pulsante **PRIVACY OPTIONS** compare nelle Impostazioni
quando UMP lo richiede (utenti UE). Nell'Editor resta la pubblicità simulata.

Regola delle pause: pubblicità ogni 5 livelli superati o dopo 3 minuti di gioco attivo, mai durante un livello;
l'acquisto "remove ads" le toglie (restano solo quelle con premio, volontarie).

## 5. Acquisti in-app da creare in Play Console

Play Console > (app) > Monetizza > Prodotti > **Prodotti in-app**. Gli ID devono essere identici:

| ID prodotto | Tipo (Play: sempre "prodotto in-app") | Uso nel gioco | Prezzo base IT |
|---|---|---|---|
| `gotcha.infinite_lives_1h` | consumabile | 1 ora di vite infinite | 0,99 € |
| `gotcha.remove_ads` | **non consumabile** (si compra una volta, torna dopo la reinstallazione) | niente pause pubblicitarie, per sempre | 4,99 € |
| `gotcha.hints_3` | consumabile | 3 suggerimenti | 0,39 € |
| `gotcha.hints_15` | consumabile | 15 suggerimenti | 1,49 € |

Su Google Play "consumabile/non consumabile" non si imposta nella Console: lo decide il codice (✅ già fatto in
`UnityIapStore`: i consumabili vengono consumati, `remove_ads` no e viene ripristinato all'avvio).
I prodotti si possono creare solo **dopo** aver caricato un AAB con la libreria di fatturazione (anche solo nel
test interno). Imposta il prezzo in EUR e lascia che Play converta per gli altri paesi.
Per provare gli acquisti senza pagare: Play Console > Impostazioni > **Test delle licenze**, aggiungi gli account
Gmail dei tester.

## 6. Build dell'AAB

1. [ ] Aggiorna `GameUI.Version` (in `Assets/Scripts/UI/GameUI.cs`) per ogni caricamento.
   **Regola versionCode** (automatica): `major*10000 + minor*100 + patch` → "0.39" = 3900, "0.39.1" = 3901,
   "1.0" = 10000. Minor e patch tra 0 e 99. Play rifiuta un versionCode già usato: per ricaricare la stessa
   versione usa una patch ("0.39.1").
2. [ ] Menu **Gotcha > Build Google Play AAB**. Il menu:
   - legge la firma da `ReleaseSigning.json` (se manca spiega come crearla e si ferma);
   - imposta pacchetto, versione, API min 24 / target 36, IL2CPP, ARM64 + ARMv7;
   - compila con il define `GOTCHA_RELEASE` (solo per quella build: livelli bloccati finché non li risolvi,
     niente pubblicità simulata) e firma l'AAB; dopo rimette le impostazioni di prima.
3. Le build di prova restano quelle di sempre: **Gotcha > Build Android APK** / **Build And Run on Phone** (tutti i
   livelli sbloccati, pubblicità simulata).
4. Facoltativo: in Build Settings imposta *Debug Symbols = Public* per avere i report di crash leggibili in Play
   Console (altrimenti compare solo un avviso, non blocca).
5. Minify (R8) è lasciato **spento** di proposito: eliminerebbe le classi Java chiamate per nome dal gioco.

## 7. Scheda dello store

### Grafica (cartella `Store/`)
- ✅ Icona 512×512 PNG: `Store/icon_512.png`
- ✅ Grafica in evidenza 1024×500: `Store/feature_graphic.png`
- [ ] Almeno **2 screenshot telefono** (consigliati 4–8), verticali 1080×1920 o 1080×2400, senza barre di stato
      finte. Fai screenshot dal telefono di 4–6 livelli belli (es. Make a Wish, Tilt Maze, Snowman, un livello di
      Toni, la schermata del titolo). Non mostrare annunci di prova negli screenshot.
- [ ] Facoltativo: video YouTube (gameplay 30 s).

### Testi in inglese (lingua predefinita consigliata: en-US)

**Titolo** (≤30): `Gotcha! Trick Puzzle Game`

**Descrizione breve** (≤80): `150 trick puzzles: tilt, blow, shake. Every level is a gotcha!`

**Descrizione completa** (≤4000):

```
Think outside the screen. In Gotcha!, every level is a gotcha.

150 hand-drawn trick puzzles where the obvious answer is almost never the right one. Read the title twice, look at every corner, and don't trust the big red button. Sometimes the solution is on the screen... and sometimes it's in your hands.

USE YOUR WHOLE PHONE
• Tilt it to roll a ball through neon mazes
• Blow into the microphone to put out birthday candles
• Shake it to find a hidden code
• Turn it upside down, press the volume buttons, wait, or do nothing at all

MEET TONI
Toni the black pug shows up every 25 levels with a puzzle of his own. He is very good at sleeping and very bad at sharing his ball.

HUNT FOR EASTER EGGS
Many levels hide a secret second ending. Find them all, record the funny moment and share it with your friends.

FEATURES
• 150 levels, from gentle brain teasers to devious tricks
• Cute doodle art, cats, wasps, snowmen and a very sleepy neighbour
• Hints for every level, from a little nudge to the full solution
• Lives that refill over time, no account needed
• Short levels, perfect for a quick break

Gotcha! is free to play. It contains ads and optional in-app purchases (infinite lives, remove ads, hint packs).
The microphone is used only to detect blowing, on your device: nothing is recorded or sent.

Can you beat them all without getting caught?
```

### Testi in italiano (it-IT)

**Titolo** (≤30): `Gotcha! Enigmi a trabocchetto`

**Descrizione breve** (≤80): `150 enigmi a trabocchetto: inclina, soffia, scuoti. Ogni livello è un tranello!`

**Descrizione completa** (≤4000):

```
Pensa fuori dallo schermo. In Gotcha! ogni livello è un tranello.

150 enigmi disegnati a mano in cui la risposta ovvia non è quasi mai quella giusta. Rileggi il titolo, guarda in ogni angolo e non fidarti del grosso pulsante rosso. A volte la soluzione è sullo schermo... e a volte è nelle tue mani.

USA TUTTO IL TELEFONO
• Inclinalo per guidare una pallina nei labirinti al neon
• Soffia nel microfono per spegnere le candeline
• Scuotilo per scoprire un codice nascosto
• Capovolgilo, premi i tasti del volume, aspetta... o non fare proprio niente

ECCO TONI
Toni, il carlino nero, arriva ogni 25 livelli con un enigma tutto suo. È bravissimo a dormire e pessimo a prestare la sua pallina.

CACCIA AGLI EASTER EGG
Molti livelli nascondono un secondo finale segreto. Trovali tutti, registra il momento più buffo e condividilo con gli amici.

CARATTERISTICHE
• 150 livelli, dai rompicapi semplici ai tranelli più perfidi
• Disegni simpatici: gatti, vespe, pupazzi di neve e un vicino molto assonnato
• Suggerimenti per ogni livello, da un piccolo aiuto alla soluzione completa
• Vite che si ricaricano col tempo, nessun account richiesto
• Livelli brevi, perfetti per una pausa

Gotcha! è gratuito. Contiene pubblicità e acquisti in-app facoltativi (vite infinite, niente pubblicità, pacchetti di suggerimenti).
Il microfono serve solo a riconoscere quando soffi, sul telefono: niente viene registrato o inviato.

Riuscirai a superarli tutti senza cadere in trappola?
```

### Categoria e contatti
- [ ] Tipo: **Gioco**, categoria **Rompicapo** (Puzzle). Tag: Brain teaser, Puzzle, Casual, Offline.
- [ ] Email di contatto, sito (facoltativo), **URL privacy policy** (§9).

## 8. Test prima della produzione

1. [ ] **Test interno** (subito, fino a 100 tester, disponibile in pochi minuti): carica il primo AAB, crea i
       prodotti IAP, prova acquisti con account in "Test delle licenze".
2. [ ] **Test chiuso** (obbligatorio per gli account personali nuovi, §0.1): crea una traccia "Closed testing",
       aggiungi **almeno 12 tester** (Google Group o lista email), condividi il link di iscrizione, e verifica in
       Play Console che risultino **iscritti** (opt-in). Devono restare iscritti **14 giorni di fila**: chiedi di
       giocare e lasciare feedback (Google chiede come hai usato i feedback nel modulo di accesso alla produzione).
3. [ ] Dopo i 14 giorni: Dashboard > **Richiedi l'accesso alla produzione** e compila il questionario.
4. [ ] Produzione: rilascio graduale consigliato (es. 20% → 100% dopo 2–3 giorni senza crash).

### Checklist "prova prima di caricare"
- [ ] Build AAB fatta col menu **Build Google Play AAB** (non un APK di prova); versionCode più alto del precedente.
- [ ] Installata dal test interno di Play (non a mano) su almeno 2 telefoni (uno vecchio, Android 7–9, uno recente).
- [ ] Prima apertura in Italia: compare il modulo di consenso; "Privacy options" nelle Impostazioni lo riapre.
- [ ] Livelli bloccati come previsto (nella build per lo store solo il primo è aperto).
- [ ] Permesso microfono: chiesto, livello "Make a Wish" funziona; negando il permesso il gioco non si blocca.
- [ ] Pubblicità: pausa dopo 5 livelli; "WATCH AD: +1 LIFE" dà la vita; in modalità aereo i pulsanti spariscono
      senza errori.
- [ ] Acquisti: ognuno dei 4 prodotti con un account di test; "remove ads" torna dopo disinstallazione e
      reinstallazione; prezzi in valuta locale sui pulsanti.
- [ ] Condivisione di un Easter egg (video in galleria + menu di condivisione) e "Invite a friend".
- [ ] Tasto Indietro, rotazione bloccata in verticale, notch / bordi (Android 15+ è edge-to-edge).
- [ ] Nessun crash nel *Pre-launch report* di Play Console (si genera da solo dopo il caricamento).

## 9. Privacy policy

- ✅ Testo pronto: `docs/privacy-policy.html` (inglese + italiano). Sostituisci `[EMAIL]` e `[DEVELOPER NAME]` /
  `[NOME SVILUPPATORE]` con i tuoi dati.
- [ ] Pubblicala con **GitHub Pages**: repository su GitHub > Settings > Pages > Source: branch `main`, cartella
      `/docs`. L'URL sarà `https://<utente>.github.io/<repo>/privacy-policy.html`. (Va bene anche Google Sites o
      qualsiasi pagina pubblica, non un PDF.)
- [ ] Inserisci l'URL in Play Console (Contenuti dell'app > Norme sulla privacy) e nel messaggio GDPR di AdMob.

## 10. Contenuti dell'app (Play Console > Policy > Contenuti dell'app)

### Classificazione dei contenuti (questionario IARC)
- Categoria: **Gioco**.
- Violenza: solo **cartoon/fantasy lieve** (si spruzza uno scarafaggio, una vespa, un pupazzo di neve si scioglie;
  nessun sangue, nessuna violenza realistica). Rispondi Sì a "violenza fantastica/cartoon" se chiesto, No al resto.
- Paura, sesso, linguaggio volgare, droghe, gioco d'azzardo: **No**.
- Interazione tra utenti / chat / condivisione posizione: **No**. (La condivisione dei video avviene tramite il menu
  di Android verso altre app: non è interazione nell'app.)
- Acquisti digitali: **Sì**. Pubblicità: dichiarata a parte.
- Risultato atteso: PEGI 3–7 / ESRB Everyone.

### Pubblico di destinazione
- Consiglio: fascia **13–15, 16–17, 18+** (non selezionare fasce sotto i 13 anni).
- Perché: se includi under 13 l'app entra nel programma **Famiglie**: annunci solo da SDK certificati per
  famiglie con contenuti adatti ai bambini, niente ID pubblicità, consenso dei genitori (COPPA/GDPR-K), revisione più
  severa e più lenta. Lo stile cartoon può far chiedere a Google "attira i bambini?": rispondi in modo coerente
  (gioco di enigmi per ragazzi e adulti) e, se richiesto, spiega che non è progettato per bambini.

### Pubblicità
- "La tua app contiene annunci?" → **Sì** (anche senza AdMob attivo al primo rilascio conviene dichiararlo solo
  quando gli annunci ci sono davvero: se esci con `NoAdsProvider` rispondi **No** e cambia la risposta
  all'aggiornamento con AdMob).

### Accesso all'app
- Nessun login: "Tutte le funzionalità sono disponibili senza accesso speciale".

### ID pubblicità
- Con AdMob: **Sì, l'app usa l'ID pubblicità** → scopo **Pubblicità o marketing** (il plugin aggiunge il permesso
  `AD_ID`). Senza AdMob: No.

### App governative, finanziarie, sanitarie, notizie, COVID: tutte **No**.

### Sicurezza dei dati (Data safety)
Risposte consigliate con AdMob + IAP attivi:

- **L'app raccoglie o condivide dati utente?** Sì (per via degli SDK di Google/Unity).
- **Tutti i dati sono criptati in transito?** Sì (gli SDK usano HTTPS).
- **Gli utenti possono chiedere la cancellazione?** Non ci sono account: puoi rispondere che i dati non sono legati a
  un account e indicare l'email per le richieste.
- Tipi di dati:
  | Dato | Raccolto | Condiviso | Scopo | Note |
  |---|---|---|---|---|
  | **ID dispositivo o altri ID** (ID pubblicità, app set ID) | Sì | Sì (Google, pubblicità) | Pubblicità, analisi, prevenzione frodi | AdMob |
  | **Posizione approssimativa** (da IP) | Sì | Sì | Pubblicità, prevenzione frodi | AdMob |
  | **Interazioni con l'app** (visualizzazioni/tocchi annunci) | Sì | Sì | Pubblicità, analisi | AdMob |
  | **Diagnostica / log arresti anomali** | Sì | No | Analisi | AdMob/Unity |
  | **Cronologia acquisti** | Sì | No | Funzionalità dell'app | Google Play Billing / Unity IAP |
  | **Audio / registrazioni vocali** | **No** | No | — | il microfono è letto solo sul telefono (volume del soffio): nessuna registrazione, nessun invio. I dati elaborati solo sul dispositivo non vanno dichiarati |
  | Foto e video | No | No | — | il video dell'Easter egg è creato solo se l'utente decide di condividerlo, tramite il menu di Android: non arriva a noi |
  | Informazioni personali, contatti, posizione precisa, salute, messaggi, file | No | No | — | |
- Senza AdMob al primo rilascio restano solo "Cronologia acquisti" (e diagnostica Unity, se presente).
- Riferimenti: guida "Data safety" di AdMob e di Unity IAP (cercale prima di compilare: aggiornano le liste).

## 11. Prezzi e paesi
- [ ] Prezzo: **Gratis** (scelta definitiva: un'app gratuita non può diventare a pagamento).
- [ ] Paesi: tutti, oppure inizia con Italia + paesi UE/EN. I testi sono in inglese: va bene ovunque.
- [ ] Contiene annunci: Sì/No coerente con §10.

## 12. Note di rilascio 0.39 (≤500 caratteri)

EN:
```
First release! 150 trick puzzles where every level is a gotcha. Tilt, blow, shake and think outside the screen. Meet Toni the pug every 25 levels and hunt for hidden easter eggs to share with your friends.
```
IT:
```
Prima versione! 150 enigmi a trabocchetto in cui ogni livello è un tranello. Inclina, soffia, scuoti e pensa fuori dallo schermo. Incontra il carlino Toni ogni 25 livelli e scova gli Easter egg nascosti da condividere con gli amici.
```

## 13. Calendario consigliato

| Quando | Cosa |
|---|---|
| 19–20 ott | Account Play, nome pacchetto, keystore, privacy policy online, aggiornamento Unity IAP (§0.2) |
| 20 ott | Primo AAB nel **test interno**, prodotti IAP, prove acquisti; **avvio test chiuso** con 12+ tester |
| 21–22 ott | AdMob: plugin, App ID, ID unità, messaggio GDPR; nuovo AAB (0.39.1) in test chiuso |
| 22–24 ott | Scheda store, screenshot, Data safety, classificazione, pubblico, annunci |
| 25 ott | Account vecchio/organizzazione: invio in **produzione** (revisione: da poche ore a ~7 giorni). Account personale nuovo: annuncio del test chiuso |
| 3 nov circa | (Account personale nuovo) fine dei 14 giorni → richiesta accesso produzione → uscita |

Nota: anche con un account senza vincoli, la **prima revisione** di un'app nuova può richiedere diversi giorni:
invia in produzione il prima possibile, non il 25.


## Notifiche (dalla versione 0.42)
- Il gioco usa il pacchetto **Mobile Notifications** di Unity (`com.unity.mobile.notifications`, già nel
  `Packages/manifest.json`) per un promemoria locale "I'm missing you! <3" 24 ore dopo l'ultimo accesso, solo tra le
  08:00 e le 22:00. Nessun server, nessun dato inviato.
- Su Android 13+ serve il permesso `POST_NOTIFICATIONS`: il gioco lo chiede dopo il primo livello superato. Il
  giocatore può spegnere i promemoria da Impostazioni → REMINDERS.
- In Unity: Edit → Project Settings → Mobile Notifications → Android: aggiungi un'icona piccola (bianca su
  trasparente, es. il "G!" del logo) con id `icon_0`, altrimenti Android mostra un quadratino.
- Data safety: le notifiche sono locali, non serve dichiarare raccolta dati per questo.

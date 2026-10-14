# Gotcha! — v0.2 (primo livello giocabile)

Progetto Unity 2022.3 LTS, C#, 2D (Built-in Render Pipeline). Grafica ancora segnaposto, generata da codice.

## Come aprirlo
1. Unity Hub > **Add project from disk** > seleziona questa cartella `Gotcha/` (Unity 2022.3 LTS, con Android Build Support).
2. Alla prima apertura lo script `Assets/Editor/GotchaProjectBuilder.cs` genera da solo sprite segnaposto,
   prefab del livello, LevelDefinition, LevelCatalog, scena `Assets/Scenes/Main.unity` e impostazioni Android.
   Si può rilanciare da **Gotcha > Build Scene and Levels**. Attenzione: ricrea la scena e il prefab del pupazzo.
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
- `Levels/Snowman/SnowmanLevel` livello 1.
- `Components/` Draggable2D, DropSlot, PinchScalable, TiltGravity, GoalZone, IdleTimer, SensorCondition, EasterEggTrigger.
- `UI/GameUI` interfaccia costruita da codice: barra (MENU, titolo, HINT), pannello Hint, Menu (livelli sbloccati,
  conteggio Easter egg), esito livello (SOLVED! / GOTCHA!), fine gioco. Il tasto Indietro di Android apre/chiude il menu.
- `Editor/GotchaProjectBuilder` genera scena, prefab e asset.

## Livello 1 — Snowman
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

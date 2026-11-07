using System.Collections;
using UnityEngine;
using Gotcha.Components;
using Gotcha.Levels.V64;

namespace Gotcha.Levels
{
    /// <summary>
    /// Livello 299 "The Impossible Print" (id lost_sheet) — "PRINT SOMETHING." Nello stesso ufficio di PAPERWORK
    /// NIGHTMARE c'è una scrivania con una stampante senza carta (spia rossa, schermino "PAPER EMPTY" / "INSERT PAPER"),
    /// il vassoio vuoto, il pulsante PRINT, una cucitrice e un cestino. Dentro il livello non c'è nessun foglio: ogni
    /// tentativo fallisce in modo buffo e non blocca niente (PRINT: "PAPER JAM. Just kidding. NO PAPER."; vassoio vuoto;
    /// cestino "Nothing useful in here."; scuotere il telefono: la stampante sbatacchia; frasi a rotazione; dopo qualche
    /// tentativo una battuta-indizio). Mai sconfitta.
    /// Soluzione (Easter egg fra due livelli): in PAPERWORK NIGHTMARE si trascina un foglio oltre il bordo destro dello
    /// schermo; quel livello salva subito la preferenza LostSheetLogic.CarriedKey = 1. Qui, se la preferenza c'è, il foglio
    /// entra svolazzando da sinistra e si posa sulla scrivania ("...where did THAT come from?"); trascinato sulla stampante
    /// (zona generosa: tutta la stampante e sopra) viene tirato dentro dalla fessura in cima, la stampante stampa ed esce
    /// un foglio con l'uovo d'oro e "You rock!"; poi la festa comune e WinWithEgg("you_rock") (la sequenza epica
    /// dell'uovo con la fanfara la fa GameUI). La preferenza NON si consuma: ogni nuova partita ha di nuovo il foglio.
    /// Foglio lasciato altrove: torna sulla scrivania. La logica è in Gotcha.Levels.V64.LostSheetLogic.
    /// Tasti di prova nell'Editor: C porta il foglio (preferenza = 1) e ricomincia, X toglie la preferenza e ricomincia,
    /// 1 PRINT, 2 vassoio, 3 cestino, 4 cucitrice, 5 stampante, S scossone, F foglio nella stampante, D foglio lasciato
    /// altrove, O pausa simulata (dito annullato), R ricomincia.
    /// </summary>
    public class V89LostSheetLevel : LevelController
    {
        [Header("Stampante")]
        [SerializeField] Transform printerBody;      // la parte che trema (vassoio + corpo + pannello)
        [SerializeField] Transform tray;             // il vassoio d'ingresso vuoto (rimbalzo)
        [SerializeField] Transform printButton;      // il pulsante PRINT (rimbalzo)
        [SerializeField] TextMesh lcd;               // lo schermino
        [SerializeField] SpriteRenderer lamp;        // bagliore sopra la spia (rosso / verde)
        [SerializeField] Collider2D printArea;
        [SerializeField] Collider2D trayArea;
        [SerializeField] Collider2D printerArea;
        [SerializeField] Collider2D feedZone;        // dove lasciare il foglio (generosa: tutta la stampante e sopra)
        [SerializeField] Transform feedPoint;        // sopra la fessura d'ingresso (mondo)
        [SerializeField] Transform slotPoint;        // dentro la fessura, nascosto dal corpo
        [SerializeField] Transform outPoint;         // la fessura d'uscita (bordo alto del foglio stampato)
        [Header("Scrivania")]
        [SerializeField] Transform stapler;
        [SerializeField] Collider2D staplerArea;
        [SerializeField] Transform bin;
        [SerializeField] Collider2D binArea;
        [Header("Il foglio portato da PAPERWORK NIGHTMARE")]
        [SerializeField] Transform sheet;
        [SerializeField] Collider2D sheetArea;
        [SerializeField] SpriteRenderer sheetArt;
        [SerializeField] Transform sheetRest;        // dove si posa sulla scrivania (posizione e rotazione)
        [SerializeField] Transform sheetFrom;        // da dove entra svolazzando (fuori a sinistra)
        [SerializeField] int sheetOrder = 30, dragOrder = 45, feedOrder = 9;
        [Header("Il foglio stampato")]
        [SerializeField] Transform page;
        [SerializeField] TextMesh pageText;
        [SerializeField] SpriteRenderer pageGlow;
        [SerializeField] SpriteRenderer[] sparkles;
        [SerializeField] Transform pageShow;         // dove si mostra in grande
        [SerializeField] float pageShowScale = 1.75f;
        [Header("Scritte e festa")]
        [SerializeField] TextMesh infoText;
        [SerializeField] Transform infoHolder;
        [SerializeField] LevelReward reward;

        const string Compliment = "YOU ROCK!";
        const float PageHalfH = 0.56f;
        static readonly Color LampRed = new Color(1f, 0.25f, 0.2f, 1f), LampGreen = new Color(0.35f, 1f, 0.45f, 1f), LampAmber = new Color(1f, 0.75f, 0.2f, 1f);

        LostSheetLogic logic;
        Camera cam;
        HwToast toast;
        HwDragger dragger;
        HwDragger.Item sheetItem;
        bool captured, finishing, overFeed;
        Vector3 bodyHome, trayScale, buttonScale, staplerScale, binScale, pageScale;
        Vector3 feedFrom;
        Quaternion feedFromRot;
        float buttonT = 1f, trayT = 1f, staplerT = 1f, binT = 1f, rattleT = 1f, blinkT = 1f, printTickT;
        string lcdShown;

        public override void OnLevelStart()
        {
            cam = Camera.main;
            StopAllCoroutines();
            if (!captured)
            {
                captured = true;
                logic = new LostSheetLogic();
                bodyHome = printerBody.localPosition;
                trayScale = tray != null ? tray.localScale : Vector3.one;
                buttonScale = printButton != null ? printButton.localScale : Vector3.one;
                staplerScale = stapler != null ? stapler.localScale : Vector3.one;
                binScale = bin != null ? bin.localScale : Vector3.one;
                pageScale = page.localScale;
                dragger = new HwDragger(this);
                sheetItem = dragger.Add(sheet, sheetArea, 1);
                sheetItem.Lift = 1.1f;
            }
            dragger.Release();
            sheetItem.Home = sheetRest.position;
            bool carried = false;
            try { carried = PlayerPrefs.GetInt(LostSheetLogic.CarriedKey, 0) == 1; } catch { carried = false; }
            logic.Start(carried);
            finishing = overFeed = false;
            buttonT = trayT = staplerT = binT = rattleT = blinkT = 1f;
            printTickT = 0f;
            lcdShown = null;
            toast = new HwToast(infoText, infoHolder);
            if (reward != null) reward.ResetReward();

            printerBody.localPosition = bodyHome;
            if (tray != null) tray.localScale = trayScale;
            if (printButton != null) printButton.localScale = buttonScale;
            if (stapler != null) stapler.localScale = staplerScale;
            if (bin != null) bin.localScale = binScale;
            sheet.localScale = Vector3.one;
            sheetArt.sortingOrder = sheetOrder;
            sheet.gameObject.SetActive(carried);
            if (carried) { PlaceArriving(0f); Sfx.Play(this, "whoosh", 0.4f); }
            page.gameObject.SetActive(false);
            page.localScale = pageScale;
            HalloweenKit.Show(pageGlow, false);
            if (sparkles != null) foreach (SpriteRenderer s in sparkles) HalloweenKit.Show(s, false);
            if (pageText != null) pageText.text = "You rock!";
            Display();
        }

        // ---------- Ciclo di vita ----------

        void OnDisable() { CancelInput(); }

        void OnEnable()
        {
            if (!captured || IsFinished || logic == null) return;
            // spento a metà della festa: si rifà il finale (WinWithEgg una volta sola, la logica fa da guardia)
            if (logic.State == LostSheetState.PageOut) { finishing = false; StartCoroutine(Finale()); }
        }

        void OnApplicationPause(bool paused) { if (paused) CancelInput(); }
        void OnApplicationFocus(bool focus) { if (!focus) CancelInput(); }

        /// <summary>Dito perso (pausa, pubblicità, secondo dito): il foglio in mano torna sulla scrivania.</summary>
        void CancelInput()
        {
            if (logic == null || dragger == null) return;
            if (dragger.Held != null) dragger.Release();
            overFeed = false;
            if (logic.Cancel() == LostSheetEvent.SheetReturned) SendSheetHome();
        }

        void SendSheetHome()
        {
            sheetArt.sortingOrder = sheetOrder;
            dragger.SendTo(sheetItem, sheetRest.position, 0.3f, 0.2f);
            sheet.rotation = sheetRest.rotation;
        }

        // ---------- Ogni fotogramma ----------

        void Update()
        {
            toast?.Tick();
            if (logic == null || IsFinished || toast == null) return;
            if (Time.timeScale <= 0f) { CancelInput(); return; }   // pausa o pubblicità: niente input
            if (!finishing && !logic.InputLocked)
            {
#if UNITY_EDITOR
                EditorKeys();
#endif
                ReadPointer();
                React(logic.Shake(Inputs.Shake()));
            }
            React(logic.Tick(Time.deltaTime));
            Animate(Time.deltaTime);
            Display();
        }

        void ReadPointer()
        {
            if (sheetItem != null) sheetItem.Enabled = logic.State == LostSheetState.SheetOnDesk;
            if (!HalloweenKit.ReadPointer(cam, out Vector2 w, out bool began, out bool ended))
            {
                if (dragger.Held != null) CancelInput();   // dito sparito o due dita: il gesto si annulla
                return;
            }
            if (dragger.Held != null)
            {
                dragger.MoveHeld(w);
                overFeed = OverFeed(w);
                if (ended) Drop();
                return;
            }
            if (!began) return;
            if (dragger.TryPick(w) != null)
            {
                LostSheetEvent e = logic.Grab();
                if (e != LostSheetEvent.Grabbed) { dragger.Release(); return; }
                sheetArt.sortingOrder = dragOrder;
                overFeed = OverFeed(w);
                React(e);
                if (ended) Drop();   // tocco e rilascio nello stesso fotogramma
                return;
            }
            if (Hit(printArea, w)) React(logic.Tap(LostSheetTarget.PrintButton));
            else if (Hit(staplerArea, w)) React(logic.Tap(LostSheetTarget.Stapler));
            else if (Hit(binArea, w)) React(logic.Tap(LostSheetTarget.Bin));
            else if (Hit(trayArea, w)) React(logic.Tap(LostSheetTarget.Tray));
            else if (Hit(printerArea, w)) React(logic.Tap(LostSheetTarget.Printer));
        }

        static bool Hit(Collider2D c, Vector2 w) => c != null && c.OverlapPoint(w);

        /// <summary>Il foglio (o il dito che lo tiene) è sopra la stampante.</summary>
        bool OverFeed(Vector2 finger) => feedZone != null && (feedZone.OverlapPoint(finger) || feedZone.OverlapPoint(sheet.position));

        void Drop()
        {
            bool over = overFeed;
            dragger.Release();
            overFeed = false;
            LostSheetEvent e = logic.Release(over);
            if (e == LostSheetEvent.Fed) { feedFrom = sheet.position; feedFromRot = sheet.rotation; }
            React(e);
        }

        void React(LostSheetEvent e)
        {
            if (e == LostSheetEvent.None || e == LostSheetEvent.Ignored) return;
            string line = logic.LineFor(e);
            switch (e)
            {
                case LostSheetEvent.PrintNoPaper:
                case LostSheetEvent.PrintNeedsFeed:
                    buttonT = 0f;
                    blinkT = 0f;
                    Sfx.Play(this, "buzz", 0.5f);
                    Haptics.Light();
                    break;
                case LostSheetEvent.TrayEmpty:
                    trayT = 0f;
                    Sfx.Play(this, "clunk", 0.4f);
                    break;
                case LostSheetEvent.BinNothing:
                    binT = 0f;
                    Sfx.Play(this, "bonk", 0.4f);
                    break;
                case LostSheetEvent.StaplerNothing:
                    staplerT = 0f;
                    Sfx.Play(this, "clunk", 0.6f);
                    Haptics.Tap();
                    break;
                case LostSheetEvent.PrinterBlink:
                    blinkT = 0f;
                    Sfx.Play(this, "tick", 0.5f);
                    break;
                case LostSheetEvent.Rattle:
                    rattleT = 0f;
                    Sfx.Play(this, "clunk", 0.7f);
                    Haptics.Medium();
                    break;
                case LostSheetEvent.SheetLanded:
                    Sfx.Play(this, "pop", 0.4f);
                    toast.Show(line, 2.4f);
                    return;
                case LostSheetEvent.Grabbed:
                    Sfx.Play(this, "tick", 0.4f);
                    return;
                case LostSheetEvent.SheetReturned:
                    SendSheetHome();
                    Sfx.Play(this, "whoosh", 0.3f);
                    return;
                case LostSheetEvent.Fed:
                    sheetArt.sortingOrder = feedOrder;
                    Sfx.Play(this, "whoosh", 0.6f);
                    toast.Show("Ooh. Fresh paper.", 1.2f);
                    return;
                case LostSheetEvent.PrintStarted:
                    sheet.gameObject.SetActive(false);
                    Sfx.Play(this, "ding", 0.5f);
                    return;
                case LostSheetEvent.PageOut:
                    StartCoroutine(Finale());
                    return;
            }
            if (logic.TakeNudge()) toast.Show(LostSheetLogic.NudgeLine, 3f);
            else if (line != null) toast.Show(line, 1.8f);
        }

        // ---------- Animazioni (ricavate dalla logica e da piccoli timer, mai cumulative) ----------

        void PlaceArriving(float k)
        {
            Vector3 from = sheetFrom.position, to = sheetRest.position;
            float e = 1f - (1f - k) * (1f - k);
            float sway = Mathf.Sin(k * Mathf.PI * 2.5f) * 0.3f * (1f - k);
            sheet.position = Vector3.Lerp(from, to, e) + Vector3.up * sway;
            sheet.rotation = sheetRest.rotation * Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI * 3f) * 40f * (1f - k));
        }

        void Animate(float dt)
        {
            LostSheetState s = logic.State;
            float k = logic.Progress;
            if (s == LostSheetState.SheetArriving) PlaceArriving(k);
            else if (s == LostSheetState.Feeding)
            {
                // prima sopra la fessura (dritto), poi giù dentro: il corpo della stampante lo copre (ordine di disegno)
                if (k < 0.35f)
                {
                    float a = k / 0.35f;
                    sheet.position = Vector3.Lerp(feedFrom, feedPoint.position, 1f - (1f - a) * (1f - a));
                    sheet.rotation = Quaternion.Slerp(feedFromRot, Quaternion.identity, a);
                }
                else
                {
                    float b = (k - 0.35f) / 0.65f;
                    sheet.position = Vector3.Lerp(feedPoint.position, slotPoint.position, b * b);
                    sheet.rotation = Quaternion.identity;
                }
            }

            // stampante: sbatacchiata (scossone) o vibrazione di stampa
            rattleT = Mathf.Min(1f, rattleT + dt / 0.45f);
            Vector3 off = Vector3.zero;
            if (rattleT < 1f)
            {
                float a = 0.07f * (1f - rattleT);
                off = new Vector3(Mathf.Sin(Time.time * 70f) * a, Mathf.Abs(Mathf.Sin(Time.time * 45f)) * a, 0f);
            }
            if (s == LostSheetState.Printing)
            {
                off += new Vector3(Mathf.Sin(Time.time * 55f) * 0.02f, Mathf.Sin(Time.time * 38f) * 0.012f, 0f);
                printTickT -= dt;
                if (printTickT <= 0f) { printTickT = 0.18f; Sfx.Play(this, "tick", 0.35f); }
            }
            printerBody.localPosition = bodyHome + off;

            // rimbalzi (dalla scala di riposo, non si sommano mai)
            buttonT = Bump(printButton, buttonScale, buttonT, dt, 0.25f, new Vector3(1.15f, 0.8f, 1f));
            trayT = Bump(tray, trayScale, trayT, dt, 0.3f, new Vector3(1.05f, 1.1f, 1f));
            staplerT = Bump(stapler, staplerScale, staplerT, dt, 0.25f, new Vector3(1.1f, 0.7f, 1f));
            binT = Bump(bin, binScale, binT, dt, 0.3f, new Vector3(1.12f, 0.9f, 1f));
            blinkT = Mathf.Min(1f, blinkT + dt / 0.8f);
        }

        static float Bump(Transform t, Vector3 home, float timer, float dt, float seconds, Vector3 peak)
        {
            if (t == null) return 1f;
            timer = Mathf.Min(1f, timer + dt / seconds);
            float w = Mathf.Sin(timer * Mathf.PI);
            t.localScale = Vector3.Scale(home, Vector3.Lerp(Vector3.one, peak, w));
            return timer;
        }

        /// <summary>Schermino e spia secondo lo stato.</summary>
        void Display()
        {
            LostSheetState s = logic.State;
            string text;
            Color c;
            float alpha = 0.85f;
            bool fast = blinkT < 1f;
            switch (s)
            {
                case LostSheetState.Dragging:
                    text = overFeed ? "FEED ME!" : "INSERT PAPER";
                    c = overFeed ? LampGreen : LampRed;
                    if (!overFeed) alpha = Blink(fast);
                    break;
                case LostSheetState.Feeding: text = "LOADING..."; c = LampAmber; break;
                case LostSheetState.Printing: text = "PRINTING..."; c = LampGreen; alpha = Blink(true); break;
                case LostSheetState.PageOut:
                case LostSheetState.Won: text = "YOU ROCK!"; c = LampGreen; break;
                default:
                    text = Mathf.Repeat(Time.time, 1.6f) < 0.8f ? "PAPER EMPTY" : "INSERT PAPER";
                    c = LampRed;
                    alpha = Blink(fast);
                    break;
            }
            if (lcd != null && text != lcdShown) { lcd.text = text; lcdShown = text; }
            if (lamp != null) lamp.color = new Color(c.r, c.g, c.b, alpha);
        }

        static float Blink(bool fast) => (Mathf.Repeat(Time.time, fast ? 0.2f : 0.8f) < (fast ? 0.1f : 0.4f)) ? 0.9f : 0.15f;

        // ---------- Vittoria ----------

        IEnumerator Finale()
        {
            if (finishing) yield break;
            finishing = true;
            if (dragger.Held != null) dragger.Release();
            sheet.gameObject.SetActive(false);
            printerBody.localPosition = bodyHome;
            // il foglio stampato esce dalla fessura davanti (il bordo alto resta nella fessura)
            page.gameObject.SetActive(true);
            page.rotation = Quaternion.identity;
            Vector3 top = outPoint.position;
            Sfx.Play(this, "whoosh", 0.5f);
            for (float t = 0f; t < 0.55f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.55f);
                page.localScale = new Vector3(pageScale.x, pageScale.y * Mathf.Max(0.04f, k), pageScale.z);
                page.position = top + Vector3.down * (PageHalfH * pageScale.y * k);
                yield return null;
            }
            page.localScale = pageScale;
            page.position = top + Vector3.down * (PageHalfH * pageScale.y);
            yield return new WaitForSeconds(0.2f);
            // ...e vola in grande al centro: uovo d'oro e "You rock!"
            Sfx.Play(this, "ding", 0.8f);
            Haptics.Success();
            Vector3 from = page.position;
            HalloweenKit.Show(pageGlow, true);
            if (sparkles != null) foreach (SpriteRenderer sp in sparkles) HalloweenKit.Show(sp, true);
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / 0.6f), e = 1f - (1f - k) * (1f - k);
                page.position = Vector3.Lerp(from, pageShow.position, e) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.35f;
                page.localScale = pageScale * Mathf.Lerp(1f, pageShowScale, e);
                page.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI * 2f) * 8f * (1f - k));
                PageShine(k);
                yield return null;
            }
            page.position = pageShow.position;
            page.localScale = pageScale * pageShowScale;
            page.rotation = Quaternion.identity;
            if (reward != null) StartCoroutine(Bob());
            if (reward != null) yield return reward.Play(Compliment, 1.8f);
            else yield return new WaitForSeconds(1.5f);
            if (!IsFinished && logic.TryComplete()) WinWithEgg(LostSheetLogic.EggId);
        }

        /// <summary>Bagliore dorato e scintille intorno al foglio mostrato.</summary>
        void PageShine(float k)
        {
            if (pageGlow != null)
            {
                pageGlow.transform.localScale = Vector3.one * (2.2f + 0.25f * Mathf.Sin(Time.time * 6f));
                HalloweenKit.SetAlpha(pageGlow, 0.6f * Mathf.Clamp01(k * 2f));
            }
            if (sparkles == null) return;
            for (int i = 0; i < sparkles.Length; i++)
            {
                if (sparkles[i] == null) continue;
                float a = Time.time * 1.5f + i * Mathf.PI * 2f / sparkles.Length;
                sparkles[i].transform.localPosition = new Vector3(Mathf.Cos(a) * 0.62f, Mathf.Sin(a) * 0.75f, 0f);
                sparkles[i].transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 120f);
                HalloweenKit.SetAlpha(sparkles[i], Mathf.Clamp01(k * 2f) * (0.6f + 0.4f * Mathf.Sin(Time.time * 9f + i)));
            }
        }

        IEnumerator Bob()
        {
            Vector3 home = pageShow.position;
            while (finishing && !IsFinished)
            {
                page.position = home + Vector3.up * Mathf.Sin(Time.time * 3f) * 0.05f;
                PageShine(1f);
                yield return null;
            }
        }

#if UNITY_EDITOR
        // ---------- Tasti di prova nell'Editor ----------

        void EditorKeys()
        {
            if (Input.GetKeyDown(KeyCode.C)) { PlayerPrefs.SetInt(LostSheetLogic.CarriedKey, 1); PlayerPrefs.Save(); OnLevelStart(); return; }
            if (Input.GetKeyDown(KeyCode.X)) { PlayerPrefs.DeleteKey(LostSheetLogic.CarriedKey); PlayerPrefs.Save(); OnLevelStart(); return; }
            if (Input.GetKeyDown(KeyCode.R)) { OnLevelStart(); return; }
            if (Input.GetKeyDown(KeyCode.Alpha1)) React(logic.Tap(LostSheetTarget.PrintButton));
            if (Input.GetKeyDown(KeyCode.Alpha2)) React(logic.Tap(LostSheetTarget.Tray));
            if (Input.GetKeyDown(KeyCode.Alpha3)) React(logic.Tap(LostSheetTarget.Bin));
            if (Input.GetKeyDown(KeyCode.Alpha4)) React(logic.Tap(LostSheetTarget.Stapler));
            if (Input.GetKeyDown(KeyCode.Alpha5)) React(logic.Tap(LostSheetTarget.Printer));
            if (Input.GetKeyDown(KeyCode.O)) CancelInput();
            if (dragger.Held == null && logic.State == LostSheetState.SheetOnDesk)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    logic.Grab();
                    sheet.position = feedPoint.position + Vector3.up * 0.3f;
                    overFeed = true;
                    sheetArt.sortingOrder = dragOrder;
                    LostSheetEvent e = logic.Release(true);
                    feedFrom = sheet.position;
                    feedFromRot = sheet.rotation;
                    React(e);
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    logic.Grab();
                    sheet.position = new Vector3(-1.6f, -3.2f, sheet.position.z);
                    React(logic.Release(false));
                }
            }
        }
#endif
    }
}

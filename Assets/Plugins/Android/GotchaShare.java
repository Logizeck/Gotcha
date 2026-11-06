package com.gotcha.game;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Build;
import android.os.SystemClock;

import com.unity3d.player.UnityPlayer;

/**
 * Menu di condivisione "con ricevuta": come Intent.createChooser, ma il sistema ci avvisa di QUALE app il giocatore
 * ha toccato (IntentSender del chooser, Android 5.1+). Se il menu viene chiuso senza scegliere, non arriva niente.
 * Android non dice mai se il messaggio è stato poi inviato davvero: il C# (VerifiedShare) unisce questa ricevuta al
 * tempo passato fuori dal gioco dopo la scelta (almeno 4 s) per decidere se dare il premio.
 *
 * Il receiver è dinamico, registrato sul contesto dell'applicazione e NON esportato (Android 13+:
 * RECEIVER_NOT_EXPORTED): il broadcast lo manda il PendingIntent creato da noi, quindi arriva con la nostra
 * identità e nessun'altra app può fingere una scelta. Il PendingIntent è MUTABLE (dal 31 va dichiarato; il chooser
 * deve poterci aggiungere EXTRA_CHOSEN_COMPONENT) e il suo Intent è esplicito sul nostro pacchetto (dal 34 un
 * PendingIntent mutabile con Intent implicito è vietato). Niente modifiche al manifest.
 *
 * Uso dal C#: begin() → shareText(...) (o GotchaVideo.share, che passa da chooser()) → al ritorno nel gioco
 * pickedAt() / pickedComponent() e now(), tutti nello stesso orologio monotòno (elapsedRealtime).
 */
public class GotchaShare {
    static final String ACTION = "com.gotcha.game.SHARE_PICKED";

    static volatile long pickedAt = -1;
    static volatile String picked = "";
    static volatile int requestCode;
    static BroadcastReceiver receiver;

    /** Orologio monotòno (ms dall'accensione): lo stesso di pickedAt. */
    public static long now() { return SystemClock.elapsedRealtime(); }

    /** Momento della scelta di un'app nel menu (dopo l'ultimo begin), -1 se nessuna. */
    public static long pickedAt() { return pickedAt; }

    /** Pacchetto/classe dell'app scelta ("" se nessuna). */
    public static String pickedComponent() { return picked; }

    /** Azzera la ricevuta: da chiamare subito prima di aprire il menu. */
    public static void begin() {
        pickedAt = -1;
        picked = "";
        Activity activity = UnityPlayer.currentActivity;
        if (activity != null) ensureReceiver(activity.getApplicationContext());
    }

    /** Condivide un testo con il menu "con ricevuta". */
    public static boolean shareText(final String text, final String chooserTitle) {
        try {
            final Activity activity = UnityPlayer.currentActivity;
            if (activity == null) return false;
            activity.runOnUiThread(new Runnable() {
                @Override public void run() {
                    try {
                        Intent send = new Intent(Intent.ACTION_SEND);
                        send.setType("text/plain");
                        send.putExtra(Intent.EXTRA_TEXT, text);
                        activity.startActivity(chooser(activity, send, chooserTitle));
                    } catch (Exception ignored) { }
                }
            });
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    /** Il chooser con la ricevuta della scelta (se qualcosa va storto, il chooser normale: niente ricevuta, niente premio). */
    public static Intent chooser(Context context, Intent send, CharSequence title) {
        try {
            Context app = context.getApplicationContext();
            ensureReceiver(app);
            Intent callback = new Intent(ACTION);
            callback.setPackage(app.getPackageName());   // esplicito: solo la nostra app lo riceve
            int flags = PendingIntent.FLAG_UPDATE_CURRENT;
            if (Build.VERSION.SDK_INT >= 31) flags |= PendingIntent.FLAG_MUTABLE;
            PendingIntent pi = PendingIntent.getBroadcast(app, ++requestCode, callback, flags);
            return Intent.createChooser(send, title, pi.getIntentSender());
        } catch (Exception e) {
            return Intent.createChooser(send, title);
        }
    }

    @SuppressWarnings("deprecation")
    static ComponentName legacyChosen(Intent intent) { return intent.getParcelableExtra(Intent.EXTRA_CHOSEN_COMPONENT); }

    static synchronized void ensureReceiver(Context app) {
        if (receiver != null || app == null) return;
        BroadcastReceiver r = new BroadcastReceiver() {
            @Override public void onReceive(Context c, Intent intent) {
                ComponentName chosen = null;
                try {
                    // la versione tipizzata esiste dal 33 ma è affidabile dal 34 (bug noto sul 33)
                    if (Build.VERSION.SDK_INT >= 34) chosen = intent.getParcelableExtra(Intent.EXTRA_CHOSEN_COMPONENT, ComponentName.class);
                    else chosen = legacyChosen(intent);
                } catch (Exception ignored) { }
                picked = chosen != null ? chosen.flattenToShortString() : "unknown";
                pickedAt = SystemClock.elapsedRealtime();
            }
        };
        IntentFilter filter = new IntentFilter(ACTION);
        try {
            if (Build.VERSION.SDK_INT >= 33) app.registerReceiver(r, filter, Context.RECEIVER_NOT_EXPORTED);
            else app.registerReceiver(r, filter);
            receiver = r;
        } catch (Exception ignored) { }
    }
}

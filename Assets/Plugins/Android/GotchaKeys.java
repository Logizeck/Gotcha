package com.gotcha.game;

import android.app.Activity;
import android.view.ActionMode;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.SearchEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.view.accessibility.AccessibilityEvent;

import com.unity3d.player.UnityPlayer;

/**
 * Conta le pressioni dei tasti volume senza cambiare il comportamento del telefono.
 * Avvolge il Window.Callback dell'activity di Unity: ogni evento passa all'originale, noi contiamo soltanto.
 * Serve perché, con il volume già al minimo, premere "volume -" non cambia il valore letto da AudioManager.
 */
public class GotchaKeys implements Window.Callback {
    static volatile int volumeDown, volumeUp;
    static volatile boolean installed;

    final Window.Callback inner;

    GotchaKeys(Window.Callback inner) { this.inner = inner; }

    public static void install() {
        final Activity activity = UnityPlayer.currentActivity;
        if (activity == null || installed) return;
        activity.runOnUiThread(new Runnable() {
            @Override public void run() {
                if (installed) return;
                Window w = activity.getWindow();
                Window.Callback cb = w.getCallback();
                if (cb == null || cb instanceof GotchaKeys) { installed = cb != null; return; }
                w.setCallback(new GotchaKeys(cb));
                installed = true;
            }
        });
    }

    public static int volumeDownCount() { return volumeDown; }
    public static int volumeUpCount() { return volumeUp; }
    public static boolean isInstalled() { return installed; }

    @Override public boolean dispatchKeyEvent(KeyEvent e) {
        if (e.getAction() == KeyEvent.ACTION_DOWN && e.getRepeatCount() == 0) {
            if (e.getKeyCode() == KeyEvent.KEYCODE_VOLUME_DOWN) volumeDown++;
            else if (e.getKeyCode() == KeyEvent.KEYCODE_VOLUME_UP) volumeUp++;
        }
        return inner.dispatchKeyEvent(e);
    }

    @Override public boolean dispatchKeyShortcutEvent(KeyEvent e) { return inner.dispatchKeyShortcutEvent(e); }
    @Override public boolean dispatchTouchEvent(MotionEvent e) { return inner.dispatchTouchEvent(e); }
    @Override public boolean dispatchTrackballEvent(MotionEvent e) { return inner.dispatchTrackballEvent(e); }
    @Override public boolean dispatchGenericMotionEvent(MotionEvent e) { return inner.dispatchGenericMotionEvent(e); }
    @Override public boolean dispatchPopulateAccessibilityEvent(AccessibilityEvent e) { return inner.dispatchPopulateAccessibilityEvent(e); }
    @Override public View onCreatePanelView(int featureId) { return inner.onCreatePanelView(featureId); }
    @Override public boolean onCreatePanelMenu(int featureId, Menu menu) { return inner.onCreatePanelMenu(featureId, menu); }
    @Override public boolean onPreparePanel(int featureId, View view, Menu menu) { return inner.onPreparePanel(featureId, view, menu); }
    @Override public boolean onMenuOpened(int featureId, Menu menu) { return inner.onMenuOpened(featureId, menu); }
    @Override public boolean onMenuItemSelected(int featureId, MenuItem item) { return inner.onMenuItemSelected(featureId, item); }
    @Override public void onWindowAttributesChanged(WindowManager.LayoutParams attrs) { inner.onWindowAttributesChanged(attrs); }
    @Override public void onContentChanged() { inner.onContentChanged(); }
    @Override public void onWindowFocusChanged(boolean hasFocus) { inner.onWindowFocusChanged(hasFocus); }
    @Override public void onAttachedToWindow() { inner.onAttachedToWindow(); }
    @Override public void onDetachedFromWindow() { inner.onDetachedFromWindow(); }
    @Override public void onPanelClosed(int featureId, Menu menu) { inner.onPanelClosed(featureId, menu); }
    @Override public boolean onSearchRequested() { return inner.onSearchRequested(); }
    @Override public boolean onSearchRequested(SearchEvent e) { return inner.onSearchRequested(e); }
    @Override public ActionMode onWindowStartingActionMode(ActionMode.Callback callback) { return inner.onWindowStartingActionMode(callback); }
    @Override public ActionMode onWindowStartingActionMode(ActionMode.Callback callback, int type) { return inner.onWindowStartingActionMode(callback, type); }
    @Override public void onActionModeStarted(ActionMode mode) { inner.onActionModeStarted(mode); }
    @Override public void onActionModeFinished(ActionMode mode) { inner.onActionModeFinished(mode); }
}

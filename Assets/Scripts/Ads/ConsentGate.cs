namespace Gotcha.Ads
{
    /// <summary>
    /// Il modulo dei cookie (UMP) è ancora in corso: finché è vero, le richieste di Android (microfono, notifiche)
    /// aspettano, così all'avvio non si sovrappongono e il modulo del consenso compare per primo.
    /// Vale sempre false senza AdMob (Editor, build senza plugin).
    /// </summary>
    public static class ConsentGate
    {
        public static bool Pending;
    }
}

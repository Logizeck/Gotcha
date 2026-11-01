using System.Collections;
using Gotcha.Ads;
using Gotcha.Sensors;
using UnityEngine;

namespace Gotcha.Core
{
    /// <summary>
    /// All'avvio, una richiesta alla volta e sempre nello stesso ordine:
    /// 1) consenso ai cookie/pubblicità (modulo di Google, solo in UE e quando serve);
    /// 2) microfono (alcuni livelli si giocano con la voce o soffiando);
    /// 3) notifiche (promemoria "I'm missing you!", Android 13 e successivi), solo la prima volta.
    /// Prima della 0.76 il microfono veniva chiesto subito e la finestra di Android copriva il modulo dei cookie,
    /// che così non compariva; le notifiche si chiedevano solo dopo il primo livello superato.
    /// </summary>
    public class FirstLaunchPermissions : MonoBehaviour
    {
        const float ConsentWaitMax = 30f;   // se Google non risponde, dopo 30 s si va avanti lo stesso

        IEnumerator Start()
        {
            yield return null;
            float waited = 0f;
            while (ConsentGate.Pending && waited < ConsentWaitMax)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            yield return new WaitForSecondsRealtime(0.5f);
            if (SensorHub.Instance != null) yield return SensorHub.Instance.RequestMicPermissionAndWait();
            yield return new WaitForSecondsRealtime(0.3f);
            if (ComebackNotifier.Instance != null) ComebackNotifier.Instance.AskPermissionOnce();
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Háttér polling coroutine: minden <see cref="syncInterval"/> másodpercben letölti
/// a felhő fájlt, és tájékoztatja a feliratkozókat az <see cref="OnSyncComplete"/> eseményen.
///
/// Sikeres szinkronizáció után az ES3 helyi fájl naprakész –
/// használjon ES3.Load-ot az értékek kiolvasásához.
///
/// Beállítás: csatolja egy GameObject-hez, rendelje hozzá az AuthConfig asset-et.
/// </summary>
public class CloudSyncManager : MonoBehaviour
{
    [SerializeField] private AuthConfig authConfig;

    [Tooltip("Szinkronizáció gyakorisága másodpercben.")]
    [SerializeField, Min(1f)] private float syncInterval = 5f;

    // ── Nyilvános esemény ─────────────────────────────────────────────────────

    /// <summary>
    /// Minden szinkronizáció végén meghívódik.
    /// Paraméter: true = sikeres letöltés, false = hálózati / szerver hiba.
    /// </summary>
    public event Action<bool> OnSyncComplete;

    // ── Belső állapot ─────────────────────────────────────────────────────────

    private Coroutine pollCoroutine;
    private bool isSyncing;

    // ── Életciklus ────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        pollCoroutine = StartCoroutine(PollLoop());
    }

    private void OnDisable()
    {
        if (pollCoroutine != null)
        {
            StopCoroutine(pollCoroutine);
            pollCoroutine = null;
        }
        isSyncing = false;
    }

    // ── Polling coroutine ─────────────────────────────────────────────────────

    /// <summary>
    /// Azonnal végrehajt egy szinkronizációt, majd syncInterval másodpercenként ismétli.
    /// </summary>
    private IEnumerator PollLoop()
    {
        while (true)
        {
            yield return StartCoroutine(SyncOnce());
            yield return new WaitForSeconds(syncInterval);
        }
    }

    /// <summary>Egy letöltési ciklus: felhő → helyi ES3 fájl.</summary>
    private IEnumerator SyncOnce()
    {
        isSyncing = true;

        ES3CloudSettings settings = authConfig.CreateSettings();
        yield return ES3Cloud.DownloadFile(settings, authConfig.defaultFilename);

        bool success = !ES3Cloud.isError;
        if (!success)
            Debug.LogWarning($"[CloudSync] Szinkronizáció sikertelen: {ES3Cloud.error}");
        else
            Debug.Log($"[CloudSync] Szinkronizáció sikeres – {DateTime.Now:HH:mm:ss}");

        isSyncing = false;
        OnSyncComplete?.Invoke(success);
    }

    // ── Nyilvános API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Azonnali, ütemezésen kívüli szinkronizációt indít.
    /// Ha már fut egy sync, a hívás figyelmen kívül marad.
    /// </summary>
    public void TriggerSync()
    {
        if (!isSyncing)
            StartCoroutine(SyncOnce());
    }

    /// <summary>Az aktuálisan konfigurált szinkronizációs intervallum másodpercben.</summary>
    public float SyncInterval => syncInterval;

    /// <summary>True, ha éppen folyamatban van egy szinkronizáció.</summary>
    public bool IsSyncing => isSyncing;
}

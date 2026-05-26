using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Generikus Save/Load coroutine-ok ES3Cloud fölé.
/// Támogatott típusok: bool, int, float, string, és ezek tömbváltozatai,
/// illetve bármilyen más ES3 által szerializálható típus.
///
/// Használat (más MonoBehaviour-ból):
///   yield return StartCoroutine(cloudHelper.SaveToCloud<int>("score", 42));
///   yield return StartCoroutine(cloudHelper.LoadFromCloud<int>("score", (v, ok) => Debug.Log(v)));
/// </summary>
public class GenericCloudHelper : MonoBehaviour
{
    [SerializeField] private AuthConfig authConfig;

    /// <summary>Közvetlen hozzáférés a konfigurációhoz (pl. CloudSyncManager számára).</summary>
    public AuthConfig Config => authConfig;

    // ── SAVE ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Elmenti a <paramref name="value"/> értéket helyben (ES3.Save),
    /// majd feltölti a megadott fájlt a felhőbe.
    /// </summary>
    /// <param name="onComplete">Callback: true = sikeres, false = hiba.</param>
    public IEnumerator SaveToCloud<T>(string key, T value, string filename, Action<bool> onComplete = null)
    {
        ES3.Save<T>(key, value, filename);

        ES3CloudSettings settings = authConfig.CreateSettings();
        yield return ES3Cloud.UploadFile(settings, filename);

        bool success = !ES3Cloud.isError;
        if (!success)
            Debug.LogError($"[CloudHelper] Feltöltési hiba – kulcs='{key}', fájl='{filename}': {ES3Cloud.error}");

        onComplete?.Invoke(success);
    }

    /// <summary>A <see cref="AuthConfig.defaultFilename"/>-t használja fájlnévként.</summary>
    public IEnumerator SaveToCloud<T>(string key, T value, Action<bool> onComplete = null)
    {
        yield return SaveToCloud<T>(key, value, authConfig.defaultFilename, onComplete);
    }

    // ── LOAD ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Letölti a fájlt a felhőből, majd visszaadja a kulcshoz tartozó értéket.
    /// </summary>
    /// <param name="onComplete">Callback: (érték, true) siker esetén; (default, false) hiba esetén.</param>
    public IEnumerator LoadFromCloud<T>(string key, string filename, Action<T, bool> onComplete)
    {
        ES3CloudSettings settings = authConfig.CreateSettings();
        yield return ES3Cloud.DownloadFile(settings, filename);

        if (ES3Cloud.isError)
        {
            Debug.LogError($"[CloudHelper] Letöltési hiba – kulcs='{key}', fájl='{filename}': {ES3Cloud.error}");
            onComplete?.Invoke(default(T), false);
            yield break;
        }

        if (!ES3.KeyExists(key, filename))
        {
            Debug.LogWarning($"[CloudHelper] Kulcs '{key}' nem található a '{filename}' fájlban.");
            onComplete?.Invoke(default(T), false);
            yield break;
        }

        onComplete?.Invoke(ES3.Load<T>(key, filename), true);
    }

    /// <summary>A <see cref="AuthConfig.defaultFilename"/>-t használja fájlnévként.</summary>
    public IEnumerator LoadFromCloud<T>(string key, Action<T, bool> onComplete)
    {
        yield return LoadFromCloud<T>(key, authConfig.defaultFilename, onComplete);
    }

    // ── Fire-and-forget kényelmi metódusok ───────────────────────────────────

    /// <summary>Elindítja a mentést coroutine-ként, nem kell yield return.</summary>
    public Coroutine SaveAsync<T>(string key, T value, string filename, Action<bool> onComplete = null)
        => StartCoroutine(SaveToCloud<T>(key, value, filename, onComplete));

    /// <inheritdoc cref="SaveAsync{T}(string,T,string,Action{bool})"/>
    public Coroutine SaveAsync<T>(string key, T value, Action<bool> onComplete = null)
        => StartCoroutine(SaveToCloud<T>(key, value, onComplete));

    /// <summary>Elindítja a betöltést coroutine-ként, nem kell yield return.</summary>
    public Coroutine LoadAsync<T>(string key, string filename, Action<T, bool> onComplete)
        => StartCoroutine(LoadFromCloud<T>(key, filename, onComplete));

    /// <inheritdoc cref="LoadAsync{T}(string,string,Action{T,bool})"/>
    public Coroutine LoadAsync<T>(string key, Action<T, bool> onComplete)
        => StartCoroutine(LoadFromCloud<T>(key, onComplete));
}

using UnityEngine;

/// <summary>
/// ScriptableObject – hitelesítési adatok és alap beállítások az ES3Cloud kapcsolathoz.
/// Létrehozás: Assets > Create > Cloud > Auth Config
/// </summary>
[CreateAssetMenu(fileName = "AuthConfig", menuName = "Cloud/Auth Config")]
public class AuthConfig : ScriptableObject
{
    [Tooltip("URL az ES3.php fájlhoz, pl. http://localhost/ES3/ES3.php")]
    public string serverUrl = "http://localhost/ES3/ES3.php";

    [Tooltip("Felhasználónév (hagyja üresen, ha nincs hitelesítés)")]
    public string username = "";

    [Tooltip("Jelszó a fenti felhasználóhoz")]
    public string password = "";

    [Tooltip("Az ES3 adatfájl neve a szerveren")]
    public string defaultFilename = "cloud_data.es3";

    /// <summary>
    /// ES3CloudSettings példányt hoz létre az aktuális konfiguráció alapján.
    /// Ha az Ön ES3 verziójában a konstruktor vagy a property nevek eltérnek,
    /// csak itt kell módosítani.
    /// </summary>
    public ES3CloudSettings CreateSettings()
    {
        var settings = new ES3CloudSettings(serverUrl)
        {
            username = username,
            password = password
        };
        return settings;
    }
}

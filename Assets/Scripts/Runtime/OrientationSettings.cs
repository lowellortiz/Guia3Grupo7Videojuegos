using UnityEngine;

/// <summary>
/// Preferencia de orientacion elegida por el usuario en el menu. Se guarda en PlayerPrefs
/// y se aplica en todas las escenas.
///
/// Ojo: esto solo puede RESTRINGIR lo que el AndroidManifest ya permite. El manifest se
/// genera al construir el APK a partir de Player Settings, asi que este proyecto debe
/// construirse con Auto Rotation y las dos orientaciones horizontales habilitadas.
/// </summary>
public static class OrientationSettings
{
    public const string Key = "Guia3.Orientacion";

    public enum Modo
    {
        Auto = 0,
        Horizontal = 1,
        Vertical = 2
    }

    public static Modo Actual
    {
        get => (Modo)PlayerPrefs.GetInt(Key, (int)Modo.Auto);
        set
        {
            PlayerPrefs.SetInt(Key, (int)value);
            PlayerPrefs.Save();
            Aplicar();
        }
    }

    public static string Etiqueta(Modo modo)
    {
        switch (modo)
        {
            case Modo.Horizontal: return "PANTALLA: HORIZONTAL";
            case Modo.Vertical: return "PANTALLA: VERTICAL";
            default: return "PANTALLA: AUTOMATICA";
        }
    }

    /// <summary>Pasa al siguiente modo del ciclo Auto -> Horizontal -> Vertical.</summary>
    public static Modo Siguiente()
    {
        Actual = (Modo)(((int)Actual + 1) % 3);
        return Actual;
    }

    /// <summary>
    /// Se ejecuta sola al arrancar la app, antes de cargar la primera escena, para que la
    /// preferencia valga tanto en el menu como en el juego sin cablear nada en las escenas.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Aplicar()
    {
        Modo modo = Actual;

        // Boca abajo nunca: en esa posicion el altavoz y la camara frontal quedan abajo.
        Screen.autorotateToPortraitUpsideDown = false;

        switch (modo)
        {
            case Modo.Horizontal:
                Screen.autorotateToPortrait = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                break;

            case Modo.Vertical:
                Screen.autorotateToPortrait = true;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
                break;

            default:
                Screen.autorotateToPortrait = true;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                break;
        }

        // Se asigna DESPUES de las banderas: AutoRotation reevalua cuales quedaron activas.
        // Con solo las dos horizontales encendidas, el telefono queda bloqueado en horizontal
        // pero sigue pudiendo voltearse de un lado al otro, como en Clash of Clans.
        Screen.orientation = ScreenOrientation.AutoRotation;

        Debug.Log("[Guia3] Orientacion = " + modo);
    }
}

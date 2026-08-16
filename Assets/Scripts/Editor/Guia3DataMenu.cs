using System.IO;
using UnityEditor;
using UnityEngine;

namespace Guia3.EditorTools
{
    /// <summary>
    /// Utilidades para manejar la tabla de datos de las pruebas con usuarios.
    /// El CSV vive en Application.persistentDataPath, que es la misma ruta en el editor
    /// y en el celular (ahi hay que sacarlo con adb pull).
    /// </summary>
    public static class Guia3DataMenu
    {
        private const string CsvFileName = "guia3_datos.csv";

        private static string CsvPath => Path.Combine(Application.persistentDataPath, CsvFileName);

        [MenuItem("Guia3/Datos/Abrir carpeta de resultados", priority = 20)]
        public static void OpenFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        [MenuItem("Guia3/Datos/Ver resumen en consola", priority = 21)]
        public static void PrintSummary()
        {
            if (!File.Exists(CsvPath))
            {
                Debug.Log($"[Guia3] Todavia no hay datos en {CsvPath}");
                return;
            }

            string[] lines = File.ReadAllLines(CsvPath);
            Debug.Log($"[Guia3] {Mathf.Max(0, lines.Length - 1)} intentos registrados en {CsvPath}\n" +
                      string.Join("\n", lines));
        }

        [MenuItem("Guia3/Datos/Borrar CSV antes de las pruebas", priority = 22)]
        public static void ClearCsv()
        {
            if (!File.Exists(CsvPath))
            {
                Debug.Log("[Guia3] No hay CSV que borrar.");
                return;
            }

            if (!EditorUtility.DisplayDialog("Guia 3",
                    $"Se borrara la tabla de datos:\n{CsvPath}\n\nEsta accion no se puede deshacer.",
                    "Borrar", "Cancelar"))
                return;

            File.Delete(CsvPath);
            PlayerPrefs.DeleteKey("Guia3.Attempt");
            PlayerPrefs.Save();
            Debug.Log("[Guia3] CSV borrado y contador de intentos reiniciado.");
        }
    }
}

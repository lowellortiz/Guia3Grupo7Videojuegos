using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Guia3.EditorTools
{
    /// <summary>
    /// Genera un icono provisional (los cuatro cubos apilados) y lo asigna a Android e iOS.
    /// Es un marcador de posicion: si el equipo hace un icono propio, basta con reemplazar
    /// el PNG y volver a ejecutar el menu.
    /// </summary>
    public static class Guia3IconGenerator
    {
        private const string IconPath = "Assets/Guia3/Icon/Guia3_Icon.png";
        private const int Size = 1024;

        private static readonly Color Background = new Color(0.09f, 0.12f, 0.19f);
        private static readonly Color Platform = new Color(0.24f, 0.68f, 0.42f);
        private static readonly Color[] CubeColors =
        {
            new Color(0.26f, 0.50f, 0.86f), // abajo
            new Color(0.34f, 0.72f, 0.36f),
            new Color(0.93f, 0.76f, 0.24f),
            new Color(0.85f, 0.26f, 0.26f)  // arriba
        };

        [MenuItem("Guia3/Generar icono de la app", priority = 10)]
        public static void Generate()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(IconPath));
            File.WriteAllBytes(IconPath, Paint().EncodeToPNG());

            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);
            ConfigureImporter();

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon == null)
            {
                Debug.LogError("[Guia3] No se pudo cargar el icono generado.");
                return;
            }

            Texture2D[] icons = { icon };
            PlayerSettings.SetIcons(NamedBuildTarget.Android, icons, IconKind.Application);
            PlayerSettings.SetIcons(NamedBuildTarget.iOS, icons, IconKind.Application);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, icons, IconKind.Any);

            AssetDatabase.SaveAssets();
            Debug.Log($"[Guia3] Icono generado y asignado: {IconPath}");
        }

        private static Texture2D Paint()
        {
            Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[Size * Size];

            for (int i = 0; i < pixels.Length; i++) pixels[i] = Background;

            // Ojo: en Texture2D el eje Y crece hacia ARRIBA, asi que la pila se dibuja
            // sumando altura por nivel, no restandola.

            // Plataforma del punto B, en la base.
            Fill(pixels, 200, 824, 80, 130, Platform);

            const int cubeSize = 200;
            const int gap = 6;
            const int baseY = 130;
            const int left = (Size - cubeSize) / 2;

            for (int level = 0; level < CubeColors.Length; level++)
            {
                int bottom = baseY + level * (cubeSize + gap);
                int top = bottom + cubeSize;
                Fill(pixels, left, left + cubeSize, bottom, top, CubeColors[level]);
                // Franja superior mas clara: se lee como volumen y no como cuadrado plano.
                Fill(pixels, left, left + cubeSize, top - 34, top, CubeColors[level] * 1.3f);
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static void Fill(Color[] pixels, int xMin, int xMax, int yMin, int yMax, Color color)
        {
            xMin = Mathf.Clamp(xMin, 0, Size);
            xMax = Mathf.Clamp(xMax, 0, Size);
            yMin = Mathf.Clamp(yMin, 0, Size);
            yMax = Mathf.Clamp(yMax, 0, Size);

            for (int y = yMin; y < yMax; y++)
            {
                int row = y * Size;
                for (int x = xMin; x < xMax; x++)
                {
                    Color c = color;
                    c.a = 1f;
                    pixels[row + x] = c;
                }
            }
        }

        private static void ConfigureImporter()
        {
            TextureImporter importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Default;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}

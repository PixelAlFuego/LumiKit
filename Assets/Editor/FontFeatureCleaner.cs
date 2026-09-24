using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

namespace LumiKit.Editor
{
    /// <summary>
    /// Limpiador de las tablas de features de las fuentes del pack (LK-22b).
    /// Regla: un registro se conserva sólo si todos sus glifos están en el atlas.
    /// </summary>
    /// <remarks>
    /// El Font Asset Creator guarda features de glifos que el atlas no tiene. TMP no puede usarlas
    /// porque esos caracteres no se pintan con esta fuente: sólo pesan (Inter-Regular, 22,8 MB).
    ///
    /// Paso obligatorio tras cada regeneración con el Font Asset Creator y tras
    /// "Import Font Features" del menú contextual del .asset: los dos las vuelven a meter todas.
    /// Repetible: una segunda ejecución no quita nada.
    ///
    /// Sólo API pública de TMP (com.unity.ugui 2.0.0), sin reflection.
    /// </remarks>
    public static class FontFeatureCleaner
    {
        private const string LOG = "[LumiKit] ";
        private const string FONTS_FOLDER = "Assets/LumiKit/Fonts";

        // MB decimales, los mismos del informe de la Sesión 10.
        private const double BYTES_PER_MB = 1000000.0;
        private const long SIZE_LIMIT_BYTES = 5000000;

        private static readonly string[] TABLE_NAMES =
        {
            "Kerning", "MarkToBase", "MarkToMark", "Ligaduras", "Sustitución múltiple"
        };

        // ── Menú ───────────────────────────────────────────────────────────────────────

        [MenuItem("LumiKit/Fuentes/Limpiar features fuera del atlas (LK-22b)", false, 200)]
        public static void CleanFontFeatures()
        {
            List<TMP_FontAsset> targets = FindTargets();
            if (targets.Count == 0)
            {
                Debug.LogError($"{LOG}No hay ninguna fuente Static que limpiar en '{FONTS_FOLDER}'.");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Limpiar features (LK-22b)",
                $"Se van a modificar {targets.Count} fuentes de '{FONTS_FOLDER}'.\n\n" +
                "Sin commit ni copia fuera de Assets/, no hay vuelta atrás.",
                "Limpiar", "Cancelar");
            if (!confirmed)
            {
                Debug.Log($"{LOG}Limpieza cancelada. No se ha tocado nada.");
                return;
            }

            long totalBefore = 0;
            long totalAfter = 0;
            int removedTotal = 0;
            int overLimit = 0;

            foreach (TMP_FontAsset fontAsset in targets)
            {
                removedTotal += CleanFontAsset(fontAsset, out long sizeBefore, out long sizeAfter);
                totalBefore += sizeBefore;
                totalAfter += sizeAfter;
                if (sizeAfter > SIZE_LIMIT_BYTES)
                {
                    overLimit++;
                }
            }

            Debug.Log($"{LOG}Limpieza terminada: {targets.Count} fuentes, {removedTotal} registros quitados. " +
                      $"Disco: {ToMegabytes(totalBefore)} MB → {ToMegabytes(totalAfter)} MB. " +
                      $"Por encima de 5 MB: {overLimit}.");
        }

        // ── Búsqueda ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fuentes de <see cref="FONTS_FOLDER"/> que se pueden limpiar. Salta, con aviso, las que no
        /// son Static y las que no tienen glifos: se quedarían sin features que sí necesitan.
        /// </summary>
        private static List<TMP_FontAsset> FindTargets()
        {
            List<TMP_FontAsset> targets = new List<TMP_FontAsset>();

            foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { FONTS_FOLDER }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (fontAsset == null)
                {
                    continue;
                }

                if (fontAsset.atlasPopulationMode != AtlasPopulationMode.Static)
                {
                    Debug.LogWarning($"{LOG}Se salta '{path}': no es Static. En Dynamic el atlas crece en runtime y necesita sus features.");
                    continue;
                }

                if (fontAsset.glyphTable == null || fontAsset.glyphTable.Count == 0)
                {
                    Debug.LogWarning($"{LOG}Se salta '{path}': su tabla de glifos está vacía.");
                    continue;
                }

                if (fontAsset.fontFeatureTable == null)
                {
                    Debug.LogWarning($"{LOG}Se salta '{path}': no tiene tabla de features.");
                    continue;
                }

                targets.Add(fontAsset);
            }

            return targets;
        }

        // ── Limpieza ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Filtra las cinco tablas de <see cref="TMP_FontFeatureTable"/>, guarda el asset e informa
        /// por tabla y de su peso en disco. Devuelve cuántos registros ha quitado.
        /// </summary>
        private static int CleanFontAsset(TMP_FontAsset fontAsset, out long sizeBefore, out long sizeAfter)
        {
            string path = AssetDatabase.GetAssetPath(fontAsset);
            sizeBefore = new FileInfo(path).Length;

            // "En el atlas" = en la tabla de glifos del asset. Incluye glifos sin área, como el espacio.
            HashSet<uint> atlas = new HashSet<uint>();
            foreach (Glyph glyph in fontAsset.glyphTable)
            {
                atlas.Add(glyph.index);
            }

            TMP_FontFeatureTable table = fontAsset.fontFeatureTable;
            int[] before = CountRecords(table);

            table.glyphPairAdjustmentRecords?.RemoveAll(r =>
                !atlas.Contains(r.firstAdjustmentRecord.glyphIndex) ||
                !atlas.Contains(r.secondAdjustmentRecord.glyphIndex));
            table.MarkToBaseAdjustmentRecords?.RemoveAll(r =>
                !atlas.Contains(r.baseGlyphID) || !atlas.Contains(r.markGlyphID));
            table.MarkToMarkAdjustmentRecords?.RemoveAll(r =>
                !atlas.Contains(r.baseMarkGlyphID) || !atlas.Contains(r.combiningMarkGlyphID));
            table.ligatureRecords?.RemoveAll(r =>
                !atlas.Contains(r.ligatureGlyphID) || !AllInAtlas(r.componentGlyphIDs, atlas));
            table.multipleSubstitutionRecords?.RemoveAll(r =>
                !atlas.Contains(r.targetGlyphID) || !AllInAtlas(r.substituteGlyphIDs, atlas));

            int[] after = CountRecords(table);

            // Antes de guardar: reconstruye las búsquedas en memoria, y lo que TMP ajuste al releer
            // el asset queda escrito en el mismo guardado en vez de dejarlo sucio.
            fontAsset.ReadFontAssetDefinition();
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssetIfDirty(fontAsset);
            sizeAfter = new FileInfo(path).Length;

            int removed = 0;
            StringBuilder report = new StringBuilder();
            report.AppendLine($"{LOG}{fontAsset.name} · glifos en el atlas: {atlas.Count}");
            for (int i = 0; i < TABLE_NAMES.Length; i++)
            {
                removed += before[i] - after[i];
                report.AppendLine($"  {TABLE_NAMES[i]}: {before[i]} → {after[i]} (quitados {before[i] - after[i]})");
            }
            report.Append($"  Disco: {ToMegabytes(sizeBefore)} MB → {ToMegabytes(sizeAfter)} MB");

            if (sizeAfter > SIZE_LIMIT_BYTES)
            {
                Debug.LogWarning($"{report} · sigue por encima de 5 MB");
            }
            else
            {
                Debug.Log(report.ToString());
            }

            return removed;
        }

        /// <summary>Registros por tabla, en el orden de <see cref="TABLE_NAMES"/>.</summary>
        private static int[] CountRecords(TMP_FontFeatureTable table)
        {
            return new[]
            {
                Count(table.glyphPairAdjustmentRecords),
                Count(table.MarkToBaseAdjustmentRecords),
                Count(table.MarkToMarkAdjustmentRecords),
                Count(table.ligatureRecords),
                Count(table.multipleSubstitutionRecords)
            };
        }

        private static int Count<T>(List<T> records)
        {
            return records == null ? 0 : records.Count;
        }

        private static bool AllInAtlas(uint[] glyphIds, HashSet<uint> atlas)
        {
            if (glyphIds == null)
            {
                return true;
            }

            foreach (uint id in glyphIds)
            {
                if (!atlas.Contains(id))
                {
                    return false;
                }
            }

            return true;
        }

        private static string ToMegabytes(long bytes)
        {
            return (bytes / BYTES_PER_MB).ToString("0.00");
        }
    }
}

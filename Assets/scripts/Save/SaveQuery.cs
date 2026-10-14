using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalFlux.SaveSystem
{
    public enum SaveSort { Date, Wave, Dps, Name }

    public static class SaveQuery
    {
        private static readonly char[] Separators = { ' ', '\t', ',' };

        public static string[] Terms(string search)
            => string.IsNullOrWhiteSpace(search)
                ? Array.Empty<string>()
                : search.ToLowerInvariant().Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        public static string Haystack(SaveMeta m)
        {
            var sb = new StringBuilder();
            sb.Append(m.name).Append(' ').Append(m.modeName).Append(' ').Append(m.difficultyName);
            if (m.ironman) sb.Append(" ironman");
            if (m.attackNames != null)
                foreach (string a in m.attackNames) sb.Append(' ').Append(a);
            return sb.ToString().ToLowerInvariant();
        }

        public static bool Matches(SaveMeta m, string[] terms)
        {
            if (terms == null || terms.Length == 0) return true;

            string h = Haystack(m);
            foreach (string t in terms)
                if (!h.Contains(t)) return false;
            return true;
        }

        public static List<SaveFile> Apply(List<SaveFile> files, string search, SaveSort sort)
        {
            string[] terms = Terms(search);
            var result = new List<SaveFile>();
            if (files == null) return result;

            foreach (SaveFile f in files)
                if (f?.meta != null && Matches(f.meta, terms)) result.Add(f);

            result.Sort((a, b) => sort switch
            {
                SaveSort.Wave => b.meta.wave.CompareTo(a.meta.wave),
                SaveSort.Dps => b.meta.peakDps.CompareTo(a.meta.peakDps),
                SaveSort.Name => string.Compare(a.meta.name, b.meta.name, StringComparison.OrdinalIgnoreCase),
                _ => string.CompareOrdinal(b.meta.savedAtUtc, a.meta.savedAtUtc)
            });

            return result;
        }
    }
}

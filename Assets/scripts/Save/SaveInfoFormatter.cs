using System;
using System.Globalization;
using System.Text;

namespace CrystalFlux.SaveSystem
{
    public static class SaveInfoFormatter
    {
        public static string FormatPlaytime(float seconds)
        {
            int total = Math.Max(0, (int)seconds);
            int h = total / 3600;
            int m = total % 3600 / 60;
            int s = total % 60;
            return h > 0 ? $"{h}h {m:00}m" : $"{m}m {s:00}s";
        }

        public static string FormatDate(string iso)
            => DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime d)
                ? d.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                : iso ?? "";

        public static string Lines(SaveMeta m, string incompatibleReason)
        {
            var sb = new StringBuilder();
            sb.Append("Wave ").Append(m.wave).Append('\n');
            sb.Append(m.modeName).Append(" / ").Append(m.difficultyName);
            if (m.ironman) sb.Append(" / Ironman");
            sb.Append('\n');
            sb.Append("Highest DPS: ").Append(m.peakDps.ToString("N0", CultureInfo.InvariantCulture)).Append('\n');
            if (m.attackNames != null && m.attackNames.Count > 0) sb.Append("Attacks: ").Append(string.Join(", ", m.attackNames)).Append('\n');
            sb.Append("Playtime: ").Append(FormatPlaytime(m.playtimeSeconds)).Append('\n');
            sb.Append("Saved: ").Append(FormatDate(m.savedAtUtc));
            if (m.readOnly) sb.Append("\n<color=#FFC857>Read only (Ironman)</color>");
            if (!string.IsNullOrEmpty(incompatibleReason)) sb.Append("\n<color=#FF6B6B>Incompatible: ").Append(incompatibleReason).Append("</color>");
            return sb.ToString();
        }
    }
}

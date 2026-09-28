using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public static class NumberFormat
    {
        public static string Abbrev(int val)
        {
            int abs = Mathf.Abs(val);

            if (abs >= 1_000_000) return (val / 1_000_000f).ToString("0.0") + "M";
            if (abs >= 1_000) return (val / 1_000f).ToString("0.0") + "k";
            return val.ToString();
        }

        public static string Bar(int cur, int max, int over = 0) =>
            over > 0
                ? $"{Abbrev(cur + over)}(+{Abbrev(over)})/{Abbrev(max)}"
                : $"{Abbrev(cur)}/{Abbrev(max)}";
    }
}

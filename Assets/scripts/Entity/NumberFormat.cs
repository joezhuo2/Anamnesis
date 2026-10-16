using System.Globalization;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public static class NumberFormat
    {
        private static readonly string[] Suffixes = { "", "k", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

        public static string Abbrev(float val)
        {
            float v = val;
            int i = 0;
            while (i < Suffixes.Length - 1 && Mathf.Abs(v) >= 999.95f)
            {
                v /= 1000f;
                i++;
            }

            string fmt = i == 0 ? "0.##" : "0.#";
            string s = v.ToString(fmt, CultureInfo.InvariantCulture);
            if (s == "-0") s = "0";
            return s + Suffixes[i];
        }

        public static string Signed(float val) => (val >= 0f ? "+" : "") + Abbrev(val);

        public static string Bar(float cur, float max, float over = 0f) =>
            over > 0f
                ? $"{Abbrev(cur + over)}(+{Abbrev(over)})/{Abbrev(max)}"
                : $"{Abbrev(cur)}/{Abbrev(max)}";
    }
}

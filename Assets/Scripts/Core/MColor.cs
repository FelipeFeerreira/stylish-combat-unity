using UnityEngine;

namespace HnS.Core
{
    public static class MColor
    {
        public static void Set(Material m, Color c)
        {
            if (m == null) return;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        }

        public static Color Get(Material m)
        {
            if (m == null) return Color.white;
            if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
            if (m.HasProperty("_Color")) return m.GetColor("_Color");
            return Color.white;
        }

        public static void Set(Renderer r, Color c)
        {
            if (r != null) Set(r.material, c);
        }
    }
}
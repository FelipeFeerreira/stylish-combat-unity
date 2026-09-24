using UnityEngine;
using UnityEngine.UI;

namespace HnS.UI
{
    public class DamageNumber : MonoBehaviour
    {
        Text text;
        float t;
        float duration = 0.7f;
        float rise = 1.3f;
        Color color;
        Vector3 world;

        public static void Show(Vector3 worldPos, string str, Color c, float size, float dur = 0.7f)
        {
            var ui = UIManager.Instance;
            if (ui == null) return;

            var go = new GameObject("DmgNum", typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(ui.CanvasRect, false);

            var dn = go.AddComponent<DamageNumber>();
            dn.Build(str, c, size, dur, worldPos);
        }

        void Build(string str, Color c, float size, float dur, Vector3 worldPos)
        {
            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(200f, 60f);

            text = gameObject.AddComponent<Text>();
            var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.font = f;
            text.fontSize = (int)size;
            text.text = str;
            text.color = c;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var o = gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.8f);
            o.effectDistance = new Vector2(2f, -2f);

            color = c;
            duration = dur;
            world = worldPos;
        }

        void Update()
        {
            t += Time.deltaTime / duration;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            world += Vector3.up * rise * Time.deltaTime;

            var cam = Camera.main;
            var ui = UIManager.Instance;
            if (cam == null || ui == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 sp = cam.WorldToScreenPoint(world);
            Vector2 local = ui.ScreenToCanvas(sp);
            ((RectTransform)transform).anchoredPosition = local;

            float fade = Mathf.Clamp01(1f - t);
            text.color = new Color(color.r, color.g, color.b, fade);
        }
    }
}
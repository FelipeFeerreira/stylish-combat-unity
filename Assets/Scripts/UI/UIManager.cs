using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HnS.CameraControl;

namespace HnS.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        public LockOn lockOnTarget;

        public RectTransform CanvasRect { get { return canvasRect; } }

        RectTransform canvasRect;
        CanvasScaler scaler;
        Image healthFill;
        Text styleLetter;
        Text styleLabel;
        Text styleScore;
        Text styleCombo;
        Text centerText;
        Text hintText;
        Text reticleText;
        Text restartText;
        Image redOverlay;

        float hintTimer;
        float hintTotal;
        float centerTimer;
        float centerTotal;
        float restartAlpha;
        bool over;

        static Font cachedFont;

        void Awake()
        {
            Instance = this;
            Build();
        }

        static Font Font()
        {
            if (cachedFont != null) return cachedFont;
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return cachedFont;
        }

        RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        Text Text(RectTransform parent, string name, string content, int size, Color color, TextAnchor align)
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font();
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;

            var o = rt.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.85f);
            o.effectDistance = new Vector2(2.5f, -2.5f);
            return t;
        }

        Image Img(RectTransform parent, string name, Color color)
        {
            var rt = Rect(name, parent);
            var i = rt.gameObject.AddComponent<Image>();
            i.color = color;
            i.raycastTarget = false;
            return i;
        }

        public Vector2 ScreenToCanvas(Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 local);
            return local;
        }

        void Build()
        {
            var go = new GameObject("UI");
            go.layer = 5;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            canvasRect = (RectTransform)go.transform;

            RectTransform barRoot = Rect("HealthBar", canvasRect);
            SetRect(barRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -30f), new Vector2(340f, 28f));

            Image barBg = Img(barRoot, "BarBG", new Color(0f, 0f, 0f, 0.8f));
            barBg.rectTransform.anchorMin = Vector2.zero;
            barBg.rectTransform.anchorMax = Vector2.one;
            barBg.rectTransform.offsetMin = Vector2.zero;
            barBg.rectTransform.offsetMax = Vector2.zero;

            healthFill = Img(barRoot, "BarFill", new Color(0.2f, 0.55f, 1f));
            healthFill.rectTransform.anchorMin = new Vector2(0f, 0.15f);
            healthFill.rectTransform.anchorMax = new Vector2(1f, 0.85f);
            healthFill.rectTransform.offsetMin = new Vector2(4f, 0f);
            healthFill.rectTransform.offsetMax = new Vector2(-4f, 0f);

            Text healthLabel = Text(barRoot, "HealthLabel", "VIDA", 15, new Color(0.9f, 0.9f, 0.9f), TextAnchor.MiddleLeft);
            healthLabel.rectTransform.anchorMin = new Vector2(0.02f, 0f);
            healthLabel.rectTransform.anchorMax = new Vector2(0.3f, 1f);
            healthLabel.rectTransform.offsetMin = Vector2.zero;
            healthLabel.rectTransform.offsetMax = Vector2.zero;

            RectTransform styleRoot = Rect("StyleRoot", canvasRect);
            SetRect(styleRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-44f, -40f), new Vector2(320f, 250f));

            styleLabel = Text(styleRoot, "StyleLabel", "ESTILO", 24, new Color(0.8f, 0.8f, 0.85f), TextAnchor.UpperRight);
            styleLabel.rectTransform.anchorMin = Vector2.zero;
            styleLabel.rectTransform.anchorMax = Vector2.one;
            styleLabel.rectTransform.offsetMin = new Vector2(0f, 150f);
            styleLabel.rectTransform.offsetMax = new Vector2(0f, 250f);

            styleLetter = Text(styleRoot, "StyleLetter", "D", 104, new Color(1f, 1f, 1f), TextAnchor.MiddleRight);
            styleLetter.rectTransform.anchorMin = Vector2.zero;
            styleLetter.rectTransform.anchorMax = Vector2.one;
            styleLetter.rectTransform.offsetMin = new Vector2(0f, 40f);
            styleLetter.rectTransform.offsetMax = new Vector2(0f, 150f);

            styleScore = Text(styleRoot, "StyleScore", "00000", 30, new Color(1f, 1f, 1f), TextAnchor.UpperRight);
            styleScore.rectTransform.anchorMin = Vector2.zero;
            styleScore.rectTransform.anchorMax = Vector2.one;
            styleScore.rectTransform.offsetMin = new Vector2(0f, 0f);
            styleScore.rectTransform.offsetMax = new Vector2(0f, 40f);

            RectTransform comboRoot = Rect("ComboRoot", canvasRect);
            SetRect(comboRoot, new Vector2(1f, 0.4f), new Vector2(1f, 0.4f), new Vector2(1f, 0.5f), new Vector2(-44f, 0f), new Vector2(420f, 90f));
            styleCombo = Text(comboRoot, "StyleCombo", "", 44, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleRight);
            styleCombo.rectTransform.anchorMin = Vector2.zero;
            styleCombo.rectTransform.anchorMax = Vector2.one;
            styleCombo.rectTransform.offsetMin = Vector2.zero;
            styleCombo.rectTransform.offsetMax = Vector2.zero;

            var centerRoot = Rect("CenterRoot", canvasRect);
            SetRect(centerRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 200f));
            centerText = Text(centerRoot, "CenterText", "", 54, new Color(1f, 0.95f, 0.85f), TextAnchor.MiddleCenter);
            centerText.rectTransform.anchorMin = Vector2.zero;
            centerText.rectTransform.anchorMax = Vector2.one;
            centerText.rectTransform.offsetMin = Vector2.zero;
            centerText.rectTransform.offsetMax = Vector2.zero;
            centerText.raycastTarget = false;

            RectTransform hintRoot = Rect("HintRoot", canvasRect);
            SetRect(hintRoot, new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 220f));
            hintText = Text(hintRoot, "HintText", "", 22, new Color(1f, 1f, 1f), TextAnchor.MiddleCenter);
            hintText.horizontalOverflow = HorizontalWrapMode.Wrap;
            hintText.rectTransform.anchorMin = Vector2.zero;
            hintText.rectTransform.anchorMax = Vector2.one;
            hintText.rectTransform.offsetMin = Vector2.zero;
            hintText.rectTransform.offsetMax = Vector2.zero;
            hintText.raycastTarget = false;

            RectTransform reticleRoot = Rect("ReticleRoot", canvasRect);
            SetRect(reticleRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80f, 80f));
            reticleText = Text(reticleRoot, "ReticleText", "+", 44, new Color(1f, 0.2f, 0.1f), TextAnchor.MiddleCenter);
            reticleText.rectTransform.anchorMin = Vector2.zero;
            reticleText.rectTransform.anchorMax = Vector2.one;
            reticleText.rectTransform.offsetMin = Vector2.zero;
            reticleText.rectTransform.offsetMax = Vector2.zero;
            reticleText.enabled = false;

            RectTransform restartRoot = Rect("RestartRoot", canvasRect);
            SetRect(restartRoot, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 70f));
            restartText = Text(restartRoot, "RestartText", "PRESSIONE R / START PARA REINICIAR", 30, new Color(1f, 0.9f, 0.6f), TextAnchor.MiddleCenter);
            restartText.rectTransform.anchorMin = Vector2.zero;
            restartText.rectTransform.anchorMax = Vector2.one;
            restartText.rectTransform.offsetMin = Vector2.zero;
            restartText.rectTransform.offsetMax = Vector2.zero;
            Color rc = restartText.color;
            rc.a = 0f;
            restartText.color = rc;

            redOverlay = Img(canvasRect, "RedOverlay", new Color(0.55f, 0f, 0f, 0f));
            redOverlay.rectTransform.anchorMin = Vector2.zero;
            redOverlay.rectTransform.anchorMax = Vector2.one;
            redOverlay.rectTransform.offsetMin = Vector2.zero;
            redOverlay.rectTransform.offsetMax = Vector2.zero;

            SetPlayerHealth(1f);
        }

        public void SetPlayerHealth(float norm)
        {
            if (healthFill == null) return;
            healthFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(norm), 0.85f);
            healthFill.color = Color.Lerp(new Color(0.95f, 0.2f, 0.2f), new Color(0.2f, 0.7f, 1f), Mathf.Clamp01(norm));
        }

        public void ShowStyle(string rank, Color color, int score, int combo)
        {
            if (styleLetter == null) return;
            styleLetter.text = rank;
            styleLetter.color = color;
            styleScore.text = score.ToString("00000");
            styleCombo.text = combo >= 2 ? "x" + combo + " GOLPES" : "";
        }

        public void ShowCenter(string msg, float duration)
        {
            if (centerText == null) return;
            centerText.text = msg;
            centerTimer = duration;
            centerTotal = Mathf.Max(0.01f, duration);
            centerText.color = new Color(1f, 0.95f, 0.85f, 1f);
        }

        public void ShowHint(string msg, float duration)
        {
            if (hintText == null) return;
            hintText.text = msg;
            hintTimer = duration;
            hintTotal = Mathf.Max(0.01f, duration);
            hintText.color = new Color(1f, 1f, 1f, 1f);
        }

        public void SetLockReticle(bool active)
        {
            if (reticleText == null) return;
            reticleText.enabled = active;
        }

        public void RedFlash()
        {
            if (redOverlay == null || !gameObject.activeInHierarchy) return;
            StopCoroutine("FlashRoutine");
            StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                float a = Mathf.Lerp(0.34f, 0f, t / 0.3f);
                redOverlay.color = new Color(0.55f, 0f, 0f, a);
                yield return null;
            }
            redOverlay.color = new Color(0.55f, 0f, 0f, 0f);
        }

        public void GameOver(bool victory)
        {
            if (over) return;
            over = true;
            hintText.enabled = false;
            centerText.text = victory ? "VITORIA!" : "VOCE MORREU";
            centerText.color = victory ? new Color(0.95f, 0.9f, 0.4f) : new Color(1f, 0.25f, 0.2f);
            centerTimer = 12f;
            centerTotal = 12f;
            StartCoroutine(FadeInRestart());
        }

        IEnumerator FadeInRestart()
        {
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                float a = Mathf.Lerp(0f, 1f, t / 0.8f);
                Color c = restartText.color;
                c.a = a;
                restartText.color = c;
                yield return null;
            }
        }

        void Update()
        {
            if (centerTimer > 0f)
            {
                centerTimer -= Time.deltaTime * 0.5f;
                float a = Mathf.Clamp01(centerTimer / centerTotal);
                centerText.color = new Color(1f, 0.95f, 0.85f, a);
            }

            if (hintTimer > 0f)
            {
                hintTimer -= Time.deltaTime;
                float a = Mathf.Clamp01(hintTimer / hintTotal);
                hintText.color = new Color(1f, 1f, 1f, a);
            }

            if (lockOnTarget != null && lockOnTarget.Active && lockOnTarget.Target != null)
            {
                reticleText.enabled = true;
                Vector3 p = lockOnTarget.Target.position + Vector3.up * 0.8f;
                var cam = Camera.main;
                if (cam != null)
                {
                    Vector2 sp = cam.WorldToScreenPoint(p);
                    Vector2 local = ScreenToCanvas(sp);
                    reticleText.rectTransform.anchoredPosition = local;
                }
            }
            else if (reticleText.enabled)
            {
                reticleText.enabled = false;
            }
        }
    }
}
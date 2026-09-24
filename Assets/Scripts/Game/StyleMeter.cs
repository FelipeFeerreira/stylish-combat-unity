using UnityEngine;
using HnS.Core;
using HnS.Enemy;
using HnS.Player;
using HnS.UI;

namespace HnS.Game
{
    public class StyleMeter : MonoBehaviour
    {
        public static StyleMeter Instance;

        readonly string[] rankNames = { "D", "C", "B", "A", "S", "SS", "SSS" };
        readonly float[] thresholds = { 0f, 1200f, 2800f, 5000f, 9000f, 14000f, 20000f };
        readonly Color[] rankColors =
        {
            new Color(0.6f, 0.6f, 0.62f),
            new Color(0.2f, 0.85f, 0.3f),
            new Color(0.2f, 0.55f, 1f),
            new Color(0.6f, 0.3f, 1f),
            new Color(0.2f, 0.95f, 1f),
            new Color(1f, 0.9f, 0.2f),
            new Color(1f, 0.4f, 0.15f),
        };

        float score;
        int combo;
        float comboTimer;
        float lastEvent = -99f;

        public int RankIndex { get; private set; }
        public PlayerController player;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null) return;

            if (player.combat != null)
            {
                player.combat.StyleScored += OnStyleEvent;
                player.combat.HitLanded += OnHitLanded;
            }
            player.Evaded += OnEvaded;
        }

        void Update()
        {
            if (Time.time - lastEvent > 2.5f)
            {
                score = Mathf.Max(0f, score - 320f * Time.deltaTime);
            }

            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f) combo = 0;
            }

            RankIndex = 0;
            for (int i = thresholds.Length - 1; i >= 0; i--)
            {
                if (score >= thresholds[i]) { RankIndex = i; break; }
            }

            UIManager.Instance?.ShowStyle(rankNames[RankIndex], rankColors[RankIndex], Mathf.RoundToInt(score), combo);
        }

        void OnStyleEvent(string tag)
        {
            int v = 0;
            if (tag == StyleTags.Slash) v = 100;
            else if (tag == StyleTags.Finisher) v = 200;
            else if (tag == StyleTags.Launcher) v = 150;
            else if (tag == StyleTags.AerialHit) v = 50;
            else if (tag == StyleTags.Slam) v = 90;
            else if (tag == StyleTags.Gunshot) v = 20;

            AddScore(v);
        }

        void OnHitLanded()
        {
            combo++;
            comboTimer = 1.8f;
        }

        void OnEvaded()
        {
            AddScore(100);
            if (player != null)
            {
                DamageNumber.Show(player.transform.position + Vector3.up * 2.1f, "+100 ESQUIVA", new Color(0.2f, 0.95f, 1f), 26f, 0.5f);
            }
        }

        public void OnPlayerHit()
        {
            score *= 0.5f;
        }

        public void OnAirHit(int hits)
        {
            combo++;
            comboTimer = 1.8f;
            AddScore(30 + hits * 5);
        }

        public void AddKill(Demon d)
        {
            int v = 250 + (d.isBoss ? 1000 : 0);
            AddScore(v);
            DamageNumber.Show(d.transform.position + Vector3.up * 1.8f, "+" + v, new Color(1f, 0.8f, 0.2f), 34f, 0.9f);
        }

        void AddScore(int v)
        {
            if (v <= 0) return;
            float mult = 1f + combo * 0.04f;
            score += v * mult;
            lastEvent = Time.time;
        }
    }
}
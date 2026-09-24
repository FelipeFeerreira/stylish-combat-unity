using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HnS.Core;

namespace HnS.Player
{
    public class PlayerVisual : MonoBehaviour
    {
        public PlayerController owner;

        Transform body;
        Transform torso;
        Transform head;
        Transform swordPivot;
        Transform swordBlade;
        Transform lArm;
        List<Renderer> renderers = new List<Renderer>();
        bool built;

        static readonly Vector3[][] SLASH = new Vector3[][]
        {
            new[] { new Vector3(-115f, 12f, 0f), new Vector3(35f, -8f, 0f), new Vector3(-14f, 0f, 0f) },
            new[] { new Vector3(0f, 48f, -10f), new Vector3(0f, -55f, 8f), new Vector3(0f, -15f, 0f) },
            new[] { new Vector3(-135f, 0f, 0f), new Vector3(55f, 0f, 0f), new Vector3(-18f, 0f, 0f) },
            new[] { new Vector3(-35f, -40f, 0f), new Vector3(-100f, 0f, 0f), new Vector3(-18f, -5f, 0f) },
            new[] { new Vector3(0f, 30f, -6f), new Vector3(0f, -60f, 8f), new Vector3(0f, -10f, 0f) },
            new[] { new Vector3(45f, 0f, 0f), new Vector3(-95f, 0f, 0f), new Vector3(-30f, 0f, 0f) },
        };

        public PlayerCombat Combat { get { return owner != null ? owner.combat : null; } }

        void Awake()
        {
            owner = GetComponent<PlayerController>();
            BuildIfNeeded();
        }

        public void BuildIfNeeded()
        {
            if (built) return;
            if (transform.Find("Knight") != null)
            {
                built = true;
                return;
            }
            built = true;
            Build();
        }

        void Build()
        {
            body = new GameObject("Knight").transform;
            body.SetParent(transform, false);

            torso = MakePart("Torso", body, Vector3.up * 0.95f, new Vector3(0.48f, 0.6f, 0.3f), new Color(0.93f, 0.91f, 0.85f));
            MakePart("Cross", torso, new Vector3(0f, 0.1f, 0.19f), new Vector3(0.03f, 0.28f, 0.02f), new Color(0.65f, 0.08f, 0.08f));
            MakePart("CrossBar", torso, new Vector3(0f, 0.0f, 0.19f), new Vector3(0.2f, 0.03f, 0.02f), new Color(0.65f, 0.08f, 0.08f));

            head = MakePart("Head", body, new Vector3(0f, 1.62f, 0f), new Vector3(0.5f, 0.5f, 0.5f), new Color(0.72f, 0.73f, 0.76f));
            MakePart("HelmSlit", head, new Vector3(0f, -0.08f, 0.27f), new Vector3(0.34f, 0.04f, 0.02f), new Color(0.12f, 0.12f, 0.14f));

            MakePart("LArm", body, new Vector3(-0.34f, 1.2f, 0f), new Vector3(0.14f, 0.48f, 0.14f), new Color(0.93f, 0.91f, 0.85f));
            MakePart("RArm", body, new Vector3(0.34f, 1.2f, 0f), new Vector3(0.14f, 0.48f, 0.14f), new Color(0.93f, 0.91f, 0.85f));
            MakePart("LPalm", body, new Vector3(-0.34f, 0.9f, 0f), new Vector3(0.18f, 0.16f, 0.18f), new Color(0.72f, 0.73f, 0.76f));
            MakePart("RPalm", body, new Vector3(0.34f, 0.9f, 0f), new Vector3(0.18f, 0.16f, 0.18f), new Color(0.72f, 0.73f, 0.76f));

            MakePart("LLeg", body, new Vector3(-0.16f, 0.42f, 0f), new Vector3(0.2f, 0.62f, 0.2f), new Color(0.45f, 0.33f, 0.22f));
            MakePart("RLeg", body, new Vector3(0.16f, 0.42f, 0f), new Vector3(0.2f, 0.62f, 0.2f), new Color(0.45f, 0.33f, 0.22f));

            lArm = MakePart("Shield", body, new Vector3(-0.45f, 1.05f, 0.03f), new Vector3(0.45f, 0.62f, 0.07f), new Color(0.62f, 0.64f, 0.68f));
            MakePart("ShieldCross", lArm, new Vector3(0f, 0f, 0.05f), new Vector3(0.04f, 0.2f, 0.01f), new Color(0.65f, 0.08f, 0.08f));

            swordPivot = new GameObject("SwordPivot").transform;
            swordPivot.SetParent(torso, false);
            swordPivot.localPosition = new Vector3(0.42f, -0.3f, 0f);

            MakePart("Guard", swordPivot, new Vector3(0f, 0f, 0.14f), new Vector3(0.22f, 0.08f, 0.06f), new Color(0.55f, 0.45f, 0.25f));
            swordBlade = MakePart("Blade", swordPivot, new Vector3(0f, 0.06f, 0.75f), new Vector3(0.08f, 0.05f, 1.35f), new Color(0.82f, 0.84f, 0.9f));
            MakePart("GuardCross", swordPivot, new Vector3(0f, 0f, 0.34f), new Vector3(0.08f, 0.05f, 0.05f), new Color(0.55f, 0.45f, 0.25f));
        }

        Transform MakePart(string name, Transform parent, Vector3 localPos, Vector3 localScale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;

            var coll = go.GetComponent<Collider>();
            Util.SafeDestroy(coll);

            var mr = go.GetComponent<MeshRenderer>();
            MColor.Set(mr, color);
            renderers.Add(mr);
            return go.transform;
        }

        void Update()
        {
            if (!built || body == null) return;

            if (owner == null) return;

            if (owner.Dead)
            {
                return;
            }

            var combat = Combat;
            bool attacking = combat != null && combat.IsAttacking;

            if (attacking)
            {
                AnimateAttack();
            }
            else
            {
                IdlePose();
            }
        }

        void AnimateAttack()
        {
            var combat = Combat;
            if (combat == null || combat.Current == null) return;
            var c = combat;
            var d = c.Current;
            float t = c.PhaseT;
            int id = c.VisualId;
            if (id < 0 || id >= SLASH.Length) id = 0;

            Vector3 k1 = SLASH[id][0];
            Vector3 k2 = SLASH[id][1];
            Vector3 k3 = SLASH[id][2];

            Vector3 target;
            if (t < d.windup)
            {
                float k = Mathf.Clamp01(t / Mathf.Max(0.001f, d.windup));
                target = Vector3.Slerp(Vector3.zero, k1, k);
            }
            else if (t < d.windup + d.active)
            {
                float k = Mathf.Clamp01((t - d.windup) / Mathf.Max(0.001f, d.active));
                target = Vector3.Slerp(k1, k2, SmoothStep(k));
            }
            else
            {
                float k = Mathf.Clamp01((t - d.windup - d.active) / Mathf.Max(0.001f, d.recovery));
                target = Vector3.Slerp(k2, k3, SmoothStep(k));
            }

            swordPivot.localEulerAngles = target;
            torso.localEulerAngles = new Vector3(target.x * 0.35f, 0f, 0f);
        }

        void IdlePose()
        {
            float t = Time.time;
            float bob = Mathf.Sin(t * 2.2f) * 0.5f;
            swordPivot.localEulerAngles = new Vector3(-12f, 6f, 0f) * (1f + bob * 0.02f);
            torso.localEulerAngles = Vector3.zero;
            torso.localPosition = new Vector3(0f, bob * 0.012f, 0f);

            float moveX = owner.MoveInput.x;
            float moveY = owner.MoveInput.y;
            torso.localEulerAngles = new Vector3(-moveY * 4f, 0f, -moveX * 5f);
            head.localEulerAngles = new Vector3(0f, moveX * 10f, 0f);
        }

        static float SmoothStep(float x)
        {
            return x * x * (3f - 2f * x);
        }

        public void HurtFlash()
        {
            if (!built) return;
            StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            foreach (var r in renderers)
            {
                if (r != null) MColor.Set(r, new Color(1f, 1f, 1f));
            }
            yield return new WaitForSeconds(0.05f);
            foreach (var r in renderers)
            {
                if (r != null) RestoreColor(r);
            }
        }

        void RestoreColor(Renderer r)
        {
            string n = r.gameObject.name;
            if (n == "Torso" || n == "LArm" || n == "RArm") MColor.Set(r, new Color(0.93f, 0.91f, 0.85f));
            else if (n == "Cross" || n == "CrossBar" || n == "ShieldCross") MColor.Set(r, new Color(0.65f, 0.08f, 0.08f));
            else if (n == "LLeg" || n == "RLeg") MColor.Set(r, new Color(0.45f, 0.33f, 0.22f));
            else if (n == "Shield") MColor.Set(r, new Color(0.62f, 0.64f, 0.68f));
            else if (n == "Guard" || n == "GuardCross") MColor.Set(r, new Color(0.55f, 0.45f, 0.25f));
            else MColor.Set(r, IsBlade(n) ? new Color(0.82f, 0.84f, 0.9f) : new Color(0.93f, 0.91f, 0.85f));
        }

        bool IsBlade(string n) { return n == "Blade"; }

        public void DoDodge(bool dodging)
        {
            if (!built) return;
            StartCoroutine(DodgeRoutine());
        }

        IEnumerator DodgeRoutine()
        {
            for (float t = 0f; t < 0.18f; t += Time.deltaTime)
            {
                if (torso != null)
                    torso.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 12f, t / 0.18f));
                yield return null;
            }
            for (float t = 0f; t < 0.12f; t += Time.deltaTime)
            {
                if (torso != null)
                    torso.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(12f, 0f, t / 0.12f));
                yield return null;
            }
            if (torso != null) torso.localRotation = Quaternion.identity;
        }

        public void ShootRecoil()
        {
            if (!built) return;
            StartCoroutine(RecoilRoutine());
        }

        IEnumerator RecoilRoutine()
        {
            for (float t = 0f; t < 0.06f; t += Time.deltaTime)
            {
                if (torso != null) torso.localPosition = new Vector3(0f, 0f, -t * 2f);
                yield return null;
            }
        }

        public void Die()
        {
            StartCoroutine(FallAndSink());
        }

        IEnumerator FallAndSink()
        {
            if (body != null)
            {
                for (float t = 0f; t < 0.5f; t += Time.deltaTime)
                {
                    body.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 90f, t / 0.5f), 0f, 0f);
                    yield return null;
                }
            }
        }
    }
}
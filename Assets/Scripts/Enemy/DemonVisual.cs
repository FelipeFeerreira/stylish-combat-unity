using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HnS.Core;

namespace HnS.Enemy
{
    public class DemonVisual : MonoBehaviour
    {
        Transform body;
        Transform chest;
        Transform head;
        Transform lArm;
        Transform rArm;
        List<Renderer> renderers = new List<Renderer>();
        List<Color> baseColors = new List<Color>();
        bool built;
        bool boss;
        float scale = 1f;
        Demon owner;

        void Awake()
        {
            owner = GetComponent<Demon>();
            Build();
        }

        public void ApplyBoss(bool isBoss)
        {
            boss = isBoss;
            if (!boss) return;
            if (body != null) body.localScale *= 1.55f;
        }

        void Build()
        {
            if (built) return;
            built = true;

            body = new GameObject("DemonMesh").transform;
            body.SetParent(transform, false);
            scale = transform.localScale.x > 1.1f ? transform.localScale.x : 1f;
            body.localScale = Vector3.one * scale;

            var bodyColor = new Color(0.34f, 0.06f, 0.09f);
            var boneColor = new Color(0.55f, 0.45f, 0.36f);
            var eyeColor = new Color(1f, 0.2f, 0.05f);

            chest = Make("Chest", body, new Vector3(0f, 0.95f, 0f), new Vector3(0.75f, 0.62f, 0.42f), bodyColor);
            head = Make("Head", body, new Vector3(0f, 1.62f, 0f), new Vector3(0.46f, 0.4f, 0.46f), bodyColor);

            Make("HornL", head, new Vector3(-0.16f, 0.2f, 0.05f), new Vector3(0.08f, 0.26f, 0.08f), boneColor);
            Make("HornR", head, new Vector3(0.16f, 0.2f, 0.05f), new Vector3(0.08f, 0.26f, 0.08f), boneColor);

            Make("EyeL", head, new Vector3(-0.12f, -0.02f, 0.24f), new Vector3(0.08f, 0.03f, 0.02f), eyeColor, true);
            Make("EyeR", head, new Vector3(0.12f, -0.02f, 0.24f), new Vector3(0.08f, 0.03f, 0.02f), eyeColor, true);

            Make("LegL", body, new Vector3(-0.22f, 0.5f, 0f), new Vector3(0.28f, 0.6f, 0.28f), bodyColor);
            Make("LegR", body, new Vector3(0.22f, 0.5f, 0f), new Vector3(0.28f, 0.6f, 0.28f), bodyColor);

            lArm = Make("ArmL", body, new Vector3(-0.5f, 1.25f, 0f), new Vector3(0.16f, 0.6f, 0.16f), bodyColor);
            rArm = Make("ArmR", body, new Vector3(0.5f, 1.25f, 0f), new Vector3(0.16f, 0.6f, 0.16f), bodyColor);

            Make("ClawL", lArm, new Vector3(0f, -0.45f, 0.05f), new Vector3(0.12f, 0.24f, 0.06f), boneColor);
            Make("ClawR", rArm, new Vector3(0f, -0.45f, 0.05f), new Vector3(0.12f, 0.24f, 0.06f), boneColor);
        }

        Transform Make(string name, Transform parent, Vector3 localPos, Vector3 localScale, Color color, bool emissive = false)
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
            if (emissive)
            {
                var mat = mr.material;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 2.2f);
            }
            baseColors.Add(MColor.Get(mr.material));
            renderers.Add(mr);
            return go.transform;
        }

        void Update()
        {
            if (!built) return;

            if (boss && owner != null && !owner.Dead)
            {
                float p = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
                body.localScale = Vector3.one * scale * (1.55f + p * 0.04f);
            }

            if (owner != null && owner.Dead)
            {
                return;
            }

            if (armTween > 0f)
            {
                armTween -= Time.deltaTime * 3f;
                SetArms(Mathf.Clamp01(armTween));
            }
            else
            {
                float s = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f;
                SetArms(0.25f + s * 0.12f);
            }
        }

        float armTween;

        public void JugglePose(float airTime)
        {
            float flail = Mathf.Sin(airTime * 18f) * 0.4f;
            if (lArm != null) lArm.localRotation = Quaternion.Euler(flail * 40f, 0f, 30f);
            if (rArm != null) rArm.localRotation = Quaternion.Euler(-flail * 40f, 0f, -30f);
        }

        void SetArms(float t)
        {
            if (lArm != null) lArm.localRotation = Quaternion.Euler(t * 40f, 0f, 18f);
            if (rArm != null) rArm.localRotation = Quaternion.Euler(t * 40f, 0f, -18f);
        }

        public void Flash()
        {
            if (!built) return;
            StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null) MColor.Set(renderers[i], new Color(1.5f, 1.15f, 1.15f));
            }
            yield return new WaitForSeconds(0.045f);
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null && i < baseColors.Count) MColor.Set(renderers[i], baseColors[i]);
            }
        }

        public void Die()
        {
            StartCoroutine(DeathRoutine());
        }

        IEnumerator DeathRoutine()
        {
            float t = 0f;
            float dur = 1f;
            while (t < dur)
            {
                t += Time.deltaTime;
                if (body != null)
                {
                    float k = t / dur;
                    body.localScale = Vector3.one * scale * (1f - k * 0.9f);
                    body.localRotation = Quaternion.Euler(k * 90f, 0f, 0f);
                    Vector3 p = body.localPosition;
                    p.y = -k * 0.4f;
                    body.localPosition = p;
                }
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
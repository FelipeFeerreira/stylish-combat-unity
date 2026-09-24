using UnityEngine;
using HnS.Core;

namespace HnS.Player
{
    public class GunProjectile : MonoBehaviour
    {
        float damage;
        float speed = 28f;
        float life;
        Vector3 dir;

        public void Launch(Vector3 position, Vector3 direction, float dmg)
        {
            transform.position = position;
            dir = direction;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;
            dir.Normalize();
            damage = dmg;
            life = 2.5f;

            BuildVisual();
        }

        void BuildVisual()
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "BoltMesh";
            sphere.transform.SetParent(transform, false);
            sphere.transform.localScale = Vector3.one * 0.12f;
            var mr = sphere.GetComponent<MeshRenderer>();
            MColor.Set(mr, new Color(1f, 0.85f, 0.3f));
            Util.SafeDestroy(sphere.GetComponent<Collider>());
        }

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 step = dir * speed * Time.deltaTime;

            RaycastHit hit;
            int mask = ~(1 << Layers.Player);
            if (Physics.Raycast(transform.position, dir, out hit, step.magnitude, mask, QueryTriggerInteraction.Ignore))
            {
                var cc = hit.collider as CharacterController;
                if (cc != null)
                {
                    var target = cc.GetComponent<IDamageable>();
                    if (target != null)
                    {
                        var info = new HitInfo { damage = damage, direction = dir, knockback = 3f, launch = 0f, styleTag = StyleTags.Gunshot };
                        target.ReceiveHit(info, hit.point);
                    }
                }

                SpawnImpact(hit.point);
                Destroy(gameObject);
                return;
            }

            transform.position += step;
        }

        void SpawnImpact(Vector3 point)
        {
            var spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "ImpactSpark";
            spark.transform.position = point + dir * 0.05f;
            spark.transform.localScale = Vector3.one * 0.22f;
            Util.SafeDestroy(spark.GetComponent<Collider>());
            var mr = spark.GetComponent<MeshRenderer>();
            MColor.Set(mr, new Color(1f, 0.9f, 0.4f));
            var fade = spark.AddComponent<FadeOut>();
            fade.duration = 0.18f;
        }
    }

    public class FadeOut : MonoBehaviour
    {
        public float duration = 0.2f;
        float t;
        Renderer[] rends;

        void Start()
        {
            rends = GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) { Destroy(gameObject); }
        }

        void Update()
        {
            t += Time.deltaTime / duration;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }
            foreach (var r in rends)
            {
                Color c = MColor.Get(r.material);
                c.a = Mathf.Clamp01(1f - t);
                MColor.Set(r.material, c);
            }
        }
    }
}
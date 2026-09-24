using System;
using UnityEngine;
using HnS.CameraControl;
using HnS.Core;
using HnS.Enemy;
using HnS.UI;

namespace HnS.Player
{
    [Serializable]
    public class AttackDef
    {
        public string name = "Attack";
        public float damage = 20f;
        public float windup = 0.06f;
        public float active = 0.14f;
        public float recovery = 0.24f;
        public float knockback = 6f;
        public float launch = 0f;
        public float lunge = 2.5f;
        public float hitstop = 0.02f;
        public bool bigHit;
        public string styleTag = StyleTags.Slash;
    }

    public class PlayerCombat : MonoBehaviour
    {
        public PlayerController owner;
        public Hitbox swordHitbox;

        public AttackDef slash1;
        public AttackDef slash2;
        public AttackDef smash;
        public AttackDef launcher;
        public AttackDef airSwipe;
        public AttackDef airPound;

        public event Action<string> StyleScored;
        public event Action HitLanded;

        enum Phase { Idle, Windup, Active, Recovery }
        Phase phase = Phase.Idle;
        float phaseT;
        AttackDef current;
        int comboStep;
        int pending = -1;
        float lastActionTime;
        const float comboWindow = 0.9f;

        public bool IsAttacking { get { return phase != Phase.Idle; } }
        public bool InActiveWindow { get { return phase == Phase.Active; } }
        public bool InRecovery { get { return phase == Phase.Recovery; } }
        public float PhaseT { get { return phaseT; } }
        public AttackDef Current { get { return current; } }
        public int VisualId { get; private set; }

        void Awake()
        {
            owner = GetComponent<PlayerController>();

            if (slash1 == null) slash1 = new AttackDef { name = "Slash 1", damage = 18f, windup = 0.07f, active = 0.13f, recovery = 0.22f, knockback = 6f, lunge = 2.6f, styleTag = StyleTags.Slash, hitstop = 0.02f };
            if (slash2 == null) slash2 = new AttackDef { name = "Slash 2", damage = 24f, windup = 0.08f, active = 0.15f, recovery = 0.24f, knockback = 8f, lunge = 2.4f, styleTag = StyleTags.Slash, hitstop = 0.025f };
            if (smash == null) smash = new AttackDef { name = "Esmagamento", damage = 55f, windup = 0.14f, active = 0.18f, recovery = 0.36f, knockback = 16f, lunge = 3.4f, styleTag = StyleTags.Finisher, hitstop = 0.06f, bigHit = true };
            if (launcher == null) launcher = new AttackDef { name = "Lançador", damage = 18f, windup = 0.11f, active = 0.15f, recovery = 0.3f, knockback = 5f, launch = 10f, lunge = 2f, styleTag = StyleTags.Launcher, hitstop = 0.045f };
            if (airSwipe == null) airSwipe = new AttackDef { name = "Corte Aéreo", damage = 22f, windup = 0.05f, active = 0.16f, recovery = 0.26f, knockback = 5f, launch = 2.5f, lunge = 0.4f, styleTag = StyleTags.AerialHit, hitstop = 0.025f };
            if (airPound == null) airPound = new AttackDef { name = "Craveira", damage = 42f, windup = 0.08f, active = 0.14f, recovery = 0.24f, knockback = 12f, launch = 0f, lunge = 0f, styleTag = StyleTags.Slam, hitstop = 0.05f, bigHit = true };

            BuildHitbox();
        }

        void BuildHitbox()
        {
            var go = new GameObject("SwordHit");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.2f, 0.65f);
            go.layer = gameObject.layer;

            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.75f;

            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            swordHitbox = go.AddComponent<Hitbox>();
            swordHitbox.onHit = FrameHit;
            go.SetActive(false);
        }

        void Update()
        {
            if (phase == Phase.Idle)
            {
                if (Time.time - lastActionTime > comboWindow) comboStep = 0;
                return;
            }

            phaseT += Time.deltaTime;

            switch (phase)
            {
                case Phase.Windup:
                    if (phaseT >= current.windup)
                    {
                        phase = Phase.Active;
                        phaseT = 0f;
                        swordHitbox.Enable();
                    }
                    break;

                case Phase.Active:
                    if (phaseT >= current.active)
                    {
                        phase = Phase.Recovery;
                        phaseT = 0f;
                        swordHitbox.Disable();
                    }
                    break;

                case Phase.Recovery:
                    if (phaseT >= current.recovery)
                    {
                        phase = Phase.Idle;
                        phaseT = 0f;
                        current = null;
                        if (pending >= 0)
                        {
                            int k = pending;
                            pending = -1;
                            StartAttack(k);
                        }
                    }
                    break;
            }
        }

        public void TryStartLight()
        {
            if (phase == Phase.Idle) StartAttack(0);
            else if (phase != Phase.Windup && pending < 0) pending = 0;
        }

        public void TryStartHeavy()
        {
            if (phase == Phase.Idle) StartAttack(1);
            else if (pending < 0) pending = 1;
        }

        void StartAttack(int kind)
        {
            bool ground = owner != null && owner.Grounded;

            if (kind == 0)
            {
                if (ground)
                {
                    int step = comboStep;
                    current = step == 0 ? slash1 : (step == 1 ? slash2 : smash);
                    comboStep = (step + 1) % 3;
                    VisualId = step;
                }
                else
                {
                    current = airSwipe;
                    VisualId = 4;
                }
            }
            else
            {
                if (ground)
                {
                    current = launcher;
                    VisualId = 3;
                }
                else
                {
                    current = airPound;
                    VisualId = 5;
                    if (owner != null) owner.ApplyPound(13f);
                }
            }

            phase = Phase.Windup;
            phaseT = 0f;
            lastActionTime = Time.time;

            if (current.lunge > 0.01f && owner != null) owner.ApplyLunge(current.lunge);
            FaceLocked();
        }

        void FaceLocked()
        {
            if (owner == null) return;
            var t = owner.LockTarget;
            if (t != null)
            {
                Vector3 d = t.position - owner.transform.position;
                d.y = 0f;
                if (d.sqrMagnitude > 0.0001f)
                {
                    Quaternion q = Quaternion.LookRotation(d);
                    owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation, q, owner.rotationSpeed * Time.deltaTime * 3f);
                }
                return;
            }

            Demon nearest = FindNearestInFront(5f, 70f);
            if (nearest != null)
            {
                Vector3 d = nearest.transform.position - owner.transform.position;
                d.y = 0f;
                if (d.sqrMagnitude > 0.0001f)
                {
                    Quaternion q = Quaternion.LookRotation(d);
                    owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation, q, owner.rotationSpeed * Time.deltaTime * 3f);
                }
            }
        }

        Demon FindNearestInFront(float range, float maxAngle)
        {
            Demon[] all = UnityEngine.Object.FindObjectsByType<Demon>(FindObjectsSortMode.None);
            Demon best = null;
            float bestScore = float.MaxValue;
            foreach (var d in all)
            {
                if (d.Dead) continue;
                Vector3 to = d.transform.position - owner.transform.position;
                float dist = to.magnitude;
                if (dist > range || dist < 0.01f) continue;
                float ang = Vector3.Angle(owner.transform.forward, to.normalized);
                if (ang > maxAngle) continue;
                float score = dist + ang * 0.5f;
                if (score < bestScore) { bestScore = score; best = d; }
            }
            return best;
        }

        void FrameHit(Collider other)
        {
            if (current == null) return;

            var cc = other as CharacterController;
            if (cc == null) return;

            var target = cc.GetComponent<IDamageable>();
            if (target == null) return;

            var comp = target as Component;
            if (comp == null || ReferenceEquals(comp, owner)) return;

            Vector3 push = comp.transform.position - owner.transform.position;
            push.y = 0f;
            if (push.sqrMagnitude < 0.0001f) push = owner.transform.forward;
            push.Normalize();

            var info = new HitInfo
            {
                damage = current.damage,
                direction = push,
                knockback = current.knockback,
                launch = current.launch,
                styleTag = current.styleTag
            };

            target.ReceiveHit(info, comp.transform.position);

            ShowDamageNumber(comp.transform.position + Vector3.up * 1.6f);

            if (StyleScored != null) StyleScored(current.styleTag);
            if (HitLanded != null) HitLanded();

            HitStop.Play(current.hitstop);

            var cam = Camera.main != null ? Camera.main.GetComponent<PlayerCamera>() : null;
            if (cam != null) cam.Punch(current.bigHit ? 0.16f : 0.06f, current.bigHit);
        }

        void ShowDamageNumber(Vector3 worldPos)
        {
            Color color = Color.white;
            float size = 24f;

            if (current.styleTag == StyleTags.Finisher) { color = new Color(1f, 0.85f, 0.2f); size = 32f; }
            else if (current.styleTag == StyleTags.Launcher) { color = new Color(1f, 0.6f, 0.2f); size = 28f; }
            else if (current.styleTag == StyleTags.AerialHit) { color = new Color(0.3f, 0.9f, 1f); size = 22f; }
            else if (current.styleTag == StyleTags.Slam) { color = new Color(0.75f, 0.4f, 1f); size = 30f; }

            DamageNumber.Show(worldPos, Mathf.RoundToInt(current.damage).ToString(), color, size);
        }

        public void DodgeCancel()
        {
            pending = -1;
            if (phase != Phase.Idle)
            {
                phase = Phase.Idle;
                phaseT = 0f;
                current = null;
                if (swordHitbox != null) swordHitbox.Disable();
            }
        }

        public void TryShoot()
        {
            Vector3 pos = transform.position + transform.forward * 0.7f + Vector3.up * 1.35f + transform.right * 0.25f;
            Vector3 dir = transform.forward;
            var t = owner.LockTarget;
            if (t != null)
            {
                Vector3 toT = t.position - pos;
                if (toT.sqrMagnitude > 0.0001f) dir = toT.normalized;
            }

            var go = new GameObject("GoldenBolt");
            go.layer = gameObject.layer;
            var proj = go.AddComponent<GunProjectile>();
            proj.Launch(pos, dir, 8f);

            if (StyleScored != null) StyleScored(StyleTags.Gunshot);
            owner.visual?.ShootRecoil();
        }
    }
}
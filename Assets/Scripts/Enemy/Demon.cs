using System.Collections;
using UnityEngine;
using HnS.Core;
using HnS.Player;

namespace HnS.Enemy
{
    public class Demon : MonoBehaviour, IDamageable
    {
        public float maxHp = 90f;
        public float damage = 14f;
        public float moveSpeed = 3.4f;
        public float turnSpeed = 540f;
        public float aggroRange = 9f;
        public float loseRange = 17f;
        public float attackRange = 1.9f;
        public float telegraphTime = 0.55f;
        public float strikeTime = 0.22f;
        public float recoverTime = 0.7f;
        public float knockResist = 1f;
        public bool isBoss = false;

        public bool Dead { get; private set; }
        public float Hp { get; private set; }

        CharacterController cc;
        DemonVisual vfx;
        Hitbox strike;
        PlayerController player;
        Vector3 vel;
        float stateT;
        int airHits;

        enum AI { Idle, Telegraph, Strike, Recover, Chase, Hitstun, Airborne, Dead }
        AI ai = AI.Idle;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (cc == null) cc = gameObject.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0f, 1f, 0f);

            vfx = GetComponent<DemonVisual>();
            if (vfx == null) vfx = gameObject.AddComponent<DemonVisual>();

            Hp = maxHp;
            BuildStrike();
        }

        void Start()
        {
            player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null) ai = AI.Idle;
            vfx.ApplyBoss(isBoss);
        }

        void BuildStrike()
        {
            var go = new GameObject("StrikeHit");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.15f, 0.75f);
            go.layer = gameObject.layer;

            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.85f;

            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            strike = go.AddComponent<Hitbox>();
            strike.onHit = OnStrikeHit;
            go.SetActive(false);
        }

        void OnStrikeHit(Collider other)
        {
            var cc2 = other as CharacterController;
            if (cc2 == null) return;

            var pc = cc2.GetComponent<PlayerController>();
            if (pc == null) return;

            Vector3 dir = pc.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
            dir.Normalize();

            var info = new HitInfo
            {
                damage = damage,
                direction = dir,
                knockback = 5f,
                launch = 0f,
                styleTag = ""
            };
            pc.ReceiveHit(info, transform.position + Vector3.up * 1f);
        }

        void Update()
        {
            if (ai == AI.Dead) return;

            float dt = Time.deltaTime;

            if (player == null)
            {
                player = Object.FindFirstObjectByType<PlayerController>();
                if (player == null) return;
            }

            switch (ai)
            {
                case AI.Idle:
                    if (InRange(aggroRange)) { ai = AI.Chase; }
                    break;

                case AI.Chase:
                    float distC = HDist;
                    if (distC <= attackRange)
                    {
                        EnterState(AI.Telegraph);
                    }
                    else if (distC > loseRange)
                    {
                        EnterState(AI.Idle);
                    }
                    break;

                case AI.Telegraph:
                    stateT += dt;
                    FacePlayer();
                    if (stateT >= telegraphTime)
                    {
                        EnterState(AI.Strike);
                        if (strike != null) strike.Enable();
                        vel += transform.forward * 3.5f;
                    }
                    break;

                case AI.Strike:
                    stateT += dt;
                    if (stateT >= strikeTime)
                    {
                        if (strike != null) strike.Disable();
                        EnterState(HDist <= attackRange + 0.3f ? AI.Recover : AI.Chase);
                    }
                    break;

                case AI.Recover:
                    stateT += dt;
                    if (stateT >= recoverTime)
                    {
                        EnterState(HDist <= attackRange ? AI.Telegraph : AI.Chase);
                    }
                    break;

                case AI.Hitstun:
                    stateT += dt;
                    if (stateT >= 0.45f)
                    {
                        EnterState(HDist <= attackRange ? AI.Telegraph : AI.Chase);
                    }
                    break;

                case AI.Airborne:
                    stateT += dt;
                    if (Grounded() && stateT > 0.2f)
                    {
                        vel.y = 0f;
                        EnterState(HDist <= attackRange ? AI.Telegraph : AI.Chase);
                    }
                    break;
            }

            Vector3 moveDelta;
            if (ai == AI.Chase)
            {
                Vector3 toP = player.transform.position - transform.position;
                toP.y = 0f;
                if (toP.sqrMagnitude > 0.0001f) toP.Normalize();
                FacePlayer();
                vel.x = toP.x * moveSpeed;
                vel.z = toP.z * moveSpeed;
            }
            else if (ai == AI.Telegraph || ai == AI.Recover || ai == AI.Idle)
            {
                vel.x = Mathf.Lerp(vel.x, 0f, dt * 8f);
                vel.z = Mathf.Lerp(vel.z, 0f, dt * 8f);
            }

            if (ai == AI.Idle) vel.x = Mathf.Lerp(vel.x, 0f, dt * 8f);

            if (!Grounded())
            {
                vel.y -= 18f * dt;
                if (vel.y < -30f) vel.y = -30f;
            }
            else if (vel.y < 0f)
            {
                vel.y = -2f;
            }

            moveDelta = vel * dt;

            if (cc != null && cc.enabled)
            {
                cc.Move(moveDelta);
            }

            if (ai == AI.Airborne)
            {
                vfx.JugglePose(stateT);
            }
        }

        void EnterState(AI next)
        {
            ai = next;
            stateT = 0f;
        }

        float HDist
        {
            get
            {
                Vector3 d = player.transform.position - transform.position;
                d.y = 0f;
                return d.magnitude;
            }
        }

        bool InRange(float r)
        {
            return player != null && HDist <= r;
        }

        bool Grounded()
        {
            return cc != null && cc.isGrounded;
        }

        void FacePlayer()
        {
            if (player == null) return;
            Vector3 d = player.transform.position - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude < 0.0001f) return;
            Quaternion q = Quaternion.LookRotation(d.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, turnSpeed * Time.deltaTime);
        }

        public void ReceiveHit(HitInfo hit, Vector3 hitPoint)
        {
            if (Dead || ai == AI.Dead) return;

            Hp -= hit.damage;
            vfx.Flash();

            if (ai == AI.Airborne)
            {
                vel = hit.direction * hit.knockback * 0.7f;
                vel.y = Mathf.Max(4f, vel.y + 2.5f);
                airHits++;
                HnS.Game.StyleMeter.Instance?.OnAirHit(airHits);
            }
            else
            {
                if (hit.launch > 0f && Grounded())
                {
                    vel = hit.direction * hit.knockback;
                    vel.y = hit.launch;
                    airHits = 0;
                    EnterState(AI.Airborne);
                }
                else
                {
                    vel += hit.direction * hit.knockback * (1f / Mathf.Max(0.1f, knockResist));
                    if (vel.sqrMagnitude > 81f)
                    {
                        vel = vel.normalized * 9f;
                    }
                    EnterState(AI.Hitstun);
                }
            }

            if (Hp <= 0f)
            {
                Die();
            }
        }

        void Die()
        {
            Dead = true;
            ai = AI.Dead;
            if (strike != null) strike.Disable();
            if (cc != null) cc.enabled = false;

            vfx.Die();

            HnS.Game.StyleMeter.Instance?.AddKill(this);
            HnS.Game.WaveManager.Instance?.OnEnemyKilled(this);
        }
    }
}
using UnityEngine;
using HnS.CameraControl;
using HnS.Core;
using HnS.Game;
using HnS.UI;

namespace HnS.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        public CharacterController cc;
        public PlayerCombat combat;
        public LockOn lockOn;
        public PlayerVisual visual;

        public float moveSpeed = 6f;
        public float jumpHeight = 2.2f;
        public float gravity = 22f;
        public float dodgeSpeed = 13f;
        public float dodgeTime = 0.18f;
        public float rotationSpeed = 1080f;
        public float maxHp = 100f;

        public float Hp { get; private set; }
        public bool Dead { get; private set; }
        public bool Invincible { get; private set; }
        public Vector3 Velocity { get; private set; }
        public bool Grounded { get { return cc != null && cc.isGrounded; } }
        public Vector3 MoveInput { get; private set; }

        public Transform LockTarget { get { return lockOn != null ? lockOn.Target : null; } }

        float verticalVel;
        float dodgeTimer;
        Vector3 dodgeDir;
        Vector3 smoothHoriz;
        Vector3 lungeVel;
        float lungeTimer;

        bool Dodging { get { return dodgeTimer > 0f; } }

        public event System.Action Evaded;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (cc == null) cc = gameObject.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            combat = GetComponent<PlayerCombat>();
            lockOn = GetComponent<LockOn>();
            visual = GetComponent<PlayerVisual>();

            Hp = maxHp;
            HitStop.Register(this);
        }

        void Update()
        {
            if (Dead)
            {
                if (cc != null && cc.enabled)
                {
                    verticalVel -= gravity * Time.deltaTime;
                    cc.Move(Vector3.up * verticalVel * Time.deltaTime);
                }
                return;
            }

            var input = InputHub.GetFrame();

            if (lockOn != null && input.lockToggle) lockOn.Toggle();
            if (lockOn != null) lockOn.Refresh();

            if (combat != null)
            {
                if (input.attack) combat.TryStartLight();
                if (input.heavy) combat.TryStartHeavy();
                if (input.shoot) combat.TryShoot();
            }

            if (input.jump && Grounded && !Dodging)
                verticalVel = Mathf.Sqrt(jumpHeight * 2f * gravity);

            if (input.dodge && !Dodging) BeginDodge(input.move);

            MoveInput = input.move;

            float control = 1f;
            if (Dodging) control = 0f;
            else if (combat != null && combat.IsAttacking)
            {
                control = (combat.VisualId == 5) ? 0f : 0.35f;
            }
            else if (combat != null && combat.InRecovery) control = 0.6f;

            if (!Grounded) control *= 0.72f;

            MoveBody(input.move, control);
            FaceLogic(input.move);
        }

        Vector3 ComputeWish(Vector2 move)
        {
            Vector3 wish = Vector3.zero;
            if (LockTarget != null)
            {
                Vector3 fwd = LockTarget.position - transform.position;
                fwd.y = 0f;
                if (fwd.sqrMagnitude > 0.0001f) fwd.Normalize();
                else fwd = transform.forward;
                Vector3 right = Vector3.Cross(Vector3.up, fwd);
                wish = fwd * move.y + right * move.x;
            }
            else
            {
                Transform cam = Camera.main != null ? Camera.main.transform : transform;
                Vector3 camF = cam.forward;
                camF.y = 0f;
                if (camF.sqrMagnitude < 0.0001f) camF = transform.forward;
                camF.Normalize();
                Vector3 camR = cam.right;
                camR.y = 0f;
                camR.Normalize();
                wish = camF * move.y + camR * move.x;
            }
            if (wish.sqrMagnitude > 1f) wish.Normalize();
            return wish;
        }

        void MoveBody(Vector2 move, float control)
        {
            float dt = Time.deltaTime;
            Vector3 disp = Vector3.zero;

            if (Dodging)
            {
                dodgeTimer -= dt;
                if (dodgeTimer <= 0f)
                {
                    Invincible = false;
                    dodgeTimer = 0f;
                }
                disp = dodgeDir * dodgeSpeed * dt * 1.6f;
                if (lungeTimer > 0f)
                {
                    lungeTimer -= dt;
                    disp += lungeVel * dt;
                }
            }
            else
            {
                Vector3 wish = ComputeWish(move);
                Vector3 targetHoriz = wish * moveSpeed * control;
                smoothHoriz = Vector3.Lerp(smoothHoriz, targetHoriz, dt * 12f);
                disp = smoothHoriz * dt;

                if (lungeTimer > 0f)
                {
                    lungeTimer -= dt;
                    disp += lungeVel * dt;
                }
            }

            if (Grounded && verticalVel < 0f) verticalVel = -2f;
            verticalVel -= gravity * Time.deltaTime;
            if (verticalVel < -35f) verticalVel = -35f;

            disp.y = verticalVel * Time.deltaTime;

            if (cc != null && cc.enabled)
            {
                cc.Move(disp);
                Velocity = smoothHoriz + Vector3.up * verticalVel;
            }
        }

        public void BeginDodge(Vector2 move)
        {
            Vector3 dir;
            if (move.sqrMagnitude > 0.1f)
            {
                dir = ComputeWish(move);
            }
            else if (LockTarget != null)
            {
                dir = LockTarget.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
                else dir = transform.forward;
            }
            else
            {
                dir = -transform.forward;
            }

            dodgeDir = dir;
            dodgeTimer = dodgeTime;
            Invincible = true;
            smoothHoriz = Vector3.zero;

            if (combat != null) combat.DodgeCancel();

            var cam = Camera.main != null ? Camera.main.GetComponent<PlayerCamera>() : null;
            if (cam != null) cam.DodgeKick();

            visual?.DoDodge(true);
        }

        public void ApplyLunge(float amount)
        {
            lungeVel = transform.forward * amount;
            lungeTimer = 0.12f;
        }

        public void ApplyPound(float speed)
        {
            verticalVel = -speed;
            smoothHoriz = Vector3.zero;
        }

        void FaceLogic(Vector2 move)
        {
            if (combat != null && combat.IsAttacking) return;

            if (LockTarget != null)
            {
                if (move.sqrMagnitude > 0.05f)
                {
                    Vector3 dir = ComputeWish(move);
                    FaceDir(dir);
                }
                else
                {
                    Vector3 toT = LockTarget.position - transform.position;
                    toT.y = 0f;
                    if (toT.sqrMagnitude > 0.0001f) FaceDir(toT.normalized);
                }
                return;
            }

            if (move.sqrMagnitude > 0.05f)
            {
                FaceDir(ComputeWish(move));
            }
        }

        void FaceDir(Vector3 dir)
        {
            if (dir.sqrMagnitude < 0.0001f) return;
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }

        public void ReceiveHit(HitInfo hit, Vector3 hitPoint)
        {
            if (Dead) return;

            if (Invincible)
            {
                if (Evaded != null) Evaded();
                return;
            }

            StyleMeter.Instance?.OnPlayerHit();
            Hp -= hit.damage;
            if (Hp <= 0f) Hp = 0f;
            UIManager.Instance?.SetPlayerHealth(Hp / maxHp);
            UIManager.Instance?.RedFlash();
            visual?.HurtFlash();

            if (Hp <= 0f) Die();
        }

        void Die()
        {
            Dead = true;
            Invincible = false;
            visual?.Die();
            WaveManager.Instance?.OnPlayerDied();
        }

        public void Heal(float fraction)
        {
            if (Dead) return;
            Hp = Mathf.Min(maxHp, Hp + maxHp * Mathf.Max(0f, fraction));
            UIManager.Instance?.SetPlayerHealth(Hp / maxHp);
        }
    }
}
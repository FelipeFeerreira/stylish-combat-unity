using UnityEngine;
using HnS.Core;

namespace HnS.CameraControl
{
    public class PlayerCamera : MonoBehaviour
    {
        public Transform target;
        public LockOn lockOn;

        public float distance = 4.4f;
        public float pitchMin = -35f;
        public float pitchMax = 78f;
        public float mouseSensX = 0.22f;
        public float mouseSensY = 0.16f;
        public float gamepadSens = 55f;
        public float baseFov = 60f;

        float yaw;
        float pitch;
        float currentDist;
        float fovBoost;
        float punch;
        float punchPhase;

        void Start()
        {
            if (target != null)
            {
                Vector3 dir = target.position + Vector3.up * 1.4f - transform.position;
                yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                pitch = 12f;
            }
            currentDist = distance;
        }

        public void Punch(float amount, bool big)
        {
            punch = Mathf.Max(punch, amount);
            fovBoost = Mathf.Max(fovBoost, big ? 12f : 6f);
        }

        public void DodgeKick()
        {
            fovBoost = Mathf.Max(fovBoost, 14f);
        }

        void LateUpdate()
        {
            if (target == null) return;

            float dt = Mathf.Max(0f, Time.deltaTime);
            bool locked = lockOn != null && lockOn.Active && lockOn.Target != null;

            var input = InputHub.GetFrame();
            Vector2 look = input.look;
            bool mouse = Mathf.Abs(look.x) >= 2f || Mathf.Abs(look.y) >= 2f;

            if (!locked)
            {
                if (!mouse)
                {
                    yaw += look.x * gamepadSens * dt;
                    pitch += look.y * gamepadSens * dt;
                    pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
                }
                else
                {
                    yaw += look.x * mouseSensX;
                    pitch = Mathf.Clamp(pitch - look.y * mouseSensY, pitchMin, pitchMax);
                }
            }

            Vector3 pivot = target.position + Vector3.up * 1.4f;
            Vector3 lookAt;
            float targetDist = distance;

            if (locked)
            {
                Vector3 toE = lockOn.Target.position - target.position;
                toE.y = 0f;
                float h = Mathf.Max(0.35f, toE.magnitude);

                float targetYaw = Mathf.Atan2(toE.x, toE.z) * Mathf.Rad2Deg;
                yaw = Mathf.LerpAngle(yaw, targetYaw, 10f * dt);

                float dy = lockOn.Target.position.y - target.position.y;
                pitch = Mathf.Lerp(pitch, Mathf.Atan2(dy, h) * Mathf.Rad2Deg, 6f * dt);

                targetDist = distance * Mathf.Clamp(1f + 0.45f * (h / 8f), 1f, 1.7f);
                lookAt = (target.position + lockOn.Target.position) * 0.5f + Vector3.up * 1f;
            }
            else
            {
                lookAt = target.position + Vector3.up * 1.2f;
            }

            currentDist = Mathf.Lerp(currentDist, targetDist, 8f * dt);

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 camPos = pivot + rot * Vector3.back * currentDist;
            camPos = Collide(pivot, camPos);

            if (punch > 0.001f)
            {
                punchPhase += dt * 90f;
                rot *= Quaternion.Euler(Mathf.Sin(punchPhase) * punch * 18f, 0f, 0f);
                punch = Mathf.Max(0f, punch - dt * 1.2f);
            }

            transform.position = camPos;
            transform.rotation = rot;

            var c = GetComponent<Camera>();
            if (c != null)
            {
                c.fieldOfView = Mathf.Lerp(c.fieldOfView, baseFov + fovBoost, 10f * dt);
                fovBoost = Mathf.Max(0f, fovBoost - dt * 26f);
            }
        }

        Vector3 Collide(Vector3 from, Vector3 to)
        {
            Vector3 dir = to - from;
            float d = dir.magnitude;
            if (d < 0.02f) return to;

            int mask = ~(1 << Layers.Player);
            if (Physics.Raycast(from, dir.normalized, out RaycastHit h, d, mask, QueryTriggerInteraction.Ignore))
            {
                return from + dir.normalized * Mathf.Max(h.distance - 0.22f, 0.25f);
            }
            return to;
        }
    }
}
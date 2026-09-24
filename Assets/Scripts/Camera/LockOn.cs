using UnityEngine;
using HnS.Enemy;
using HnS.Player;
using HnS.UI;

namespace HnS.CameraControl
{
    public class LockOn : MonoBehaviour
    {
        public float range = 16f;
        public Transform Target { get; private set; }
        public bool Active { get { return Target != null; } }

        PlayerController owner;

        void Awake()
        {
            owner = GetComponent<PlayerController>();
        }

        public bool Toggle()
        {
            if (Target != null)
            {
                Target = null;
                UIManager.Instance?.SetLockReticle(false);
                return false;
            }

            Target = FindBest();
            UIManager.Instance?.SetLockReticle(Target != null);
            return Target != null;
        }

        public void Refresh()
        {
            if (Target == null) return;
            var d = Target.GetComponent<Demon>();
            if (d != null && d.Dead)
            {
                Target = FindBest();
                if (Target == null) UIManager.Instance?.SetLockReticle(false);
            }
        }

        Transform FindBest()
        {
            var demons = Object.FindObjectsByType<Demon>(FindObjectsSortMode.None);
            Transform best = null;
            float bestScore = float.MaxValue;

            foreach (var d in demons)
            {
                if (d.Dead) continue;
                Vector3 to = d.transform.position - owner.transform.position;
                float dist = to.magnitude;
                if (dist > range) continue;

                float ang = Vector3.Angle(owner.transform.forward, to.normalized);
                if (ang > 70f && dist > 2.5f) continue;

                float score = dist + ang * 0.5f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = d.transform;
                }
            }
            return best;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace HnS.Core
{
    public class Hitbox : MonoBehaviour
    {
        public System.Action<Collider> onHit;
        readonly HashSet<Collider> already = new HashSet<Collider>();

        public void Enable()
        {
            already.Clear();
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!already.Add(other)) return;
            if (onHit != null) onHit(other);
        }
    }
}
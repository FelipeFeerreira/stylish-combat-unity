using UnityEngine;

namespace HnS.Core
{
    public struct HitInfo
    {
        public float damage;
        public Vector3 direction;
        public float knockback;
        public float launch;
        public string styleTag;
    }

    public interface IDamageable
    {
        void ReceiveHit(HitInfo hit, Vector3 hitPoint);
    }

    public static class StyleTags
    {
        public const string Slash = "Slash";
        public const string Finisher = "Finisher";
        public const string Launcher = "Launcher";
        public const string AerialHit = "AerialHit";
        public const string Slam = "Slam";
        public const string Gunshot = "Gunshot";
        public const string Dodge = "Dodge";
        public const string Kill = "Kill";
    }

    public static class Layers
    {
        public const int PlayerIndex = 8;
        public const int EnemyIndex = 9;

        public static int Player
        {
            get
            {
                int id = LayerMask.NameToLayer("Player");
                return id < 0 ? PlayerIndex : id;
            }
        }

        public static int Enemy
        {
            get
            {
                int id = LayerMask.NameToLayer("Enemy");
                return id < 0 ? EnemyIndex : id;
            }
        }
    }
}
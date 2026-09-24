using System.Collections;
using UnityEngine;

namespace HnS.Core
{
    public static class HitStop
    {
        static MonoBehaviour host;
        static Coroutine active;

        public static void Register(MonoBehaviour m)
        {
            host = m;
            active = null;
        }

        public static void Play(float duration)
        {
            if (host == null || active != null) return;
            active = host.StartCoroutine(Run(Mathf.Max(0.005f, duration)));
        }

        static IEnumerator Run(float d)
        {
            Time.timeScale = 0.02f;
            yield return new WaitForSecondsRealtime(d);
            Time.timeScale = 1f;
            active = null;
        }
    }

    public static class Util
    {
        public static void SafeDestroy(Object o)
        {
            if (o == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(o);
                return;
            }
#endif
            UnityEngine.Object.Destroy(o);
        }
    }
}
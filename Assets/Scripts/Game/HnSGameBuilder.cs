using UnityEngine;
using UnityEngine.Rendering;
using HnS.CameraControl;
using HnS.Core;
using HnS.Player;
using HnS.UI;

namespace HnS.Game
{
    public static class HnSGameBuilder
    {
        public static bool UsingURP
        {
            get { return GraphicsSettings.currentRenderPipeline != null; }
        }

        public static GameObject BuildAll()
        {
            Physics.IgnoreLayerCollision(Layers.PlayerIndex, Layers.PlayerIndex, true);
            Physics.IgnoreLayerCollision(Layers.EnemyIndex, Layers.EnemyIndex, true);

            ConfigureEnvironment();

            GameObject player = BuildPlayer(new Vector3(0f, 0.1f, 0f));
            BuildGameManager(player);
            BuildArena();
            BuildCamera(player);
            return player;
        }

        static void ConfigureEnvironment()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.24f, 0.31f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.016f;
            RenderSettings.fogColor = new Color(0.1f, 0.1f, 0.14f);

            Light sun = null;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type == LightType.Directional) { sun = l; break; }
            }

            if (sun == null)
            {
                var go = new GameObject("Sun");
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.name = "Sun";
            sun.color = new Color(1f, 0.82f, 0.6f);
            sun.intensity = UsingURP ? 2f : 0.9f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static Shader LitShader()
        {
            var s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            return s;
        }

        static Material Mat(Color c)
        {
            var s = LitShader();
            if (s == null) return null;
            var m = new Material(s);
            MColor.Set(m, c);
            return m;
        }

        static void SetMat(Renderer r, Color c)
        {
            var m = Mat(c);
            if (m != null) r.sharedMaterial = m;
        }

        static void BuildArena()
        {
            var arena = new GameObject("Arena");

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(arena.transform);
            floor.transform.localScale = new Vector3(3.6f, 1f, 3.6f);
            SetMat(floor.GetComponent<Renderer>(), new Color(0.3f, 0.32f, 0.38f));

            var wallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallN.name = "WallN";
            wallN.transform.SetParent(arena.transform);
            wallN.transform.position = new Vector3(0f, 2f, -18f);
            wallN.transform.localScale = new Vector3(36f, 4f, 0.6f);
            SetMat(wallN.GetComponent<Renderer>(), new Color(0.24f, 0.26f, 0.31f));

            var wallS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallS.name = "WallS";
            wallS.transform.SetParent(arena.transform);
            wallS.transform.position = new Vector3(0f, 2f, 18f);
            wallS.transform.localScale = new Vector3(36f, 4f, 0.6f);
            SetMat(wallS.GetComponent<Renderer>(), new Color(0.24f, 0.26f, 0.31f));

            var wallE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallE.name = "WallE";
            wallE.transform.SetParent(arena.transform);
            wallE.transform.position = new Vector3(-18f, 2f, 0f);
            wallE.transform.localScale = new Vector3(0.6f, 4f, 36f);
            SetMat(wallE.GetComponent<Renderer>(), new Color(0.24f, 0.26f, 0.31f));

            var wallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallW.name = "WallW";
            wallW.transform.SetParent(arena.transform);
            wallW.transform.position = new Vector3(18f, 2f, 0f);
            wallW.transform.localScale = new Vector3(0.6f, 4f, 36f);
            SetMat(wallW.GetComponent<Renderer>(), new Color(0.24f, 0.26f, 0.31f));

            Vector2[] slots = {
                new Vector2(-13f, -13f), new Vector2(13f, -13f),
                new Vector2(-13f, 13f), new Vector2(13f, 13f),
                new Vector2(0f, -9f), new Vector2(0f, 9f),
                new Vector2(-9f, 0f), new Vector2(9f, 0f),
            };

            for (int i = 0; i < slots.Length; i++)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "Pillar" + i;
                pillar.transform.SetParent(arena.transform);
                pillar.transform.position = new Vector3(slots[i].x, 2f, slots[i].y);
                pillar.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
                SetMat(pillar.GetComponent<Renderer>(), new Color(0.48f, 0.46f, 0.4f));

                bool torch = i < 6;
                if (torch)
                {
                    var torchGo = new GameObject("Torch" + i);
                    torchGo.transform.SetParent(pillar.transform);
                    torchGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);

                    var l = torchGo.AddComponent<Light>();
                    l.type = LightType.Point;
                    l.color = new Color(1f, 0.55f, 0.25f);
                    l.intensity = UsingURP ? 40f : 5f;
                    l.range = 10f;
                    l.shadows = LightShadows.None;

                    var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    flame.name = "Flame";
                    flame.transform.SetParent(torchGo.transform);
                    flame.transform.localPosition = Vector3.zero;
                    flame.transform.localScale = Vector3.one * 0.22f;
                    var fr = flame.GetComponent<MeshRenderer>();
                    SetMat(fr, new Color(1f, 0.55f, 0.2f));
                    fr.material.EnableKeyword("_EMISSION");
                    fr.material.SetColor("_EmissionColor", new Color(1f, 0.55f, 0.2f) * (UsingURP ? 1f : 3f));
                    Object.DestroyImmediate(flame.GetComponent<Collider>());
                }
            }
        }

        static GameObject BuildPlayer(Vector3 pos)
        {
            var go = new GameObject("Templario");
            go.layer = Layers.Player;

            try
            {
                go.tag = "Player";
            }
            catch
            {
                // tag nao disponivel; ok
            }

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            go.AddComponent<PlayerController>();
            go.AddComponent<LockOn>();
            go.AddComponent<PlayerCombat>();
            var vis = go.AddComponent<PlayerVisual>();
            vis.BuildIfNeeded();

            go.transform.position = pos;
            return go;
        }

        static void BuildGameManager(GameObject player)
        {
            var gm = new GameObject("GameManager");

            var wm = gm.AddComponent<WaveManager>();
            wm.player = player.GetComponent<PlayerController>();

            gm.AddComponent<StyleMeter>();

            var ui = gm.AddComponent<UIManager>();
            ui.lockOnTarget = player.GetComponent<LockOn>();
        }

        static void BuildCamera(GameObject player)
        {
            Camera cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            cam.gameObject.name = "Main Camera";
            cam.gameObject.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.14f);

            var ctrl = cam.gameObject.GetComponent<PlayerCamera>();
            if (ctrl == null) ctrl = cam.gameObject.AddComponent<PlayerCamera>();
            ctrl.target = player.transform;
            ctrl.lockOn = player.GetComponent<LockOn>();
        }
    }

    public static class HnSBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBuild()
        {
            if (Object.FindFirstObjectByType<WaveManager>() != null) return;
            if (Object.FindFirstObjectByType<PlayerController>() != null) return;
            HnSGameBuilder.BuildAll();
        }
    }
}
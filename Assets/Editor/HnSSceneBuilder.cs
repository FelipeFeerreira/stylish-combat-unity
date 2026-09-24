using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HnS.Game;

namespace HnS.EditorTools
{
    public static class HnSSceneBuilder
    {
        [MenuItem("Tools/Hack n Slash/Build Demo Scene")]
        public static void Build()
        {
            EnsureLayers();
            EnsureTag("Player");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "HackNSlashDemo";

            HnSGameBuilder.BuildAll();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            if (!EditorSceneManager.SaveScene(scene, "Assets/Scenes/Demo.unity"))
            {
                Debug.LogError("Falha ao salvar a cena.");
                return;
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Demo.unity", true),
            };

            EditorUtility.DisplayDialog(
                "Hack'n Slash",
                "Cena demo criada em Assets/Scenes/Demo.unity.\n\n" +
                "Pressione Play para jogar.\n\n" +
                "Controles (Mouse/Teclado):\n" +
                "WASD mover, Mouse camera, Click Esq atacar,\n" +
                "Click Dir lancar, Shift esquiva, Espaco pular,\n" +
                "E pistola, Tab lock-on.\n\n" +
                "Gamepad: Stick mover, A atacar, Y lancar,\nB esquiva, X pular, LB tiro, Stick-click lock-on.",
                "OK");
        }

        static void EnsureLayers()
        {
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0) return;

            var so = new SerializedObject(asset[0]);
            var layers = so.FindProperty("layers");

            SetLayerName(layers, HnS.Core.Layers.PlayerIndex, "Player");
            SetLayerName(layers, HnS.Core.Layers.EnemyIndex, "Enemy");

            so.ApplyModifiedProperties();
        }

        static void SetLayerName(SerializedProperty layers, int index, string name)
        {
            if (index < 0 || index >= layers.arraySize) return;
            var el = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(el.stringValue.Trim()))
            {
                el.stringValue = name;
            }
        }

        static void EnsureTag(string tag)
        {
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0) return;

            var so = new SerializedObject(asset[0]);
            var tags = so.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            }
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedProperties();
        }
    }
}
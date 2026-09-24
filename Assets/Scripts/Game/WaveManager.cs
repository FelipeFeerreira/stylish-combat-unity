using System.Collections;
using UnityEngine;
using HnS.Core;
using HnS.Enemy;
using HnS.Player;
using HnS.UI;
using UnityEngine.InputSystem;

namespace HnS.Game
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance;

        public PlayerController player;

        int wave;
        int alive;
        bool over;

        void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        void Start()
        {
            player = Object.FindFirstObjectByType<PlayerController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            StartCoroutine(Loop());
        }

        public void OnEnemyKilled(Demon d)
        {
            alive--;
        }

        public void OnPlayerDied()
        {
            if (over) return;
            over = true;
            UIManager.Instance?.GameOver(false);
            StartCoroutine(WaitRestart());
        }

        IEnumerator Loop()
        {
            yield return new WaitForSeconds(1.4f);

            UIManager.Instance?.ShowHint(
                "[WASD] Mover    [Mouse] Camera\n" +
                "Click Esq. Atacar    Click Dir. Lancar (juggle)\n" +
                "[Shift] Esquiva    [Espaco] Pular\n" +
                "[E] Pistola    [Tab] Lock-on\n---\nGamepad: Stick mover | A atacar | Y lancar | B esquiva | X pular | LB tiro | Stick-click lock-on",
                11f);

            wave = 0;
            while (!over && wave < 3)
            {
                wave++;
                bool bossWave = wave == 3;
                int count = bossWave ? 3 : (wave == 1 ? 3 : 5);

                UIManager.Instance?.ShowCenter(
                    bossWave ? "ONDA 3 - O MAIOR DEMAO SE APROXIMA!" : ("ONDA " + wave + " - DEMAOS SE APROXIMAM!"),
                    3.2f);

                alive = 0;
                for (int i = 0; i < count; i++)
                {
                    if (over) break;
                    bool isBoss = bossWave && i == count - 1;
                    SpawnDemon(i, count, isBoss);
                    alive++;
                    yield return new WaitForSeconds(0.75f);
                }

                while (alive > 0 && !over)
                {
                    yield return new WaitForSeconds(0.2f);
                }

                if (over) break;

                if (player != null) player.Heal(0.25f);
                UIManager.Instance?.ShowCenter(wave == 3 ? "VITORIA! A arena esta limpa." : ("Onda " + wave + " limpa!  +Recuperacao"), 2.8f);
            }

            if (!over)
            {
                over = true;
                UIManager.Instance?.GameOver(true);
                StartCoroutine(WaitRestart());
            }
        }

        void SpawnDemon(int index, int total, bool isBoss)
        {
            float angle = (index / (float)total) * Mathf.PI * 2f + index * 0.4f;
            float radius = 11f;
            Vector3 pos = new Vector3(Mathf.Sin(angle) * radius, 0.05f, Mathf.Cos(angle) * radius);

            var go = new GameObject(isBoss ? "MaiorDemonio" : "Demonio");
            go.layer = Layers.Enemy;
            go.transform.position = pos;

            var cc = go.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0f, 1f, 0f);

            var demon = go.AddComponent<Demon>();
            if (isBoss)
            {
                demon.maxHp = 320f;
                demon.damage = 24f;
                demon.moveSpeed = 3.1f;
                demon.knockResist = 0.45f;
                demon.telegraphTime = 0.65f;
                demon.attackRange = 2.3f;
                demon.isBoss = true;
                go.transform.localScale = Vector3.one * 1.5f;
            }
        }

        IEnumerator WaitRestart()
        {
            while (!over)
            {
                yield return null;
            }

            bool restarted = false;
            while (!restarted)
            {
                yield return null;
                try
                {
                    if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) restarted = true;
                    if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame) restarted = true;
                }
                catch (System.InvalidOperationException)
                {
                    // input system not active
                }
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
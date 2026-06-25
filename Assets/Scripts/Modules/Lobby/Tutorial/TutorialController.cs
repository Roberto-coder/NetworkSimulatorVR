using System.Collections;
using UnityEngine;

namespace Modules.Lobby.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        public TutorialState currentState;

        public GuideNPC robot; // tu script de movimiento
    
        public FloatingEffect floating;
        public WorldSpaceUI ui;
        public AudioSource audioSource;

        [Header("Player")]
        public Transform player;
        public Transform playerSpawn;

    
        void Start()
        {
            currentState = TutorialState.SpawnPlayer;
            StartCoroutine(RunTutorial());
        }

        IEnumerator RunTutorial()
        {
            while (currentState != TutorialState.End)
            {
                switch (currentState)
                {
                    case TutorialState.SpawnPlayer:
                        SpawnPlayer();
                        currentState = TutorialState.IntroDialog;
                        break;

                    case TutorialState.IntroDialog:
                        yield return robot.LookAtPlayer();
                        yield return PlayDialog("Bienvenido...");
                        currentState = TutorialState.MoveToPanel;
                        break;

                    case TutorialState.MoveToPanel:
                        yield return robot.MoveToPoint(0);
                        currentState = TutorialState.ExplainPanel;
                        break;

                    case TutorialState.ExplainPanel:
                        yield return robot.LookAtPlayer();
                        yield return PlayDialog("Este es el panel...");
                        currentState = TutorialState.MoveToZone2;
                        break;

                    case TutorialState.MoveToZone2:
                        yield return robot.MoveToPoint(1);
                        currentState = TutorialState.ExplainZone2;
                        break;

                    case TutorialState.ExplainZone2:
                        yield return robot.LookAtPlayer();
                        yield return PlayDialog("Aquí puedes...");
                        currentState = TutorialState.FinalDialog;
                        break;

                    case TutorialState.FinalDialog:
                        yield return PlayDialog("Eso es todo!");
                        yield return robot.MoveToPoint(2);
                        currentState = TutorialState.End;
                        break;
                }

                yield return null;
            }

            EndTutorial();
        }

        void SpawnPlayer()
        {
            player.position = playerSpawn.position;
            player.rotation = playerSpawn.rotation;
        }

        IEnumerator PlayDialog(string text, AudioClip clip = null)
        {
            ui.Show(text);

            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                yield return new WaitForSeconds(clip.length);
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

            ui.Hide();
        }

        void EndTutorial()
        {
            Debug.Log("Tutorial terminado");
        }
    }
}
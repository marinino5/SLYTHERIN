using UnityEngine;
using Slytherin.Managers;

namespace Slytherin.Items
{
    [RequireComponent(typeof(Collider))]
    public class FinalObjective : MonoBehaviour
    {
        [SerializeField] private int requiredScore = 100;
        [SerializeField] private string winMessage = "¡Entrega completada!";
        [SerializeField] private string missingScoreMessage = "Necesitas más relojes antes de entregar las mantas.";

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (GameManager.Instance == null)
            {
                Debug.Log("No hay GameManager en la escena.");
                return;
            }

            if (GameManager.Instance.Score < requiredScore)
            {
                Debug.Log(missingScoreMessage + " Puntos actuales: " + GameManager.Instance.Score + "/" + requiredScore);
                return;
            }

            Debug.Log("VICTORIA: " + winMessage);
            GameManager.Instance.WinLevel(winMessage);
        }
    }
}
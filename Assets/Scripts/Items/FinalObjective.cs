using UnityEngine;
using Slytherin.Managers;

namespace Slytherin.Items
{
    /// <summary>
    /// BULTO DE MANTAS — objetivo final del nivel.
    /// Cuando el jugador entra al trigger, dispara la victoria.
    /// Colocar este script en la "Casa #4" o en un trigger frente a su puerta.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FinalObjective : MonoBehaviour
    {
        [SerializeField] private bool requireAllCollectibles = false;
        [SerializeField] private string winMessage = "¡Entrega completada!";

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (requireAllCollectibles && !AllCollected()) return;

            GameManager.Instance?.WinLevel(winMessage);
        }

        private bool AllCollected()
        {
            // Si quedan coleccionables en la escena, todavía no se completa
            return Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length == 0;
        }
    }
}

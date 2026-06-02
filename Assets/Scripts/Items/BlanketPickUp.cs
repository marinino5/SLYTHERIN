using UnityEngine;
using Slytherin.Player;

namespace Slytherin.Items
{
    public class BlanketPickup : MonoBehaviour
    {
        [SerializeField] private GameObject blanketOnPlayer;
        [SerializeField] private KeyCode pickupKey = KeyCode.E;

        private bool playerInRange;
        private bool alreadyPickedUp;
        private PlayerController playerController;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange = true;
            playerController = other.GetComponent<PlayerController>();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange = false;
        }

        private void Update()
        {
            if (alreadyPickedUp) return;

            if (playerInRange && Input.GetKeyDown(pickupKey))
            {
                PickUp();
            }
        }

        private void PickUp()
        {
            alreadyPickedUp = true;

            if (blanketOnPlayer != null)
            {
                blanketOnPlayer.SetActive(true);
            }

            if (playerController != null)
            {
                playerController.SetCarrying(true);
            }

            Debug.Log("Bulto de mantas recogido");

            gameObject.SetActive(false);
        }
    }
}
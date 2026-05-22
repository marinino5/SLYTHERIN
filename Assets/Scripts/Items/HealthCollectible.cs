using UnityEngine;
using Slytherin.Player;

namespace Slytherin.Items
{
    /// <summary>
    /// CARAMELO DE LIMÓN — restaura 1 vida al jugador.
    /// Usar el prefab Dulce.fbx.
    /// </summary>
    public class HealthCollectible : Collectible
    {
        [SerializeField] private int healAmount = 1;

        protected override void OnCollected(Collider player)
        {
            var hp = player.GetComponent<PlayerHealth>();
            if (hp != null) hp.Heal(healAmount);
        }
    }
}

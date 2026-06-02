using UnityEngine;
using Slytherin.Managers;

namespace Slytherin.Items
{
    /// <summary>
    /// RELOJ DE ORO — suma puntos al jugador.
    /// Usar el prefab Reloj.fbx.
    /// </summary>
    public class ScoreCollectible : Collectible
    {
        [SerializeField] private int scoreAmount = 25;

        protected override void OnCollected(Collider player)
        {
            GameManager.Instance?.AddScore(scoreAmount);
        }
    }
}

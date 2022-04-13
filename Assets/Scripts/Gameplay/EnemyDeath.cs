using System.Collections;
using Platformer.Core;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    ///     Fired when the health component on an enemy has a hitpoint value of  0.
    /// </summary>
    /// <typeparam name="EnemyDeath"></typeparam>
    public class EnemyDeath : Simulation.Event<EnemyDeath>
    {
        public EnemyController enemy;

        public override void Execute()
        {
            enemy.path = null;
            enemy.mover = null;
            enemy._collider.enabled = false;
            if (enemy._audio && enemy.ouch)
                enemy._audio.PlayOneShot(enemy.ouch);
            enemy.StartCoroutine(FallCoroutine());
        }

        private IEnumerator FallCoroutine()
        {
            while (enemy.transform.position.y > -100)
            {
                enemy._rigidbody.velocity = enemy._rigidbody.velocity + Vector2.down;
                yield return null;
            }

            enemy.gameObject.SetActive(false);
        }
    }
}
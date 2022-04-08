using System.Collections;
using Platformer.Core;

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
            enemy._collider.enabled = false;
            if (enemy._audio && enemy.ouch)
                enemy._audio.PlayOneShot(enemy.ouch);
            enemy.StartCoroutine(FallCoroutine());
        }

        private IEnumerator FallCoroutine()
        {
            while (enemy.transform.position.y > -100)
                yield return null;
            enemy.gameObject.SetActive(false);
        }
    }
}
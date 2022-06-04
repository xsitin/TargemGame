using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    ///     Fired when the health component on an enemy has a hitpoint value of  0.
    /// </summary>
    /// <typeparam name="EnemyDeath"></typeparam>
    public class EnemyDeath : Simulation.Event<EnemyDeath>
    {
        public BaseEnemy enemy;
        private static readonly int die = Animator.StringToHash("Die");

        public override void Execute()
        {
            enemy.GetComponent<Collider2D>().enabled = false;
            if (enemy._audio && enemy.ouch)
                enemy._audio.PlayOneShot(enemy.ouch);
            enemy.GetComponent<Animator>().SetTrigger(die);
        }
    }
}
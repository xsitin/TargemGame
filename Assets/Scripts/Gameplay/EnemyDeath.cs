using System.Linq;
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
        private static readonly int die = Animator.StringToHash("die");

        public override void Execute()
        {
            if (enemy._audio && enemy.ouch)
                enemy._audio.PlayOneShot(enemy.ouch);
            var animator = enemy.GetComponent<Animator>();
            if (animator.parameters.Any(x => x.nameHash == die))
                animator.SetTrigger(die);
            else
                enemy.Die();
        }
    }
}
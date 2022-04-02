using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    public class PlayerEnemyAttack : Event<PlayerEnemyAttack>
    {  
        public EnemyController enemy;

        private PlatformerModel model = GetModel<PlatformerModel>();
        public PlayerController player;
        public override void Execute()
        {
            var enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.Decrement();
                if (!enemyHealth.IsAlive)
                {
                    Schedule<EnemyDeath>().enemy = enemy;
                }
            }
            else
            {
                Schedule<EnemyDeath>().enemy = enemy;
            }
            Debug.Log($"hit {enemy.name}");
        }
    }
}
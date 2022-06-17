using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    public class PlayerEnemyAttack : Event<PlayerEnemyAttack>
    {
        public BaseEnemy BaseEnemy;
        public PlayerController Player;

        public override void Execute()
        {
            var enemyHealth = BaseEnemy.health;
            if (enemyHealth is { })
            {
                enemyHealth.Decrement();
                if (!enemyHealth.IsAlive)
                    Schedule<EnemyDeath>().enemy = BaseEnemy;
                else
                {
                    if (BaseEnemy._audio && BaseEnemy.ouch)
                        BaseEnemy._audio
                            .PlayOneShot(BaseEnemy
                                .ouch);
                    BaseEnemy.PushFromAttack(Player.transform.position);
                }
            }
            else
                Schedule<EnemyDeath>().enemy = BaseEnemy;

            Debug.Log($"hit {BaseEnemy.name}");
        }
    }
}
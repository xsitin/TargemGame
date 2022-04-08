using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    public class PlayerEnemyAttack : Event<PlayerEnemyAttack>
    {
        public EnemyController Enemy;
        public PlayerController Player;

        public override void Execute()
        {
            var enemyHealth = Enemy.health;
            if (enemyHealth != null)
            {
                enemyHealth.Decrement();
                if (!enemyHealth.IsAlive)
                    Schedule<EnemyDeath>().enemy = Enemy;
                else
                {
                    if (Enemy._audio && Enemy.ouch)
                        Enemy._audio
                            .PlayOneShot(Enemy
                                .ouch); //math.sign(Player.transform.position.x - Enemy.transform.position.x) * 10
                    Enemy.PushFromAttack(Player.transform.position);
                }
            }
            else
                Schedule<EnemyDeath>().enemy = Enemy;

            Debug.Log($"hit {Enemy.name}");
        }
    }
}
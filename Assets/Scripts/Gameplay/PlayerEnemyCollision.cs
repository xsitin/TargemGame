using Platformer.Mechanics;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    /// <summary>
    ///     Fired when a Player collides with an Enemy.
    /// </summary>
    /// <typeparam name="EnemyCollision"></typeparam>
    public class PlayerEnemyCollision : Event<PlayerEnemyCollision>
    {
        public EnemyController enemy;

        public PlayerController player;

        public override void Execute()
        {
            player.health.Decrement();
            if (!player.health.IsAlive) Schedule<HealthIsZero>();
        }
    }
}
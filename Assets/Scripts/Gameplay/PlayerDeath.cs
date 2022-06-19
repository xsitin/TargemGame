using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    ///     Fired when the player has died.
    /// </summary>
    /// <typeparam name="PlayerDeath"></typeparam>
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        private readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        private static readonly int die = Animator.StringToHash("die");

        public override void Execute()
        {
            var player = model.player;
            if (player.health.IsAlive)
            {
                player.health.Die();
            }
            // player.collider.enabled = false;
            player.controlEnabled = false;

            if (player.audioSource && player.ouchAudio)
                player.audioSource.PlayOneShot(player.ouchAudio);
            player.animator.SetTrigger(die);
            Simulation.Schedule<PlayerSpawn>(2);
        }
    }
}
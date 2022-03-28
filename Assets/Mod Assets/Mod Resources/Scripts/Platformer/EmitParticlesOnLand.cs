using Platformer.Gameplay;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EmitParticlesOnLand : MonoBehaviour
{
    public bool emitOnLand = true;
    public bool emitOnEnemyDeath = true;

#if UNITY_TEMPLATE_PLATFORMER

    private ParticleSystem p;

    private void Start()
    {
        p = GetComponent<ParticleSystem>();

        if (emitOnLand)
        {
            PlayerLanded.OnExecute += PlayerLanded_OnExecute;

            void PlayerLanded_OnExecute(PlayerLanded obj)
            {
                p.Play();
            }
        }

        if (emitOnEnemyDeath)
        {
            EnemyDeath.OnExecute += EnemyDeath_OnExecute;

            void EnemyDeath_OnExecute(EnemyDeath obj)
            {
                p.Play();
            }
        }
    }

#endif
}
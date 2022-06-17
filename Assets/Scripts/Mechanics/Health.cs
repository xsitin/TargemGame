using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    ///     Represebts the current vital statistics of some game entity.
    /// </summary>
    public class Health : MonoBehaviour
    {
        /// <summary>
        ///     The maximum hit points for the entity.
        /// </summary>
        public int maxHp = 1;

        private int currentHp;

        public int CurrentHp
        {
            get => currentHp;
            private set
            {
                if (value < currentHp)
                    animator.SetTrigger(value > 0 ? hurt : die);
                currentHp = value;
            }
        }

        /// <summary>
        ///     Indicates if the entity should be considered 'alive'.
        /// </summary>
        public bool IsAlive => currentHp > 0;

        private Animator animator;
        private static readonly int hurt = Animator.StringToHash("hurt");
        private static readonly int die = Animator.StringToHash("die");

        private void Awake()
        {
            CurrentHp = maxHp;
            animator = GetComponent<Animator>();
        }

        /// <summary>
        ///     Increment the HP of the entity.
        /// </summary>
        public void Increment()
        {
            CurrentHp = Mathf.Clamp(CurrentHp + 1, 0, maxHp);
        }

        public void Add(int value)
        {
            CurrentHp = Mathf.Clamp(CurrentHp + value, 0, maxHp);
        }

        /// <summary>
        ///     Decrement the HP of the entity. Will trigger a HealthIsZero event when
        ///     current HP reaches 0.
        /// </summary>
        public void Decrement()
        {
            CurrentHp = Mathf.Clamp(CurrentHp - 1, 0, maxHp);
        }

        /// <summary>
        ///     Decrement the HP of the entitiy until HP reaches 0.
        /// </summary>
        public void Die() => CurrentHp = 0;
    }
}
using System;
using System.Linq;
using Platformer.Gameplay;
using Platformer.Model;
using TMPro;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    ///     This is the main class used to implement control of the player.
    ///     It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }

        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        ///     Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;

        /// <summary>
        ///     Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;

        /*internal new*/
        public Collider2D collider2d;

        /*internal new*/
        public AudioSource audioSource;
        public LayerMask enemyLayer;

        public Health health;
        public float attackRange = 1;

        public bool controlEnabled = true;
        public Transform attackPoint;

        internal Animator animator;
        private readonly PlatformerModel model = GetModel<PlatformerModel>();
        private bool jump;

        private Vector2 move;
        private SpriteRenderer spriteRenderer;
        private bool stopJump;
        private bool attackPointFlipX = false;
        private static readonly int attack = Animator.StringToHash("attack");
        private static readonly int grounded = Animator.StringToHash("grounded");
        private static readonly int velocityX = Animator.StringToHash("velocityX");
        private static readonly int attacking = Animator.StringToHash("attacking");

        public bool Attacking => animator.GetBool(attacking);


        public Bounds Bounds => collider2d.bounds;

        private void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
                move.x = 0;

            UpdateJumpState();
            if (Input.GetKeyDown(KeyCode.G)) health.Decrement();
            if (Input.GetKeyDown(KeyCode.F))
            {
                Attack();
            }

            base.Update();
        }

        private void Attack()
        {
            animator.SetTrigger(attack);
            
            var attackpoint =
                new Vector2(attackPoint.position.x - (attackPointFlipX ? 0 : attackPoint.localPosition.x * 2),
                    attackPoint.position.y);
            var enemies = Physics2D.OverlapCircleAll(attackpoint, attackRange,
                enemyLayer);
            foreach (var enemy in enemies)
            {
                var enemyController = enemy.GetComponent<EnemyController>();

                Schedule<PlayerEnemyAttack>().enemy = enemyController;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (attackPoint is null)
                return;

            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        private void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }

                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }

                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0) velocity.y = velocity.y * model.jumpDeceleration;
            }

            if (move.x > 0.01f)
            {
                attackPointFlipX = false;
                spriteRenderer.flipX = false;
            }
            else if (move.x < -0.01f)
            {
                attackPointFlipX = true;
                spriteRenderer.flipX = true;
            }

            animator.SetBool(grounded, IsGrounded);
            animator.SetFloat(velocityX, Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }
    }
}
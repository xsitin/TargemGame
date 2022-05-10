using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Platformer.Gameplay;
using Platformer.Model;
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
        public float dashDistance;

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
        public float attackRange;
        public float dashTimeout;

        public bool controlEnabled = true;
        public Transform attackPoint;
        public float HookRange;
        private readonly HashSet<Collider2D> attackedEnemy = new();
        private readonly PlatformerModel model = GetModel<PlatformerModel>();

        internal Animator animator;
        private bool attackPointFlipX;
        private bool dashReady = true;
        private bool jump;

        private Vector2 move;
        private SpriteRenderer spriteRenderer;
        private bool stopJump;

        public bool Attacking => animator.GetBool(AnimatorObjects.Attacking);


        public Bounds Bounds => collider2d.bounds;

        private void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = FindObjectOfType<SoundController>().EffectsVolume;
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                var input = Input.GetAxis("Horizontal");
                move.x = Math.Abs(move.x) > Math.Abs(input) ? 0 : input;
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                {
                    jumpState = JumpState.PrepareToJump;
                }
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }

            UpdateJumpState();
            InputUpdate();
            if (attackedEnemy.Count > 0 && !Attacking) attackedEnemy.Clear();

            base.Update();
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint is not null)
                Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            Gizmos.DrawWireSphere(transform.position, HookRange);
        }

        private void InputUpdate()
        {
            if (Input.GetKeyDown(KeyCode.G)) Schedule<PlayerDeath>();
            if (Input.GetKeyDown(KeyCode.F)) Attack();
            if (Input.GetButtonDown("Dash") && dashReady) Dash();
            if (Input.GetButtonDown("Hook")) Hook();
        }

        private void Dash()
        {
            dashReady = false;
            var dashVector = attackPointFlipX ? Vector2.left : Vector2.right;
            dashVector *= 10;
            StartCoroutine(DashVelocity(dashVector));
            StartCoroutine(DashTimeout());
        }

        private IEnumerator DashVelocity(Vector2 dashVector)
        {
            var begin = transform.position.x;
            while (Math.Abs(transform.position.x - begin) < dashDistance &&
                   Physics2D.RaycastAll(
                       transform.position,
                       attackPointFlipX ? Vector2.left : Vector2.right, 1
                       , LayerMask.GetMask("Enemy", "Grid")).Length <= 0)
            {
                targetVelocity += dashVector;
                yield return null;
            }
        }

        private IEnumerator DashTimeout()
        {
            yield return new WaitForSeconds(dashTimeout);
            dashReady = true;
        }

        private void Hook()
        {
            var point = FindHookPoint();
            if (!point.HasValue)
                return;
            StartCoroutine(HookCoroutine(point.Value));
        }

        private IEnumerator HookCoroutine(Vector2 to)
        {
            while (Input.GetButton("Hook"))
            {
                var direction = (to - (Vector2)transform.position).normalized;
                velocity.y += direction.y;
                targetVelocity.x += direction.x * 10;
                yield return null;
            }
        }

        private Vector2? FindHookPoint()
        {
            var points = Physics2D.OverlapCircleAll(transform.position, HookRange, LayerMask.GetMask("HookPoints"));
            if (points.Length < 1)
                return null;

            var orderedByDistancePoints = points
                .Select(x => (x, Vector2.Distance(x.transform.position, transform.position)))
                .OrderBy(y => y.Item2).ToArray();
            if (orderedByDistancePoints.Length < 2 ||
                Math.Abs(orderedByDistancePoints[0].Item2 - orderedByDistancePoints[1].Item2) < 0.5f)
                return !(attackPointFlipX ^ (orderedByDistancePoints[0].x.transform.position.x <
                                             orderedByDistancePoints[1].x.transform.position.x))
                    ? orderedByDistancePoints[0].x.transform.position
                    : orderedByDistancePoints[1].x.transform.position;

            return orderedByDistancePoints[0].x.transform.position;
        }

        public void AttackedUpdate()
        {
            var flippedAttackPoint =
                new Vector2(attackPoint.position.x - (attackPointFlipX ? attackPoint.localPosition.x * 2 : 0),
                    attackPoint.position.y);
            var attackedEnemies = Physics2D.OverlapCircleAll(flippedAttackPoint, attackRange,
                enemyLayer);
            foreach (var enemy in attackedEnemies)
            {
                if (attackedEnemy.Contains(enemy)) continue;
                var enemyController = enemy.GetComponent<EnemyController>();
                var ev = Schedule<PlayerEnemyAttack>();
                ev.Enemy = enemyController;
                ev.Player = this;
                attackedEnemy.Add(enemy);
            }
        }

        private void Attack()
        {
            animator.SetTrigger(AnimatorObjects.Attack);
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
                if (velocity.y > 0) velocity.y *= model.jumpDeceleration;
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

            animator.SetBool(AnimatorObjects.Grounded, IsGrounded);
            animator.SetFloat(AnimatorObjects.VelocityX, Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity += move * maxSpeed;
            move.x = 0;
        }

        public static class AnimatorObjects
        {
            public static readonly int Attack = Animator.StringToHash("attack");
            public static readonly int Grounded = Animator.StringToHash("grounded");
            public static readonly int VelocityX = Animator.StringToHash("velocityX");
            public static readonly int Attacking = Animator.StringToHash("attacking");
        }
    }
}
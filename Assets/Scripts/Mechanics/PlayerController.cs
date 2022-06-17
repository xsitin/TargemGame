using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Platformer.Core;
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
    public class PlayerController : MonoBehaviour
    {
        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            Hooked,
            FixedOnHook,
            InFlight,
            Landed
        }

        public GameObject smokeBombPrefab;
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

        public Collider2D collider2d;

        public AudioSource audioSource;
        public LayerMask enemyLayer;

        public Health health;
        public float attackRange;
        public float dashTimeout;

        public bool controlEnabled = true;
        public Transform attackPoint;
        public float HookRange;


        public bool IsGrounded { get; set; }

        public bool Attacking => animator.GetBool(AnimatorObjects.Attacking);


        public Bounds Bounds => collider2d.bounds;
        private readonly HashSet<Collider2D> attackedEnemies = new();
        private readonly PlatformerModel model = GetModel<PlatformerModel>();
        private RaycastHit2D[] raycastResult = new RaycastHit2D[1];

        internal Animator animator;
        private bool attackPointFlipX;
        private bool dashReady = true;
        private bool jump;
        private DistanceJoint2D hookJoint;
        private LineRenderer hookLine;

        private SpriteRenderer spriteRenderer;
        private bool stopJump;
        private new Rigidbody2D rigidbody2D;
        private Vector2 flippedAttackPoint;
        private ContactFilter2D gridFilter;
        private IEnumerator HookCorutine;


        private void Start()
        {
            gridFilter = new ContactFilter2D() { layerMask = LayerMask.GetMask("Grid") };
            rigidbody2D = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = FindObjectOfType<SoundController>().EffectsVolume;
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            hookLine = gameObject.GetComponent<LineRenderer>();
        }

        protected void Update()
        {
            if (controlEnabled)
                PerformMovement();
            else
                rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
            PerformAdditionalAbilities();
            if (attackedEnemies.Count > 0 && !Attacking) attackedEnemies.Clear();
            if (hookJoint is not null)
            {
                var hookPoint = hookJoint.connectedBody.position;
                hookLine.positionCount = 2;
                hookLine.SetPositions(new[] { new Vector3(hookPoint.x, hookPoint.y, 1), transform.position });
            }

            flippedAttackPoint =
                new Vector2(attackPoint.position.x - (attackPointFlipX ? attackPoint.localPosition.x * 2 : 0),
                    attackPoint.position.y);
        }


        private void PerformMovement()
        {
            var input = Input.GetAxis("Horizontal") * maxSpeed;
            if (IsGrounded)

                rigidbody2D.velocity =
                    rigidbody2D.velocity.WithX(Math.Clamp(input, -maxSpeed,
                        maxSpeed));

            else if (Math.Abs(input) > Math.Abs(rigidbody2D.velocity.x) ||
                     Math.Sign(input) != Math.Sign(rigidbody2D.velocity.x))

                rigidbody2D.velocity =
                    rigidbody2D.velocity.WithX(Mathf.Clamp(input + rigidbody2D.velocity.x, -maxSpeed, maxSpeed));

            if (jumpState is JumpState.Grounded or JumpState.FixedOnHook && Input.GetButtonDown("Jump"))
            {
                jumpState = JumpState.PrepareToJump;
                HookCorutine?.MoveNext();
            }

            else if (Input.GetButtonUp("Jump"))
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }

        private void FixedUpdate()
        {
            var intersectionsCount = rigidbody2D.Cast(Vector2.down, gridFilter,
                raycastResult, 0.05f);
            IsGrounded = intersectionsCount > 0;

            UpdateJumpState();
            ComputeVelocity();
        }


        private void OnDrawGizmosSelected()
        {
            if (attackPoint is not null)
                Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            Gizmos.DrawWireSphere(transform.position, HookRange);
        }

        private void PerformAdditionalAbilities()
        {
            if (Input.GetKeyDown(KeyCode.G)) Schedule<PlayerDeath>();
            if (Input.GetKeyDown(KeyCode.F)) Attack();
            if (Input.GetButtonDown("Dash") && dashReady) Dash();
            if (Input.GetButtonDown("Hook")) Hook();
            if (Input.GetButtonDown("SmokeBomb")) ThrowSmokeBomb();
        }

        private void ThrowSmokeBomb()
        {
            var smokeBomb = Instantiate(smokeBombPrefab, flippedAttackPoint, Quaternion.identity);
            var rigidbody2D = smokeBomb.GetComponent<Rigidbody2D>();
            var force = attackPointFlipX ? Vector2.left : Vector2.right;
            force *= 10;
            rigidbody2D.AddForce(force, ForceMode2D.Impulse);
        }

        private void Dash()
        {
            dashReady = false;
            var dashVector = attackPointFlipX ? Vector2.left : Vector2.right;
            dashVector *= dashDistance;
            rigidbody2D.MovePosition(rigidbody2D.position + dashVector);
            if (dashTimeout > 0) StartCoroutine(DashTimeout());
            else dashReady = true;
        }

        private IEnumerator DashTimeout()
        {
            yield return new WaitForSeconds(dashTimeout);
            dashReady = true;
        }

        private void Hook()
        {
            if (jumpState == JumpState.FixedOnHook)
                return;
            var point = FindHookPoint();
            if (point == null)
                return;
            jumpState = JumpState.Hooked;
            var joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.distance = Vector2.Distance(point.transform.position, transform.position);
            joint.connectedBody = point.GetComponent<Rigidbody2D>();
            hookJoint = joint;
            HookCorutine = HookCoroutine(joint);
            StartCoroutine(HookCorutine);
        }

        private IEnumerator HookCoroutine(DistanceJoint2D joint)
        {
            while (jumpState is JumpState.Hooked or JumpState.FixedOnHook && Input.GetButton("Hook"))
            {
                joint.distance = Math.Clamp(joint.distance - 0.08f, 0f, 1000f);
                if (joint.distance <= 0.01)
                    jumpState = JumpState.FixedOnHook;
                yield return new WaitForFixedUpdate();
            }


            if (joint.distance > 0.1f)
            {
                var direction = joint.connectedBody.transform.position - transform.position;
                direction = direction.normalized;
                rigidbody2D.AddForce(direction * 7, ForceMode2D.Impulse);
            }

            hookJoint = null;
            hookLine.positionCount = 0;
            if (jumpState is JumpState.FixedOnHook or JumpState.Hooked)
                jumpState = JumpState.InFlight;
            Destroy(joint);
            HookCorutine = null;
        }

        private GameObject FindHookPoint()
        {
            var points = Physics2D.OverlapCircleAll(transform.position, HookRange, LayerMask.GetMask("HookPoints"));
            if (points.Length < 1)
                return null;

            var orderedByDistancePoints = points
                .Select(x =>
                {
                    var distance = Vector2.Distance(x.transform.position, transform.position);
                    if (attackPointFlipX && x.transform.position.x - transform.position.x > 0) distance += HookRange;
                    else if (!attackPointFlipX && x.transform.position.x - transform.position.x < 0)
                    {
                        distance += HookRange;
                    }

                    return (x, distance);
                })
                .OrderBy(y => y.Item2).ToArray();
            if (orderedByDistancePoints.Length >= 2 &&
                Math.Abs(orderedByDistancePoints[0].Item2 - orderedByDistancePoints[1].Item2) < 0.5f)
                return !(attackPointFlipX ^ (orderedByDistancePoints[0].x.transform.position.x <
                                             orderedByDistancePoints[1].x.transform.position.x))
                    ? orderedByDistancePoints[0].x.gameObject
                    : orderedByDistancePoints[1].x.gameObject;

            return orderedByDistancePoints[0].x.gameObject;
        }

        public void AttackedUpdate()
        {
            var overlapEnemies = Physics2D.OverlapCircleAll(flippedAttackPoint, attackRange,
                enemyLayer);
            foreach (var enemy in overlapEnemies)
            {
                if (attackedEnemies.Contains(enemy) || enemy.isTrigger) continue;
                var enemyController = enemy.GetComponent<BaseEnemy>();
                var ev = Schedule<PlayerEnemyAttack>();
                ev.BaseEnemy = enemyController;
                ev.Player = this;
                attackedEnemies.Add(enemy);
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
                    Schedule<PlayerJumped>().player = this;
                    jumpState = JumpState.InFlight;

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

        protected void ComputeVelocity()
        {
            if (jump && (IsGrounded || jumpState == JumpState.Jumping))
            {
                rigidbody2D.AddForce(new Vector2(0, jumpTakeOffSpeed * model.jumpModifier), ForceMode2D.Impulse);
                jump = false;
            }
            else if (stopJump) stopJump = false;

            switch (rigidbody2D.velocity.x)
            {
                case > 0.01f:
                    attackPointFlipX = false;
                    spriteRenderer.flipX = false;
                    break;
                case < -0.01f:
                    attackPointFlipX = true;
                    spriteRenderer.flipX = true;
                    break;
            }

            animator.SetBool(AnimatorObjects.Grounded, IsGrounded);
            animator.SetFloat(AnimatorObjects.VelocityX, Mathf.Abs(rigidbody2D.velocity.x) / maxSpeed);
        }

        public void Teleport(Vector2 position)
        {
            rigidbody2D.position = position;
            rigidbody2D.velocity = Vector2.zero;
        }

        public void AddForce(Vector2 force)
        {
            rigidbody2D.AddForce(force, ForceMode2D.Impulse);
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
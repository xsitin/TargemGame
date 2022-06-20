using System;
using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using Random = System.Random;

public class EnemyController : BaseEnemy
{
    public enum State
    {
        Patrol,
        Angry,
        Idle,
        Search
    }

    public State CurrentState = State.Patrol;

    public float maxSpeed => CurrentState == State.Angry ? runSpeed : walkSpeed;

    private Rigidbody2D rigidbody;
    private Animator animator;
    private static readonly int velocityX = Animator.StringToHash("VelocityX");
    public PatrolPath path;
    private Vector2 SpawnPoint;
    private Vector2 movingTo;
    private GameObject player;
    public Transform attackPoint;
    public float attackRange;
    private static readonly int attack = Animator.StringToHash("Attack");
    private static LayerMask playerLayer;
    private List<Collider2D> Attacked = new(2);
    public float walkSpeed;
    public float runSpeed;
    private float attackDistance;
    private static readonly Random random = new(DateTime.Now.Millisecond);
    private bool attacking = false;

    void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        SpawnPoint = (Vector2)transform.position;
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        movingTo = random.Next(0, 2) == 1 ? path.startPositionAbsolute : path.endPositionAbsolute;
        health = GetComponent<Health>();
        var randomCoordinate = ((float)random.Next(
            (int)(1000 * path.startPositionAbsolute.x),
            (int)(1000 * path.endPositionAbsolute.x))) / 1000;
        transform.position = new Vector3(randomCoordinate, rigidbody.transform.position.y,
            rigidbody.transform.position.z);
    }

    void Update()
    {
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (inSmoke)
            Wait();
        else
            switch (CurrentState)
            {
                case State.Patrol:
                    Patrol();
                    break;
                case State.Angry:
                    AttackPlayer();
                    break;
                case State.Search or State.Idle:
                    Wait();
                    StartCoroutine(BeginPatrol());
                    break;
            }
    }

    private IEnumerator BeginPatrol()
    {
        yield return new WaitForSeconds(3);
        CurrentState = State.Patrol;
        player = null;
        yield return null;
    }

    private void Wait()
    {
        animator.SetFloat(velocityX, 0);
        rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);
        if (!inSmoke && player is not null && Math.Abs(player.transform.position.x - transform.position.x) < 5)
            MoveTo(player.transform.position - transform.position);
    }

    private void AttackPlayer()
    {
        var distance = Math.Abs(player.transform.position.x - transform.position.x);
        if (distance > attackRange * 2) MoveTo(player.transform.position - transform.position);
        else
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (!attacking)
        {
            animator.SetTrigger(attack);
            attacking = true;
        }

        rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void AttackUpdate()
    {
        var attacked = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        for (var index = 0; index < attacked.Length; index++)
        {
            var player = attacked[index];
            if (Attacked.Contains(player)) continue;
            var playerController = player.GetComponent<PlayerController>();
            var ev = Simulation.Schedule<PlayerEnemyCollision>();
            ev.player = playerController;
            Attacked.Add(player);
        }
    }

    public void ClearAttacked()
    {
        Attacked.Clear();
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        attacking = false;
    }

    private void Patrol()
    {
        var distance = Math.Abs(transform.position.x - movingTo.x);
        if (distance < 0.01)
            movingTo = movingTo == path.startPositionAbsolute ? path.endPositionAbsolute : path.startPositionAbsolute;
        MoveTo(new Vector2(movingTo.x - transform.position.x, 0));
    }


    private void UpdateAnimator()
    {
        if (rigidbody.velocity.x > 0.01) transform.eulerAngles = Vector3.zero;

        if (rigidbody.velocity.x < -0.01) transform.eulerAngles = new Vector3(0, 180, 0);
    }

    private void MoveTo(Vector2 direction) //only to move on the ground
    {
        var input = (direction.x / Math.Abs(direction.x)) * maxSpeed;

        rigidbody.velocity =
            rigidbody.velocity.WithX(Math.Clamp(input, -maxSpeed,
                maxSpeed));
        animator.SetFloat(velocityX, Math.Abs(rigidbody.velocity.x));
    }

    private void OnTriggerStay2D(Collider2D other) => OnTriggerEnter2D(other);

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag(Tags.Player))
        {
            player = col.gameObject;
            CurrentState = State.Angry;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.CompareTag(Tags.Player)) CurrentState = State.Search;
    }

    public override void PushFromAttack(Vector2 position)
    {
    }

    private void OnDrawGizmos()
    {
        if (attackPoint is not null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
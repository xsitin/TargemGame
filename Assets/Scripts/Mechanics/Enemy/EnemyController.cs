using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
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
    private List<Collider2D> Attacked = new List<Collider2D>(2);
    public float walkSpeed;
    public float runSpeed;

    void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        SpawnPoint = (Vector2)transform.position;
        path.startPosition += SpawnPoint;
        path.endPosition += SpawnPoint;
        GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        GetComponent<BoxCollider2D>();
        movingTo = path.startPosition;
    }

    void Update()
    {
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (CurrentState == State.Patrol) Patrol();
        if (CurrentState == State.Angry) AttackPlayer();
        if (CurrentState is State.Search or State.Idle)
        {
            Wait();
            StartCoroutine(BeginPatrol());
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
        rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);
        if (player is not null && Math.Abs(player.transform.position.x - transform.position.x) < 5)
            MoveTo(player.transform.position - transform.position);
    }

    private void AttackPlayer()
    {
        var distance = Math.Abs(player.transform.position.x - transform.position.x);
        if (distance > 0.5) MoveTo(player.transform.position - transform.position);
        else Attack();
    }

    private void Attack()
    {
        animator.SetTrigger(attack);
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
    }

    private void Patrol()
    {
        var distance = Math.Abs(transform.position.x - movingTo.x);
        if (distance < 0.01)
            movingTo = movingTo == path.startPosition ? path.endPosition : path.startPosition;
        MoveTo(new Vector2(movingTo.x - transform.position.x, 0));
    }


    private void UpdateAnimator()
    {
        if (rigidbody.velocity.x > 0.01)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

        if (rigidbody.velocity.x < -0.01)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
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
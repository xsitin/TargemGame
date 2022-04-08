using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;

[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    /// <summary>
    ///     A simple controller for enemies. Provides movement control over a patrol path.
    /// </summary>
    public AudioClip ouch;

    public AudioSource _audio;
    public Collider2D _collider;
    public Rigidbody2D _rigidbody;
    public PatrolPath path;
    public float maxSpeed = 1f;

    private SpriteRenderer spriteRenderer;

    public Bounds Bounds => _collider.bounds;
    public Health health;
    internal PatrolPath.Mover mover;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _audio = GetComponent<AudioSource>();
        _rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();
    }


    public void PushFromAttack(Vector2 from)
    {
        var force = (Vector2)transform.position - from;
        _rigidbody.AddForce(force.normalized * 40);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            var ev = Schedule<PlayerEnemyCollision>();
            ev.player = player;
            ev.enemy = this;
        }
    }

    private void Update()
    {
        if (path == null) return;
        if (mover == null)
        {
            mover = path.CreateMover(maxSpeed * 0.5f);
            mover.transform = transform;
        }

        _rigidbody.velocity += mover.GetDelta;
    }
}
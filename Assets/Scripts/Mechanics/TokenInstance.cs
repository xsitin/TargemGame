using System;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;
using Random = UnityEngine.Random;


namespace Platformer.Mechanics
{
    /// <summary>
    ///     This class contains the data required for implementing token collection mechanics.
    ///     It does not perform animation of the token, this is handled in a batch by the
    ///     TokenController in the scene.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TokenInstance : MonoBehaviour
    {
        public AudioClip tokenCollectAudio;

        [Tooltip("If true, animation will start at a random position in the sequence.")]
        public bool randomAnimationStartTime;

        [Tooltip("List of frames that make up the animation.")]
        public Sprite[] idleAnimation, collectedAnimation;

        internal SpriteRenderer _renderer;
        internal bool collected;

        internal TokenController controller;

        //active frame in animation, updated by the controller.
        internal int frame;

        internal Sprite[] sprites = Array.Empty<Sprite>();

        //unique index which is assigned by the TokenController in a scene.
        internal int tokenIndex = -1;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (randomAnimationStartTime)
                frame = Random.Range(0, sprites.Length);
            sprites = idleAnimation;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //only exectue OnPlayerEnter if the player collides with this token.
            var player = other.gameObject.GetComponent<PlayerController>();
            if (player != null) OnPlayerEnter(player);
        }

        private void OnPlayerEnter(PlayerController player)
        {
            if (collected) return;
            //disable the gameObject and remove it from the controller update list.
            frame = 0;
            sprites = collectedAnimation;
            if (controller != null)
                collected = true;
            //send an event into the gameplay system to perform some behaviour.
            var ev = Schedule<PlayerTokenCollision>();
            ev.token = this;
            ev.player = player;
        }
    }
}
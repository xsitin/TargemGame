using System;
using UnityEngine;

namespace Platformer.Mechanics
{
    public abstract class BaseEnemy : MonoBehaviour
    {
        public String name { get; set; }
        public Health health { get; set; }
        public AudioSource _audio { get; set; }
        public AudioClip ouch { get; set; }
        public abstract void PushFromAttack(Vector2 position);
    }
}
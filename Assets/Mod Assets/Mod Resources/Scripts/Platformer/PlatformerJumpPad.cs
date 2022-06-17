using Platformer.Mechanics;
using UnityEngine;

public class PlatformerJumpPad : MonoBehaviour
{
    public float verticalVelocity;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var rb = other.attachedRigidbody;
        if (rb == null) return;
        var player = rb.GetComponent<PlayerController>();
        if (player == null) return;
        AddVelocity(player);
    }

    private void AddVelocity(PlayerController player)
    {
    }
}
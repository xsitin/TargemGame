using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarBehaviour : MonoBehaviour
{
    private Health health;
    private Animator animator;
    private int previous;
    private static readonly int hp = Animator.StringToHash("Hp");

    void Start()
    {
        health = FindObjectOfType<PlayerController>().health;
        animator = GetComponent<Animator>();
        animator.SetInteger("Hp",previous);
    }

    // Update is called once per frame
    void Update()
    {
        if (previous!=health.CurrentHp) animator.SetInteger(hp, health.CurrentHp);
        previous = health.CurrentHp;
    }
}
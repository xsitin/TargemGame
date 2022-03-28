using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarBehaviour : MonoBehaviour
{
    public Image healthBarInner;

    private Health health;

    void Start()
    {
        health = FindObjectOfType<PlayerController>().health;
    }

    // Update is called once per frame
    void Update() => healthBarInner.fillAmount = health.currentHP / (float) health.maxHP;
}
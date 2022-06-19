using System;
using System.Collections.Generic;
using Platformer.Mechanics;
using UnityEngine;

public class SmokeBombScript : MonoBehaviour
{
    private HashSet<BaseEnemy> inSmoke = new HashSet<BaseEnemy>();

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.isTrigger && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var enemy = other.gameObject.GetComponent<BaseEnemy>();
            enemy.inSmoke = true;
            inSmoke.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.isTrigger || other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
        var enemy = other.gameObject.GetComponent<BaseEnemy>();
        enemy.inSmoke = false;
        inSmoke.Remove(enemy);
    }

    private void OnDestroy()
    {
        foreach (var enemy in inSmoke) enemy.inSmoke = false;
        inSmoke.Clear();
    }
}
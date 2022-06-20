using System;
using System.Collections;
using System.Collections.Generic;
using Platformer.Mechanics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TargetController : BaseEnemy
{
    public List<string> Phrases = new();
    public GameObject textContainer;
    private bool dialogStarted = false;
    private TextMeshProUGUI text;

    private void OnTriggerEnter2D(Collider2D col)
    {
        text = textContainer.GetComponent<TextMeshProUGUI>();
        if (!dialogStarted && col.gameObject.name == "Player")
            RunDialog();
    }

    private void RunDialog()
    {
        dialogStarted = true;
        StartCoroutine(DialogCoroutine());
    }

    IEnumerator DialogCoroutine()
    {
        for (var i = 0; i < Phrases.Count; i++)
        {
            var phrase = Phrases[i];
            text.text = phrase;
            yield return new WaitForSeconds(3);
        }
    }

    public override void PushFromAttack(Vector2 position)
    {
    }

    private void OnDestroy() => SceneManager.LoadScene("MainMenu");
}
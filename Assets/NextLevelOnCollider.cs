using Platformer.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelOnCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag(Tags.Player))
        {
            SceneManager.LoadScene("SecondLevelReleaseV");
        }
    }
}
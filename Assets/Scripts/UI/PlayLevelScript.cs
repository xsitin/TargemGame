using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayLevelScript : MonoBehaviour
{
    public string LevelName;

    public void OnClick()
    {
        SceneManager.LoadScene(LevelName);
    }
}
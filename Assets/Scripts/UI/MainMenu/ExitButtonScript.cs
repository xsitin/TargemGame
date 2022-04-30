using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButtonScript : MonoBehaviour
{
    public void OnClick()
    {
        Application.Quit(0);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelScrollScript : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public int ScrollByClick;


    public void OnLeftButtonClick()
    {
        ScrollRect.horizontalNormalizedPosition -= (float)ScrollByClick / (ScrollRect.content.childCount - 3);
    }

    public void OnRightButtonClick()
    {
        ScrollRect.horizontalNormalizedPosition += (float)ScrollByClick / (ScrollRect.content.childCount - 3);
    }
}
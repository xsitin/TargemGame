using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCameraMover : MonoBehaviour
{
    public List<GameObject> Frames = new();
    public float speed;
    private float leftBroad, rightBroad;

    private void Start()
    {
        leftBroad = Frames[0].transform.position.x;
        rightBroad = Frames[^1].transform.position.x;
    }

    private void LateUpdate()
    {
        for (var i = 0; i < Frames.Count; i++)
        {
            var frame = Frames[i];
            var transform = frame.transform;
            transform.position += Vector3.right * speed;
            if (transform.position.x > rightBroad)
            {
                var position = transform.position;
                position = new Vector3(leftBroad, position.y, position.z);
                transform.position = position;
            }
            else if (transform.position.x < leftBroad)
            {
                var position = transform.position;
                position = new Vector3(rightBroad, position.y, position.z);
                transform.position = position;
            }
        }
    }
}
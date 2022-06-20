using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMover : MonoBehaviour

{
    public List<GameObject> Frames = new();
    public float speed;
    private float leftBroad, rightBroad;
    private Transform _camera;
    private int counter = 0;
    private Vector3 FrameSize;

    private void Awake()
    {
        _camera = Camera.main.transform;
    }

    private void Start()
    {
        leftBroad = Frames[0].transform.position.x;
        rightBroad = Frames[^1].transform.position.x;
        FrameSize = Frames[0].GetComponent<SpriteRenderer>().bounds.size;
    }

    private void LateUpdate()
    {
        for (var i = 0; i < Frames.Count; i++)
        {
            var frame = Frames[i];
            var transform = frame.transform;
            transform.position = _camera.transform.position * speed +
                                 counter * FrameSize.x * Vector3.right;
            if (transform.position.x - _camera.transform.position.x > FrameSize.x)
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
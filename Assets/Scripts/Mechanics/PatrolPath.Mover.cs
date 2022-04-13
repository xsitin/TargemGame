﻿using System;
using UnityEngine;

namespace Platformer.Mechanics
{
    public partial class PatrolPath
    {
        public class Mover
        {
            private float p;
            private bool toStartPosition;
            private readonly float speed;
            public Transform transform;
            private readonly Vector2 endPosition;
            private readonly Vector2 startPosition;

            public Mover(PatrolPath path, float speed)
            {
                var position = path.transform.position;
                startPosition = path.startPosition + (Vector2)position;
                endPosition = path.endPosition + (Vector2)position;
                this.speed = speed;
            }

            public Vector2 GetDelta
            {
                get
                {
                    if (toStartPosition)
                    {
                        if (transform.position.x - startPosition.x < float.Epsilon)
                        {
                            toStartPosition = false;
                            return ComputeVelocity();
                        }
                    }
                    else if (endPosition.x - transform.position.x < float.Epsilon)
                    {
                        toStartPosition = true;
                        return ComputeVelocity();
                    }

                    return ComputeVelocity() * Time.deltaTime;
                }
            }

            private Vector2 ComputeVelocity()
            {
                var move = toStartPosition ? Vector2.left : Vector2.right;
                move *= speed / 4f;
                return move;
            }
        }
    }
}
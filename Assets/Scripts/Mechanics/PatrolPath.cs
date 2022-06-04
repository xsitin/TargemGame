using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    ///     This component is used to create a patrol path, two points which enemies will move between.
    /// </summary>
    public partial class PatrolPath : MonoBehaviour
    {
        /// <summary>
        ///     One end of the patrol path.
        /// </summary>
        public Vector2 startPosition, endPosition;

        private void Reset()
        {
            startPosition = Vector3.left;
            endPosition = Vector3.right;
        }

        /// <summary>
        ///     Create a Mover instance which is used to move an entity along the path at a certain speed.
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public Mover CreateMover(float speed = 1)
        {
            if (startPosition.x > endPosition.x) 
                (startPosition, endPosition) = (endPosition, startPosition);

            return new Mover(this, speed);
        }
        public partial class Mover
        {
            private readonly Vector2 endPosition;
            private readonly float speed;
            private readonly Vector2 startPosition;
            private float p;
            private bool toStartPosition;
            public Transform transform;

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

                    return ComputeVelocity() * Time.fixedDeltaTime;
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
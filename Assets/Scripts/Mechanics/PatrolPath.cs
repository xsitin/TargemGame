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
    }
}
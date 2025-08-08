using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D),typeof(SpriteFlipper))]
public class PathMovement : MonoBehaviour
{
    private List<Vector3> currentPath;
    public int currentPathIndex;
    private Rigidbody2D rigidBody;
    private SpriteFlipper spriteFlipper;


    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteFlipper = GetComponent<SpriteFlipper>();
    }

    public void MoveAlongPath(double deltaTime, float moveSpeed)
    {
        if (currentPath == null || currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
            return;

        float distance = (currentPath[currentPathIndex] - transform.position).sqrMagnitude;

        if (distance < 0.001f)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.Count)
                return;
        }

        Vector2 direction = (currentPath[currentPathIndex] - transform.position).normalized;
        rigidBody.MovePosition(rigidBody.position + direction * moveSpeed * Time.fixedDeltaTime);
        spriteFlipper.FlipByDirection(direction);
    }

    public bool SetPathTo(Vector3 destination, AStarPathfinder pathFinder)
    {
        if (pathFinder == null)
            return false;
        currentPath = pathFinder.GetShortestWorldPath(transform.position, destination);
        currentPathIndex = 0;
        return currentPath != null && currentPath.Count > 0;
    }

    public bool UpdatePath(Vector3 destination, AStarPathfinder pathFinder)
    {
        if (pathFinder == null)
            return false;

        // get updated path starting from next step
        int nextIndex = currentPathIndex;

        // if there is no next step, get new path
        if (nextIndex >= currentPath.Count)
        {
            return SetPathTo(destination, pathFinder);
        }


        List<Vector3> newPath = pathFinder.GetShortestWorldPath(currentPath[nextIndex], destination);
        if (newPath == null || newPath.Count == 0)
            return false;

        newPath.Insert(0, currentPath[currentPathIndex]);

        currentPath = newPath;
        currentPathIndex = 0;

        return true;
    }

    public bool IsPathComplete()
    {
        return currentPath == null || currentPathIndex >= currentPath.Count;
    }

    private void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        for (int i = 0; i < currentPath.Count; ++i)
        {
            if (i == currentPath.Count - 1)
                break;

            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }
    }
}

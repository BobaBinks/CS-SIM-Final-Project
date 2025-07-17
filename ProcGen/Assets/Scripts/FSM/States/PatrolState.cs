using UnityEngine;
using System.Collections.Generic;
public class PatrolState : BaseState<EnemyAI>
{
    Transform currentWaypointTarget;
    int currentWaypointIndex;
    List<Vector3> path;
    int currentPathIndex;

    AStarPathfinder aStarPathfinder;

    public PatrolState(string stateId) : base(stateId)
    {
        if(GameManager.Instance.aStarPathfinder != null)
            aStarPathfinder = GameManager.Instance.aStarPathfinder;
        currentWaypointTarget = null;
    }

    public override void EnterState(EnemyAI owner)
    {

        owner.animator.Play("SkeletonWarriorMoving");
        Debug.Log("Entering Patrol State");
    }

    public override void ExitState(EnemyAI owner)
    {
        Debug.Log("Exiting Patrol State");
    }

    public override void UpdateState(EnemyAI owner, double deltaTime)
    {
        if (owner.HealthPoints < 0)
        {
            owner.Sm.SetNextState("death");
        }
        else if (owner.wayPoints == null || owner.wayPoints.Count == 0)
        {
            owner.Sm.SetNextState("idle");
        }
        else
        {
            // set the first waypoint
            if(currentWaypointTarget == null)
            {
                // set closest waypoint as first waypoint
                currentWaypointTarget = Helper.GetClosestTransform(owner.wayPoints, owner.transform.position);
                currentWaypointIndex = owner.wayPoints.FindIndex((waypoint) => { return waypoint == currentWaypointTarget; });

                if(currentWaypointTarget == null)
                {
                    owner.Sm.SetNextState("idle");
                    return;
                }

                // get initial path
                GetWaypointPath(owner, currentWaypointTarget.transform.position);
                return;
            }

            float distanceToTargetPath = (path[currentPathIndex] - owner.transform.position).sqrMagnitude;

            //  check if reach destination
            if (distanceToTargetPath < 0.001f)
            {
                // if reach end of path
                if(currentPathIndex == path.Count - 1)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % owner.wayPoints.Count;
                    currentWaypointTarget = owner.wayPoints[currentWaypointIndex];

                    // update new path
                    GetWaypointPath(owner, currentWaypointTarget.transform.position);
                    return;
                }
                else
                {
                    currentPathIndex++;
                }
            }
            else
            {
                Vector2 direction = (path[currentPathIndex] - owner.transform.position).normalized;
                owner.rigidBody.MovePosition(owner.rigidBody.position + direction * owner.MoveSpeed * Time.fixedDeltaTime);

                owner.spriteFlipper.FlipByDirection(direction);
            }
        }
    }


    private void GetWaypointPath(EnemyAI owner, Vector3 waypointPosition)
    {
        path = aStarPathfinder.GetShortestWorldPath(owner.transform.position, waypointPosition);

        if (path == null || path.Count == 0)
        {
            owner.Sm.SetNextState("idle");
            return;
        }
        owner._pathDebugList = path;
        currentPathIndex = 0;
    }
}

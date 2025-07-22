using UnityEngine;
using System.Collections.Generic;
public class PatrolState : BaseState<EnemyAI>
{
    Transform currentWaypointTarget;
    int currentWaypointIndex;
    AStarPathfinder aStarPathfinder;

    public PatrolState(string stateId) : base(stateId)
    {
        currentWaypointTarget = null;
    }

    public override void EnterState(EnemyAI owner)
    {
        if (GameManager.Instance.aStarPathfinder != null)
            aStarPathfinder = GameManager.Instance.aStarPathfinder;

        owner.animator.Play("SkeletonWarriorMoving");
        Debug.Log("Entering Patrol State");
    }

    public override void ExitState(EnemyAI owner)
    {
        Debug.Log("Exiting Patrol State");
    }

    public override void UpdateState(EnemyAI owner, double deltaTime)
    {
        if (owner.HealthPoints <= 0)
        {
            owner.Sm.SetNextState("death");
        }
        else if (owner.player != null && owner.PlayerInChaseRange())
        {
            owner.Sm.SetNextState("chase");
            return;
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
                if(!owner.pathMover.SetPathTo(currentWaypointTarget.transform.position, aStarPathfinder))
                {
                    owner.Sm.SetNextState("idle");
                }
            }

            //  check if reach destination
            else if (owner.pathMover.IsPathComplete())
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % owner.wayPoints.Count;
                currentWaypointTarget = owner.wayPoints[currentWaypointIndex];

                // update new path
                if(!owner.pathMover.SetPathTo(currentWaypointTarget.transform.position, aStarPathfinder))
                {
                    owner.Sm.SetNextState("idle");
                    return;
                }
            }
            else
            {
                owner.pathMover.MoveAlongPath(deltaTime, owner.MoveSpeed);
            }
        }
    }
}

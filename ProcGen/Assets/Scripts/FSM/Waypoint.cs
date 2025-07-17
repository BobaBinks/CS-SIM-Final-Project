using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public float gizmoRadius = 0.1f;

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }
}

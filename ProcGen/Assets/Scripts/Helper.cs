using UnityEngine;
using System.Collections.Generic;
public class Helper
{
    public static Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return child;
        }
        return null;
    }

    public static Transform GetClosestTransform(List<Transform> transforms,Vector3 position)
    {
        if (transforms == null || transforms.Count == 0)
            return null;

        Transform closestTransform = null;
        float closestSqrDistance = float.MaxValue;

        foreach (Transform transform in transforms)
        {
            if (transform == null)
                continue;

            float sqrDistance = (transform.position - position).sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestTransform = transform;
            }
        }

        return closestTransform;
    }
}

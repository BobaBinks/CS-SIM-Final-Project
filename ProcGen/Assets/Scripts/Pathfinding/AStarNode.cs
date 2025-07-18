using UnityEngine;

public class AStarNode
{
    public int g;   // cost from start to current node
    public int h;   // heuristic cost to goal
    public int f;   // total cost = g + h
    public Vector3Int position;
    public bool occupied;
    public AStarNode prevNode;

    public AStarNode(Vector3Int position, int g = int.MaxValue, int h = int.MaxValue, bool occupied = true)
    {
        this.position = position;
        this.g = g;
        this.h = h;
        this.f = g + h;
        this.occupied = occupied;
    }
}

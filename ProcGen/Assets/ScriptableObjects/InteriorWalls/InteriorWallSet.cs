using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InteriorWallSet", menuName = "Scriptable Objects/InteriorWallSet")]
public class InteriorWallSet : ScriptableObject
{
    [Header("Corner Walls")]
    public Tile topLeftCornerWall;
    public Tile topRightCornerWall;
    public Tile bottomLeftCornerWall;
    public Tile bottomRightCornerWall;

    [Header("Vertical Walls")]
    public Tile leftWall;
    public Tile rightWall;

    [Header("Horizontal Walls")]
    public Tile topWall;
    public Tile bottomWall;

    [Header("Empty Space")]
    public Tile emptySpace;

    [Header("Corner inverted walls")]
    // eg if the wall goes down and makes a right turn
    public Tile topLeftInvertedCornerWall;
    public Tile topRightInvertedCornerWall;
    public Tile bottomLeftInvertedCornerWall;
    public Tile bottomRightInvertedCornerWall;
}

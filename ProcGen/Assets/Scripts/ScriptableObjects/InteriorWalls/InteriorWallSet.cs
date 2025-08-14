using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InteriorWallSet", menuName = "Scriptable Objects/InteriorWallSet")]
public class InteriorWallSet : ScriptableObject
{
    public List<Tile> interiorWallTiles;

    public Tile topLeftCornerWall;
    public Tile topRightCornerWall;

    // need to create tiles for this by rotate the top corner walls
    public Tile bottomLeftCornerWall;
    public Tile bottomRightCornerWall;

    public Tile leftWall;
    public Tile rightWall;

    public Tile topWall;
    public Tile bottomWall;

    public Tile emptySpace;
}

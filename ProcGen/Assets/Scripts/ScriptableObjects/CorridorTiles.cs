using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "CorridorTiles", menuName = "Scriptable Objects/CorridorTiles")]
public class CorridorTiles : ScriptableObject
{
    public Tile corridorFloor;
    public Tile TopHorizontalWall;
    public Tile BottomHorizontalWall;

    public Tile LeftVerticalWall;
    public Tile RightVerticalWall;

    public Tile TopLeftCornerWall;
    public Tile TopRightCornerWall;

    public Tile BottomLeftCornerWall;
    public Tile BottomRightCornerWall;

    public Tile TurningBottomLeftCornerWall;
    public Tile TurningBottomRightCornerWall;

}

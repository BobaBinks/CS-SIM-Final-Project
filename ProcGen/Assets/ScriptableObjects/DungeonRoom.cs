using UnityEngine;
using System.Collections.Generic;
using System;
[CreateAssetMenu(fileName = "DungeonRoom", menuName = "Scriptable Objects/DungeonRoom")]
public class DungeonRoom : ScriptableObject
{
    public RoomTypes roomType;

    public float styleLeft;
    public float styleTop;
    public string guid;
    public List<DungeonRoom> connectionList = new List<DungeonRoom>();

    public Vector2 GetPosition()
    {
        return new Vector2(styleLeft, styleTop);
    }
}

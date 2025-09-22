using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomTypesList", menuName = "Scriptable Objects/RoomTypesList")]
public class RoomTypesList : ScriptableObject
{
    public List<RoomTypes> roomTypeList = new List<RoomTypes>();
}

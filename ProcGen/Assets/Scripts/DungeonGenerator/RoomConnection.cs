using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class RoomConnection
{
    DungeonRoom room1;
    DungeonRoom room2;

    public RoomConnection(DungeonRoom room1, DungeonRoom room2)
    {
        this.room1 = room1;
        this.room2 = room2;
    }

    public DungeonRoom GetRoom1()
    {
        return room1;
    }

    public DungeonRoom GetRoom2()
    {
        return room2;
    }

    public static bool CheckConnectionUnique(List<RoomConnection> connections, RoomConnection connection)
    {
        if(connections == null || connection == null || connection.room1 == null || connection.room2 == null)
            return false;

        foreach(RoomConnection con in connections)
        {
            if(ConnectionExist(con, connection)) return false;
        }
        return true;
    }

    private static bool ConnectionExist(RoomConnection a, RoomConnection b)
    {
        // checks the different possible orders of having the same rooms
        bool hasSameRooms = (a.room1 == b.room1 && a.room2 == b.room2) ||
                            (a.room2 == b.room1 && a.room1 == b.room2);

        // ensure room1 and room2 not the same
        bool bothRoomNotTheSame = a.room1 != a.room2;

        return hasSameRooms && bothRoomNotTheSame;
    }
}

using UnityEngine;
using System.Collections.Generic;
public class GraphRewriter
{
    GraphRewriteRuleList ruleList;

    public GraphRewriter(GraphRewriteRuleList ruleList)
    {
        this.ruleList = ruleList;
    }

    public bool RewriteGraph(DungeonLayout dungeonLayout, out List<DungeonRoom> modifiedGraph)
    {
        modifiedGraph = new List<DungeonRoom>();

        if (dungeonLayout == null ||
            dungeonLayout.dungeonRoomList == null ||
            dungeonLayout.dungeonRoomList.Count == 0 ||
            ruleList == null ||
            ruleList.rules == null ||
            ruleList.rules.Count == 0)
            return false;

        return ApplyMatchingRewriteRules(dungeonLayout, out modifiedGraph);
    }

    public List<DungeonRoom> CloneDungeonLayout(DungeonLayout dungeonLayout)
    {
        List<DungeonRoom> clonedLayout = new List<DungeonRoom>();

        if (dungeonLayout == null ||
            dungeonLayout.dungeonRoomList == null ||
            dungeonLayout.dungeonRoomList.Count == 0)
            return null;

        Dictionary<DungeonRoom, DungeonRoom> cloneMap = new Dictionary<DungeonRoom, DungeonRoom>();

        // clone rooms
        foreach (var room in dungeonLayout.dungeonRoomList)
        {
            DungeonRoom clonedRoom = ScriptableObject.Instantiate(room);
            cloneMap[room] = clonedRoom;
            clonedLayout.Add(clonedRoom);
        }

        foreach(var room in clonedLayout)
        {
            List<DungeonRoom> clonedConnections = new List<DungeonRoom>();

            foreach(var connection in room.connectionList)
            {
                if(cloneMap.ContainsKey(connection))
                    clonedConnections.Add(cloneMap[connection]);
            }

            room.connectionList = clonedConnections;
        }


        return clonedLayout;
    }

    public bool ApplyMatchingRewriteRules(DungeonLayout dungeonLayout, out List<DungeonRoom> modifiedGraph, float rewriteChance = 0.8f)
    {
        modifiedGraph = new List<DungeonRoom>();
        if (dungeonLayout == null ||
            dungeonLayout.dungeonRoomList == null ||
            dungeonLayout.dungeonRoomList.Count == 0)
            return false;

        // make a copy of the layout and dungeonrooms to avoid unintentionally modifying the scriptable objects
        List<DungeonRoom> clonedLayout = CloneDungeonLayout(dungeonLayout);
        if (clonedLayout == null)
            return false;

        modifiedGraph = clonedLayout;

        Queue<DungeonRoom> roomsToCheck = new Queue<DungeonRoom>();
        HashSet <DungeonRoom> roomsVisited = new HashSet<DungeonRoom>();
        List<GraphRewriteRule> matchingRules = new List<GraphRewriteRule>();

        // get entrance room
        DungeonRoom entranceRoom = clonedLayout.Find((x) => { return x.roomType.name == "EntranceRoomType"; });

        // checks if entrance room is present in the layout
        if (entranceRoom == null)
        {
            Debug.Log("GraphRewriter: Could not find entrance room in layout!");
            return false;
        }

        // start with entrance room
        roomsToCheck.Enqueue(entranceRoom);

        // go through every room and find matching patterns
        while (roomsToCheck.Count > 0)
        {
            DungeonRoom currRoom = roomsToCheck.Dequeue();

            if (roomsVisited.Contains(currRoom) || currRoom.connectionList == null)
                continue;

            roomsVisited.Add(currRoom);

            bool shouldRewrite = Random.Range(0f, 1f) < rewriteChance;
            if (!shouldRewrite)
                continue;

            // add neighbours to queue
            foreach (var connection in currRoom.connectionList)
            {
                if (!roomsVisited.Contains(connection))
                    roomsToCheck.Enqueue(connection);
            }

            // get a list of matching rules
            foreach (var rule in ruleList.rules)
            {
                RoomTypes currRoomType = currRoom.roomType;
                DungeonRoom firstRHSRoom = rule.RHS.dungeonRoomList[0];
                if (currRoomType == rule.LHS && 
                    currRoom.connectionList.Count + firstRHSRoom.connectionList.Count <= 4)
                {
                    matchingRules.Add(rule);
                }
            }

            // pick a random rule to use
            if (matchingRules.Count == 0)
                continue;

            int ruleIndex = Random.Range(0, matchingRules.Count);

            GraphRewriteRule selectedRule = matchingRules[ruleIndex];
            matchingRules.Clear();

            List<DungeonRoom> rhsClone = CloneDungeonLayout(selectedRule.RHS);

            if (rhsClone == null || rhsClone.Count == 0)
                return false;

            // reupdate the old connections with the new subgraph from selected rule
            foreach (var connection in currRoom.connectionList)
            {
                if(!roomsVisited.Contains(connection))
                    roomsToCheck.Enqueue(connection);

                int index = connection.connectionList.FindIndex(x => x == currRoom);

                // update the connections of the rooms that were connected to the LHS room
                if (index != -1)
                    connection.connectionList[index] = rhsClone[0];

                // update connections of first room in RHS
                if (!rhsClone[0].connectionList.Contains(connection))
                    rhsClone[0].connectionList.Add(connection);
            }

            clonedLayout.Remove(currRoom);
            clonedLayout.AddRange(rhsClone);
        }

        return true;
    }
}

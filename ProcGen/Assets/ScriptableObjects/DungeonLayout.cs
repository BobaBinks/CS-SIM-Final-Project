using UnityEngine;
using System.Collections.Generic;

# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif


[CreateAssetMenu(fileName = "DungeonLayout", menuName = "Scriptable Objects/DungeonLayout")]
public class DungeonLayout : ScriptableObject
{
    public List<DungeonRoom> dungeonRoomList = new List<DungeonRoom>();
    public RoomTypesList roomTypeList;
    public int width = 100;
    public int height = 100;
    public int minGapBetweenRooms = 5;

#if UNITY_EDITOR
    public List<NodeConnection> nodeConnections = new List<NodeConnection>();
    public List<NodeElement> nodeElements = new List<NodeElement>();

    /// <summary>
    /// Update the connection's line orientation
    /// </summary>
    /// <param name="node"></param>
    public void UpdateConnection(NodeElement node) 
    {
        if (node == null || nodeConnections == null) return;

        foreach (NodeConnection nodeConnection in nodeConnections)
        {
            if (nodeConnection.NodeInConnection(node))
            {
                nodeConnection.UpdateLine();
            }
        }
    }

    /// <summary>
    /// Adds connections between two nodes into adjacency list
    /// </summary>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    public void AddConnection(DungeonRoom room1, DungeonRoom room2, NodeElement element1, NodeElement element2, NodeConnectionElement line)
    {
        if(room1 == null || room2 == null) return;

        void Connect(DungeonRoom room1, DungeonRoom room2)
        {
            if (room1.connectionList.Contains(room2))
                return;
            room1.connectionList.Add(room2);

            // to update the Scriptable Object so it persists
            EditorUtility.SetDirty(room1);
        }

        // add the new connection for both room's perspective
        Connect(room1, room2);
        Connect(room2, room1);

        // add to unique node connection list
        if (nodeConnections == null)
            return;
        NodeConnection connection = new NodeConnection(element1, element2, line, this);
        nodeConnections.Add(connection);
    }

    public void RemoveConnection(NodeConnection connection)
    {
        if (nodeConnections == null || connection == null) return;
        KeyValuePair<NodeElement, NodeElement> kvp = connection.GetNodeElements();

        DungeonRoom room1 = kvp.Key.userData as DungeonRoom;
        DungeonRoom room2 = kvp.Value.userData as DungeonRoom;

        if (room1 != null && room1.connectionList != null)
        {
            room1.connectionList.Remove(room2);
            EditorUtility.SetDirty(room1);
        }

        if (room2 != null && room2.connectionList != null)
        {
            room2.connectionList.Remove(room1);
            EditorUtility.SetDirty(room2);
        }

        nodeConnections.Remove(connection);
        connection.GetLine().RemoveFromHierarchy();
    }

    /// <summary>
    /// Remove all connections that the node is associated with.
    /// </summary>
    /// <param name="node"></param>
    public void RemoveAllConnections(NodeElement node)
    {
        if (nodeConnections == null || node == null)
            return;
        List<NodeConnection> connectionsToRemove = new List<NodeConnection>();

        // iterate through the connection list
        foreach(NodeConnection connection in nodeConnections)
        {
            // check if the connection contains the node
            if(NodeConnection.CheckNodeInConnection(connection, node))
                connectionsToRemove.Add(connection);
                
        }

        foreach(NodeConnection connection in connectionsToRemove)
        {
            RemoveConnection(connection);
        }
    }


    /// <summary>
    /// Gets the node connection that contains both node elements in nodeConnections list
    /// </summary>
    /// <param name="node1"></param>
    /// <param name="node2"></param>
    /// <returns></returns>
    public NodeConnection GetNodeConnection(NodeElement node1, NodeElement node2)
    {
        if (nodeConnections != null)
        {
            foreach (NodeConnection nodeConnection in nodeConnections)
            {
                if (NodeConnection.CheckNodesInConnection(nodeConnection, node1, node2))
                {
                    return nodeConnection;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if a connection exists between both rooms
    /// </summary>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <returns></returns>
    public bool ConnectionExist(DungeonRoom room1, DungeonRoom room2)
    {
        if (room1 == null || room2 == null)
            return false;

        return room1.connectionList.Contains(room2);
    }

    /// <summary>
    /// Check if connection exist in the node element list
    /// </summary>
    /// <param name="node1"></param>
    /// <param name="node2"></param>
    /// <returns></returns>
    public bool NodeElementConnectionExist(NodeElement node1, NodeElement node2 = null)
    {
        if(nodeConnections != null)
        {
            foreach (NodeConnection nodeConnection in nodeConnections)
            {
                if (node2 != null && NodeConnection.CheckNodesInConnection(nodeConnection, node1, node2))
                    return true;

                if (NodeConnection.CheckNodeInConnection(nodeConnection, node1))
                    return true;
            }
        }

        return false;
    }


    // this opens the layout editor with this scriptable object
    //https://docs.unity3d.com/ScriptReference/Callbacks.OnOpenAssetAttribute.html
    //https://discussions.unity.com/t/is-it-possible-to-open-scriptableobjects-in-custom-editor-cindows-with-double-click/813843/2
    [OnOpenAssetAttribute(OnOpenAssetAttributeMode.Execute)]
    public static bool OnOpenAsset(int instanceID)
    {
        DungeonLayout layout = EditorUtility.InstanceIDToObject(instanceID) as DungeonLayout;

        if (layout != null)
        {
            DungeonLayoutEditor window = EditorWindow.GetWindow<DungeonLayoutEditor>();

            window.rootVisualElement.Clear();
            window.Init(layout);
            window.Show();
            return true;
        }

        return false;
    }
#endif
}

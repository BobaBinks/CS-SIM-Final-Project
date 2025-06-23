using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class NodeConnectionManipulator : PointerManipulator
{
    NodeConnectionElement line;
    NodeElement startNode;
    DungeonLayout layout;
    bool removeConnection;
    public NodeConnectionManipulator(NodeElement startNode, VisualElement root, DungeonLayout layout, bool removeConnection = false)
    {
        this.startNode = startNode;
        target = root;
        this.layout = layout;
        this.removeConnection = removeConnection;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
    }

    private void OnPointerDown(PointerDownEvent e)
    {
        if (layout == null) return;

        if(!removeConnection)
        {
            CreateConnection(e);
        }
        else
        {
            DeleteConnection(e);
        }
    }

    /// <summary>
    /// Create connection between start Node and Node in pointer down event
    /// </summary>
    /// <param name="e"></param>
    private void CreateConnection(PointerDownEvent e)
    {
        if (e.button == 1)
        {
            target.Remove(line);

            // remove manipulator
            target.RemoveManipulator(this);
            return;
        }

        // check if mouse hovering over node
        NodeElement hoveredNodeElement = target.panel.Pick(e.position) as NodeElement;

        // check if valid node for connection
        if (hoveredNodeElement != null && IsValidNodeForConnection(hoveredNodeElement, startNode))
        {
            DungeonRoom startRoom = startNode.userData as DungeonRoom;
            DungeonRoom endRoom = hoveredNodeElement.userData as DungeonRoom;

            // estab connection
            if (startRoom != null && endRoom != null)
            {
                layout.AddConnection(startRoom, endRoom, startNode, hoveredNodeElement, line);

                // just to update the alignment of the connection
                layout.UpdateConnection(hoveredNodeElement);
                VisualElement canvas = target.ElementAt(0);
                line.SendToBack();
                canvas.SendToBack();

                // remove manipulator
                target.RemoveManipulator(this);
            }
        }
    }

    /// <summary>
    /// Remove connection between start Node and Node in pointer down event
    /// </summary>
    /// <param name="e"></param>
    private void DeleteConnection(PointerDownEvent e)
    {
        if (e.button == 1)
        {
            // remove manipulator
            target.RemoveManipulator(this);
            return;
        }

        // check if mouse hovering over node
        NodeElement hoveredNodeElement = target.panel.Pick(e.position) as NodeElement;

        // check if valid node for connection
        if (hoveredNodeElement != null && IsBothNodesConnected(hoveredNodeElement, startNode))
        {
            DungeonRoom startRoom = startNode.userData as DungeonRoom;
            DungeonRoom endRoom = hoveredNodeElement.userData as DungeonRoom;

            // remove connnection in rooms
            if(RemoveConnectionInRoom(startRoom, endRoom) &&
                RemoveConnectionInRoom(endRoom, startRoom))
            {
                // get connection from layout
                NodeConnection nodeConnection = layout.GetNodeConnection(startNode, hoveredNodeElement);

                if (nodeConnection != null)
                {
                    // remove connection from the connection list in layout
                    layout.RemoveConnection(nodeConnection);

                    // remove NodeConnectionElement from root
                    target.Remove(nodeConnection.GetLine());
                }

                // remove manipulator
                target.RemoveManipulator(this);
            }
        }
    }

    /// <summary>
    /// Remove room2 from room1's connectionList
    /// </summary>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <returns></returns>
    private bool RemoveConnectionInRoom(DungeonRoom room1, DungeonRoom room2)
    {
        if(room1 != null && room1.connectionList != null)
        {
            room1.connectionList.Remove(room2);
            EditorUtility.SetDirty(room1);
            return true;
        }
        return false;
    }

    private void OnPointerMove(PointerMoveEvent e)
    {
        if(!removeConnection)
            DrawConnection(startNode.layout.center, e.position);
    }

    /// <summary>
    /// Check if node is a valid option for new connection
    /// </summary>
    /// <param name="element"></param>
    /// <param name="startNode"></param>
    /// <returns></returns>
    bool IsValidNodeForConnection(NodeElement element, NodeElement startNode)
    {
        if (element == null || startNode == null) return false;

        // get dungeon room from visual elements
        DungeonRoom startRoom = startNode.userData as DungeonRoom;
        DungeonRoom endRoom = element.userData as DungeonRoom;


        // check if dungeon room exist for both VisualElements
        if (startRoom == null || endRoom == null)
        {
            Debug.LogWarning("One of the visual element does not have a dungeon room");
            return false;
        }

        // ensure not connecting to self
        if (startRoom == endRoom)
        {
            Debug.LogWarning("Cannot connect node to self");
            return false;
        }

        // if connection exist return false, else return true
        return !layout.ConnectionExist(startRoom, endRoom);
    }

    bool IsBothNodesConnected(NodeElement element, NodeElement startNode)
    {
        if (element == null || startNode == null) return false;

        // get dungeon room from visual elements
        DungeonRoom startRoom = startNode.userData as DungeonRoom;
        DungeonRoom endRoom = element.userData as DungeonRoom;

        // check if dungeon room exist for both VisualElements
        if (startRoom == null || endRoom == null)
        {
            Debug.LogWarning("One of the visual element does not have a dungeon room");
            return false;
        }

        // ensure both elements are not the same
        if (startRoom == endRoom)
        {
            Debug.LogWarning("Cant Remove Connection, please select a different node");
            return false;
        }

        // make sure both rooms have a connection list
        if (startRoom.connectionList == null || endRoom.connectionList == null)
            return false;

        // check if both rooms have a connection with each other
        return startRoom.connectionList.Contains(endRoom) && endRoom.connectionList.Contains(startRoom);
    }

    void DrawConnection(Vector2 start, Vector2 end)
    {
        if (target == null) return;

        if (line == null)
        {
            line = new NodeConnectionElement(start,end);
            target.Add(line);
            


            line.MarkDirtyRepaint();
            return;
        }
        line.startPos = start;
        line.endPos = end;
        line.MarkDirtyRepaint();
    }
}
